using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Xml.Linq;
using System;

public class StreamHandler : MonoBehaviour
{
    IMeshManager iMeshManager;
    [SerializeField] public string BaseURL = "";
    [SerializeField] public string Manifest = "";

    GLTFast.GltfImport gltfImport;

    public int TotalLoadCount {get; private set;} = 0;
    public int CurrentLoadCount {get; private set;} = 0;
    public bool isLoaded { get; private set; } = false;


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
        XDocument xml = XDocument.Parse(mpdContent);
        XNamespace ns = "urn:mpeg:dash:schema:mpd:2011";
        var segmentURLs = xml.Descendants(ns + "SegmentURL");

        List<string> segments = new List<string>();
        foreach (var urlElement in segmentURLs)
        {
            segments.Add($"{BaseURL}/{urlElement.Attribute("media").Value}");
        }

        TotalLoadCount = segments.Count;
        Debug.Log($"[IMeshStreamer - Handler] Manifest parsed: {segments.Count} segments");
        
        LoadSegment(segments);
    }

    public async void LoadSegment(List<string> segments)
    {
        CurrentLoadCount = 0;
        isLoaded = false;

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

        isLoaded = true;
        Debug.Log("[IMeshStreamer - Handler] Segments loaded");
    }
}
