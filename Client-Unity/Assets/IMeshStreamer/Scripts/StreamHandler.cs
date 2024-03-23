using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Xml.Linq;
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.Video;

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
            case "model/gltf-binary":
                StartCoroutine(ParseGLBinary(xDocument));
                StartCoroutine(ParseMP4());
                break;

            default:
                break;
        }
    }

    IEnumerator ParseGLBinary(XDocument xDocument)
    {
        XNamespace ns = "urn:mpeg:dash:schema:mpd:2011";
        var segmentURLs = xDocument.Descendants(ns + "SegmentURL");

        List<string> segments = new List<string>();
        foreach (var urlElement in segmentURLs)
        {
            segments.Add($"{BaseURL}/{urlElement.Attribute("media").Value}");
        }

        TotalLoadCount = segments.Count;
        Debug.Log($"[IMeshStreamer - Handler] Manifest parsed: {segments.Count} segments");
        
        LoadSegment(segments);

        yield return null;
    }

    IEnumerator ParseMP4()
    {
        Debug.Log("[IMeshStreamer - Handler] Loading video");

        iMeshManager.streamContainer.InitVideoTexture($"{BaseURL}/stream.mp4");

        yield return null;
    }

    public IEnumerator VideoTextureOnReady()
    {
        while (TotalLoadCount < 0)
        {
            yield return null;
        }

        if (iMeshManager.streamContainer.VideoContainer.frameCount == (ulong)TotalLoadCount)
        {
            Debug.Log("[IMeshStreamer - Handler] Video Mesh Matched");
            isTextureLoaded = true;

            iMeshManager.streamPlayer.Play();
        }
        else
        {
            Debug.LogError("[IMeshStreamer - Handler] Video Mesh Mismatch");
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
                iMeshManager.streamContainer.LoadMaterial(gltfImport.GetMaterial());

                CurrentLoadCount++;
            }
        }

        isMeshLoaded = true;
        Debug.Log("[IMeshStreamer - Handler] Segments loaded");
    }

    void OnDestory()
    {
        iMeshManager.streamContainer.Clear();
    }
}
