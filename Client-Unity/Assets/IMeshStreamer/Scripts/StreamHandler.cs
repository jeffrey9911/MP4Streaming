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
using GLTFast;
using UnityEngine.UI;

public class StreamHandler : MonoBehaviour
{
    IMeshManager iMeshManager;
    [SerializeField] public string BaseURL = "";
    [SerializeField] public string Manifest = "";

    [SerializeField] private InputField inputField;

    GLTFast.GltfImport gltfImport;

    public int TotalLoadCount {get; private set;} = -1;
    public int CurrentLoadCount {get; private set;} = 0;

    public bool isMeshLoaded { get; private set; } = false;
    public bool isTextureLoaded { get; private set; } = false;


    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    void Start()
    {
        
        
    }

    public void LoadOnClick()
    {
        BaseURL = $"http://{inputField.text}/video";
        Manifest = "stream.mpd";
        StartLoad();
    }


    [ContextMenu("StartLoad")]
    public void StartLoad()
    {
        try
        {
            stopwatch.Start();
            if(transform.TryGetComponent<IMeshManager>(out iMeshManager))
            {
                gltfImport = new GLTFast.GltfImport();
                StartCoroutine(FetchManifest());
            }
            else
            {
                iMeshManager.Debug("[IMeshStreamer - Handler] No IMeshManager found");
            }
        }
        catch (Exception e)
        {
            iMeshManager.Debug($"[Start()]: {e} : {e.Message} : {e.StackTrace}");

        }
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
            Debug.Log($"BaseURL: {BaseURL} Manifest: {Manifest}");
            using (UnityWebRequest webRequest = UnityWebRequest.Get($"{BaseURL}/{Manifest}"))
            {
                yield return webRequest.SendWebRequest();

                try
                {
                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError(webRequest.error);
                    }
                    else
                    {
                        Debug.Log("[IMeshStreamer - Handler] Manifest fetched");
                        ParseManifest(webRequest.downloadHandler.text);
                    }
                }
                catch (Exception e)
                {
                    iMeshManager.Debug($"[FetchManifest()]: {e} : {e.Message} : {e.StackTrace}");
                }
            }
        }
    }

    void ParseManifest(string mpdContent)
    {
        try
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
        catch (Exception e)
        {
            iMeshManager.Debug($"[ParseManifest()]: {e} : {e.Message} : {e.StackTrace}");
        }
    }

    IEnumerator ParseGLBinary(XDocument xDocument)
    {
        try
        {
            XNamespace ns = "urn:mpeg:dash:schema:mpd:2011";
            var segmentURLs = xDocument.Descendants(ns + "GLBURL");

            List<string> segments = new List<string>();
            foreach (var urlElement in segmentURLs)
            {
                segments.Add($"{BaseURL}/{urlElement.Attribute("media").Value}");
            }

            TotalLoadCount = segments.Count;
            iMeshManager.Debug($"[IMeshStreamer - Handler] Manifest parsed: {segments.Count} segments");
            
            LoadSegment(segments);
        }
        catch (Exception e)
        {
            iMeshManager.Debug($"[ParseGLBinary()]: {e} : {e.Message} : {e.StackTrace}");
        }

        yield return null;
    }

    IEnumerator ParseMP4(XDocument xDocument)
    {
        try
        {
            Debug.Log("[IMeshStreamer - Handler] Loading video");

            XNamespace ns = "urn:mpeg:dash:schema:mpd:2011";
            var segmentURLs = xDocument.Descendants(ns + "VAURL");

            if (segmentURLs.Count() > 0)
            {
                iMeshManager.streamContainer.InitVideoTexture($"{BaseURL}/{segmentURLs.First().Attribute("media").Value}");
            }
        }
        catch (Exception e)
        {
            iMeshManager.Debug($"[ParseMP4()]: {e} : {e.Message} : {e.StackTrace}");
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
            try
            {
                stopwatch.Stop();
                iMeshManager.Debug($"[IMeshStreamer - Handler] Video Mesh Loaded in {stopwatch.ElapsedMilliseconds}ms");
                iMeshManager.Debug("[IMeshStreamer - Handler] Video Mesh Matched");

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
            catch (Exception e)
            {
                iMeshManager.Debug($"[VideoTextureOnReady()]: {e} : {e.Message} : {e.StackTrace}");
            }
        }
        else
        {
            iMeshManager.Debug($"[IMeshStreamer - Handler] Video Mesh Mismatch - GLB: {TotalLoadCount} MP4: {iMeshManager.streamContainer.VideoContainer.frameCount.ToString()}");
            isTextureLoaded = false;
        }
    }

    string ParseMimeType(XDocument xDocument)
    {
        XNamespace ns = "urn:mpeg:dash:schema:mpd:2011";
        string mimeType = "";

        try
        {
            mimeType = xDocument.Descendants(ns + "Representation")
                .FirstOrDefault()?
                .Attribute("mimeType")?.Value;
        }
        catch (Exception e)
        {
            iMeshManager.Debug(e.Message);
        }

        return mimeType;
    }

    void InitPlayer(XDocument xDocument)
    {
        try
        {
            XNamespace ns = "urn:mpeg:dash:schema:mpd:2011";
            var segmentURLs = xDocument.Descendants(ns + "SEGINFO");

            if (segmentURLs.Count() > 0)
            {
                int overidedFrameRate = int.Parse(segmentURLs.First().Attribute("fps").Value);
                iMeshManager.streamPlayer.TargetFPS = overidedFrameRate;
            }
        }
        catch (Exception e)
        {
            iMeshManager.Debug($"[InitPlayer()]: {e} : {e.Message} : {e.StackTrace}");
        }
    }

    //GLTFast.ImportSettings importSettings = new GLTFast.ImportSettings();

    public async void LoadSegment(List<string> segments)
    {
        try
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
                        Material material = gltfImport.GetMaterial();
                        
                        if (material != null)
                        {
                            iMeshManager.streamPlayer.InitMaterial(material);
                        }
                        else
                        {
                            iMeshManager.Debug("[IMeshStreamer - Handler] Material not found");
                        }
                    }
                    

                    CurrentLoadCount++;
                }
            }

            isMeshLoaded = true;
            iMeshManager.Debug($"[IMeshStreamer - Handler] Segments loaded: {CurrentLoadCount}");
        }
        catch (Exception e)
        {
            iMeshManager.Debug($"[LoadSegment()]: {e} : {e.Message} : {e.StackTrace}");
        }
    }

    // Dispose gltfImport when quitting
    void OnApplicationQuit()
    {
        try
        {
            iMeshManager.streamContainer.Clear();
            gltfImport.Dispose();
        }
        catch (Exception e)
        {
            iMeshManager.Debug($"[OnApplicationQuit()]: {e} : {e.Message} : {e.StackTrace}");
        }
    }

    
}
