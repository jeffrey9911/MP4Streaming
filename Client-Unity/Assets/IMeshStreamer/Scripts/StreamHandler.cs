using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Xml.Linq;
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.Video;
using UnityEngine.UIElements;

public class StreamHandler : MonoBehaviour
{
    IMeshManager iMeshManager;
    [SerializeField] public string BaseURL = "";
    [SerializeField] public string Manifest = "";

    GLTFast.GltfImport gltfImport;

    public int TotalLoadCount {get; private set;} = -1;
    public int CurrentLoadCount {get; private set;} = 0;

    public bool isMeshLoaded { get; private set; } = false;
    public bool isTextureLoaded { get; private set; } = false;

    public void SetManifestURL(string baseUrl, string name)
    {
        BaseURL = baseUrl;
        Manifest = name;
    }

    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    void Start()
    {
        
        
        if(transform.TryGetComponent<IMeshManager>(out iMeshManager))
        {
            gltfImport = new GLTFast.GltfImport();
            StartCoroutine(FetchManifest());
        }
        else
        {
            Debug.LogError("[IMeshStreamer - Handler] No IMeshManager found");
            iMeshManager.iMeshDebugger.Debug("[IMeshStreamer - Handler] No IMeshManager found");
        }

        iMeshManager.iMeshDebugger.Debug("[IMeshStreamer - Handler] StreamHandler started");
        stopwatch.Start();
        
    }

    IEnumerator FetchManifest()
    {
        if (BaseURL == "" || Manifest == "")
        {
            Debug.LogError("URL is not set");
            yield break;
        }
        else
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get($"{BaseURL}/{Manifest}"))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(webRequest.error);
                }
                else
                {
                    Debug.Log("[IMeshStreamer - Handler] Manifest fetched");
                    iMeshManager.iMeshDebugger.Debug("[IMeshStreamer - Handler] Manifest fetched");
                    ParseManifest(webRequest.downloadHandler.text);
                }
            }
        }
    }

    void ParseManifest(string mpdContent)
    {
        XDocument xDocument = XDocument.Parse(mpdContent);
        string mimeType = ParseMimeType(xDocument);

        switch (mimeType)
        {
            case "video/volumetric-video":
                InitPlayer(xDocument);
                StartCoroutine(ParseGLBinary(xDocument));
                StartCoroutine(ParseMP4(xDocument));
                break;

            default:
                break;
        }
    }

    IEnumerator ParseGLBinary(XDocument xDocument)
    {
        XNamespace ns = "urn:mpeg:dash:schema:mpd:2011";
        var segmentURLs = xDocument.Descendants(ns + "GLBURL");

        List<string> segments = new List<string>();
        foreach (var urlElement in segmentURLs)
        {
            segments.Add($"{BaseURL}/{urlElement.Attribute("media").Value}");
        }

        TotalLoadCount = segments.Count;
        Debug.Log($"[IMeshStreamer - Handler] Manifest parsed: {segments.Count} segments");
        iMeshManager.iMeshDebugger.Debug($"[IMeshStreamer - Handler] Manifest parsed: {segments.Count} segments");
        
        LoadSegment(segments);

        yield return null;
    }

    IEnumerator ParseMP4(XDocument xDocument)
    {
        Debug.Log("[IMeshStreamer - Handler] Loading video");
        iMeshManager.iMeshDebugger.Debug("[IMeshStreamer - Handler] Loading video");

        XNamespace ns = "urn:mpeg:dash:schema:mpd:2011";
        var segmentURLs = xDocument.Descendants(ns + "VAURL");

        if (segmentURLs.Count() > 0)
        {
            iMeshManager.streamContainer.InitVideoTexture($"{BaseURL}/{segmentURLs.First().Attribute("media").Value}");
        }

        yield return null;
    }

    public IEnumerator VideoTextureOnReady()
    {
        while (TotalLoadCount < 0)
        {
            yield return null;
        }

        if (iMeshManager.streamContainer.VideoContainer.frameCount >= (ulong)TotalLoadCount)
        {
            stopwatch.Stop();
            Debug.Log($"[IMeshStreamer - Handler] Video Mesh Loaded in {stopwatch.ElapsedMilliseconds}ms");
            iMeshManager.iMeshDebugger.Debug($"[IMeshStreamer - Handler] Video Mesh Loaded in {stopwatch.ElapsedMilliseconds}ms");
            Debug.Log("[IMeshStreamer - Handler] Video Mesh Matched");
            iMeshManager.iMeshDebugger.Debug("[IMeshStreamer - Handler] Video Mesh Matched");

            /*
            for (ulong i = 0; i < iMeshManager.streamContainer.VideoContainer.frameCount; i++)
            {
                iMeshManager.streamContainer.VideoContainer.frame = (long)i;

                Texture2D tex = new Texture2D(iMeshManager.streamContainer.VideoContainer.texture.width, iMeshManager.streamContainer.VideoContainer.texture.height);
                RenderTexture.active = iMeshManager.streamContainer.VideoContainer.texture as RenderTexture;
                tex.ReadPixels(new Rect(0, 0, iMeshManager.streamContainer.VideoContainer.texture.width, iMeshManager.streamContainer.VideoContainer.texture.height), 0, 0);
                tex.Apply();
                RenderTexture.active = null;
                iMeshManager.streamContainer.LoadTexture(tex);



                yield return null;
            }
            */

            //yield return new WaitForSeconds(0.5f);
            //iMeshManager.streamPlayer.Play();

            isTextureLoaded = true;
        }
        else
        {
            Debug.LogError($"[IMeshStreamer - Handler] Video Mesh Mismatch - GLB: {TotalLoadCount} MP4: {iMeshManager.streamContainer.VideoContainer.frameCount.ToString()}");
            iMeshManager.iMeshDebugger.Debug($"[IMeshStreamer - Handler] Video Mesh Mismatch - GLB: {TotalLoadCount} MP4: {iMeshManager.streamContainer.VideoContainer.frameCount.ToString()}");
            isTextureLoaded = false;
        }

    }

    string ParseMimeType(XDocument xDocument)
    {
        XNamespace ns = "urn:mpeg:dash:schema:mpd:2011";

        string mimeType = xDocument.Descendants(ns + "Representation")
            .FirstOrDefault()?
            .Attribute("mimeType")?.Value;

        return mimeType;
    }

    void InitPlayer(XDocument xDocument)
    {
        XNamespace ns = "urn:mpeg:dash:schema:mpd:2011";
        var segmentURLs = xDocument.Descendants(ns + "SEGINFO");

        if (segmentURLs.Count() > 0)
        {
            int overidedFrameRate = int.Parse(segmentURLs.First().Attribute("fps").Value);
            iMeshManager.streamPlayer.TargetFPS = overidedFrameRate;
        }
    }

    public async void LoadSegment(List<string> segments)
    {
        CurrentLoadCount = 0;
        isMeshLoaded = false;

        foreach (var segment in segments)
        {
            gltfImport = new GLTFast.GltfImport();
            var success = await gltfImport.Load(new Uri(segment));
            if (success)
            {
                iMeshManager.streamContainer.LoadMesh(gltfImport.GetMeshes()[0]);

                if (CurrentLoadCount == 0)
                {
                    iMeshManager.streamPlayer.InitMaterial(gltfImport.GetMaterial());
                }
                

                CurrentLoadCount++;
            }
        }

        isMeshLoaded = true;
        Debug.Log("[IMeshStreamer - Handler] Segments loaded");
    }

    // Dispose gltfImport when quitting
    void OnApplicationQuit()
    {
        iMeshManager.streamContainer.Clear();
        gltfImport.Dispose();
    }

    
}
