using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SampleScript : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    
    public RawImage rawImage;

    RenderTexture renderTexture;

    public List<RenderTexture> Textures = new List<RenderTexture>();

    public GameObject cube;

    bool flag = false;

    int index = 0;

    void Start()
    {
        renderTexture = new RenderTexture(2048, 2048, 24);
        videoPlayer.url = "http://192.168.2.77:8000/video/stream.mp4";
        videoPlayer.playOnAwake = false;
        videoPlayer.targetTexture = renderTexture;
        rawImage.texture = renderTexture;

        
        videoPlayer.prepareCompleted += VideoPrepared;
        //videoPlayer.frameReady += OnFrameReady;
        videoPlayer.Prepare();
    }

    void Update()
    {
        if (flag)
        {
            cube.GetComponent<Renderer>().material.mainTexture = Textures[40];
            /*
            Debug.Log("Texture at index: " + index + " is ready");
            index++;
            if (index >= Textures.Count)
            {
                index = 0;
            }
            */
        }
    }

    void VideoPrepared(VideoPlayer vp)
    {
        vp.Play();
        StartCoroutine(LoadVideo(vp));
    }

    IEnumerator LoadVideo(VideoPlayer vp)
    {
        Debug.Log("Video prepared");
        vp.frame = 40;
        Debug.Log(vp.frameCount);
        Debug.Log(vp.frame);
        Debug.Log(vp.frameRate);

        for (int i = 0; i < (int)vp.frameCount; i++)
        {
            vp.frame = i;

            RenderTexture RT = new RenderTexture(vp.targetTexture.width, vp.targetTexture.height, vp.targetTexture.depth, vp.targetTexture.format);
            Graphics.Blit(vp.targetTexture, RT);
/*
            Texture2D frameTexture = new Texture2D(renderTexture.width, renderTexture.height);
            RenderTexture.active = renderTexture;
            frameTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            frameTexture.Apply();
            Textures.Add(frameTexture);
*/
            Textures.Add(RT);

            Debug.Log("Frame at index: " + i + " is ready");

            yield return null;
        }

        flag = true;
    }

    void OnFrameReady(VideoPlayer vp, long frameIdx)
    {
        // This is called for each frame.
        Debug.Log("Frame at index: " + frameIdx + " is ready");

        // Example operation: Copy the current frame into a Texture2D
        Texture2D frameTexture = new Texture2D(renderTexture.width, renderTexture.height);
        RenderTexture.active = renderTexture;
        frameTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        frameTexture.Apply();
        RenderTexture.active = null; // Reset the active RenderTexture

        // Now you have the frame in frameTexture, you can process/display it as needed
        // Note: Handling every single frame like this can be very performance-intensive,
        // especially for high-resolution and high-frame-rate videos.
    }
}
