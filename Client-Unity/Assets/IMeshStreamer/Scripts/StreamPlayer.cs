using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreamPlayer : MonoBehaviour
{
    IMeshManager iMeshManager;

    int CurrentFrameIndex = 0;
    int BufferingThreshold = 3;

    bool isPlaying = false;
    bool isBuffering = false;

    public int TargetFPS = 30;
    float FrameTimer = 0;

    GameObject PlayerInstance;
    MeshFilter PlayerInstanceMesh;
    MeshRenderer PlayerInstanceRenderer;
    Material PlayerInstanceMaterial;

    void Start()
    {
        if (!transform.TryGetComponent<IMeshManager>(out iMeshManager))
        {
            Debug.LogError("[IMeshStreamer - Player] No IMeshManager found");
        }
    }

    void Update()
    {
        if(iMeshManager.streamHandler.isTextureLoaded)
        {
            FramePlay();
        }
    }

    [ContextMenu("Play")]
    public void Play()
    {
        isPlaying = true;

        PlayerInstance = new GameObject("PlayerInstance");
        PlayerInstance.transform.SetParent(this.transform);
        PlayerInstance.transform.localPosition = Vector3.zero;
        PlayerInstance.transform.localRotation = Quaternion.Euler(new Vector3(90f, 0, 0));
        PlayerInstanceMesh = PlayerInstance.AddComponent<MeshFilter>();
        PlayerInstanceRenderer = PlayerInstance.AddComponent<MeshRenderer>();
        PlayerInstanceMaterial = new Material(Shader.Find("Standard"));
        PlayerInstanceMaterial.SetTexture("_MainTex", iMeshManager.streamContainer.VideoTexture);
        PlayerInstanceMaterial.SetFloat("_Glossiness", 0);
        PlayerInstanceRenderer.material = PlayerInstanceMaterial;
    }



    void SwapFrame(bool isReverse = false)
    {
        if (isReverse)
        {
            CurrentFrameIndex = (CurrentFrameIndex - 1) < 0 ? iMeshManager.streamHandler.TotalLoadCount - 1 : CurrentFrameIndex - 1;
        }
        else
        {
            CurrentFrameIndex = (CurrentFrameIndex + 1) >= iMeshManager.streamHandler.TotalLoadCount ? 0 : CurrentFrameIndex + 1;
        }


        PlayerInstanceMesh.mesh = iMeshManager.streamContainer.Meshes[CurrentFrameIndex];
        iMeshManager.streamContainer.VideoContainer.frame = CurrentFrameIndex;
        //PlayerInstanceRenderer.material = iMeshManager.streamContainer.Materials[CurrentFrameIndex];
        //Debug.Log("[IMeshStreamer - Player] Swapping to frame " + CurrentFrameIndex);
    }

    void BufferingPlay(bool isReverse = false)
    {
        if(iMeshManager.streamHandler.isMeshLoaded)
        {
            SwapFrame(isReverse);
        }
        else
        {
            if (isBuffering)
            {
                if ((iMeshManager.streamHandler.CurrentLoadCount - CurrentFrameIndex) > TargetFPS 
                || iMeshManager.streamHandler.isMeshLoaded)
                {
                    isBuffering = false;
                    SwapFrame();
                }
                else
                {
                    Debug.LogWarning("[IMeshStreamer - Player] Buffering");
                }
            }
            else
            {
                if(CurrentFrameIndex >= iMeshManager.streamHandler.CurrentLoadCount - BufferingThreshold)
                {
                    isBuffering = true;
                }
                else
                {
                    SwapFrame();
                }
            }
        }
    }

    void FramePlay(bool isReverse = false)
    {
        if (isPlaying)
        {
            FrameTimer += Time.deltaTime;
            if (FrameTimer >= 1.0f / TargetFPS)
            {
                FrameTimer -= 1.0f / TargetFPS;
                BufferingPlay(isReverse);
            }
        }
    }

}
