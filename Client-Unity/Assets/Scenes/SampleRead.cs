using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Xml.Linq;

public class SampleRead : MonoBehaviour
{

    [SerializeField] string baseUrl = "";
    [SerializeField] string mpdFile = "";


    private string loadedMPD = "";

    bool isLoaded {get; set;} = false;

    public SampleLoad sampleLoad;

    IEnumerator Start()
    {
        yield return StartCoroutine(FetchMPD($"{baseUrl}/{mpdFile}"));
    }

    IEnumerator FetchMPD(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if(webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(webRequest.error);
            }
            else
            {
                loadedMPD = webRequest.downloadHandler.text;
                Debug.Log(loadedMPD);
            }
        }
    }

    IEnumerator ParseMPD(string mpdContent)
    {
        XDocument mpdDocument = XDocument.Parse(mpdContent);

        XNamespace ns = "urn:mpeg:dash:schema:mpd:2011";

        var segmentURLs = mpdDocument.Descendants(ns + "SegmentURL");

        foreach (var urlElement in segmentURLs)
        {
            sampleLoad.segments.Add($"{baseUrl}/{urlElement.Attribute("media").Value}");
            yield return urlElement.Attribute("media").Value;
        }

        foreach (var segment in sampleLoad.segments)
        {
            Debug.Log(segment);
        }

        isLoaded = true;
        sampleLoad.TriggerLoad();
    }

    [ContextMenu("StartParse")]
    public void StartParseAsync()
    {
        StartCoroutine(ParseMPD(loadedMPD));
    }

    void RenderFrame()
    {
        
    }
}
