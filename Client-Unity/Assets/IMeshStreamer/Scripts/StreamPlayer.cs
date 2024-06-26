using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
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

    public bool IsManual = false;
    public bool IsAVControlledPlay = false;

    GameObject PlayerInstance;
    MeshFilter PlayerInstanceMesh;
    MeshRenderer PlayerInstanceRenderer;
    Material PlayerInstanceMaterial;



    public List<Texture2D> Textures = new List<Texture2D>();


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
            if (IsManual)
            {
                ManualPlay();
            }
            else
            {
                FramePlay();
            }
        }
    }

    [ContextMenu("Play")]
    public void Play()
    {
        try
        {
            isPlaying = true;

            PlayerInstance = new GameObject("PlayerInstance");
            PlayerInstance.transform.SetParent(this.transform);
            PlayerInstance.transform.localPosition = Vector3.zero;
            PlayerInstance.transform.localRotation = Quaternion.Euler(new Vector3(90f, 0, 0));

            PlayerInstance.AddComponent<StandRotate>();


            PlayerInstanceMesh = PlayerInstance.AddComponent<MeshFilter>();
            PlayerInstanceRenderer = PlayerInstance.AddComponent<MeshRenderer>();
            
            // todo: fix material issues - set shaders to include downloaded ones.

            
            PlayerInstanceMaterial.SetTexture("baseColorTexture", iMeshManager.streamContainer.VideoTexture);
            PlayerInstanceMaterial.SetTexture("emissiveTexture", iMeshManager.streamContainer.VideoTexture);

            PlayerInstanceRenderer.material = PlayerInstanceMaterial;
            
            
            if (IsAVControlledPlay)
            {
                iMeshManager.streamContainer.VideoContainer.isLooping = true;
                iMeshManager.streamContainer.VideoContainer.Play();
            }
        }
        catch (System.Exception e)
        {
            iMeshManager.Debug($"[Play()]: {e} : {e.Message} : {e.StackTrace}");
        }
    }

    public void InitMaterial(Material material)
    {
        PlayerInstanceMaterial = new Material(material);
    }



    void SwapFrame(bool isReverse = false)
    {
        if (IsAVControlledPlay)
        {
            if (CurrentFrameIndex != (int)iMeshManager.streamContainer.VideoContainer.frame)
            {
                CurrentFrameIndex = (int)iMeshManager.streamContainer.VideoContainer.frame;

                if ( (CurrentFrameIndex + 1) >= iMeshManager.streamHandler.TotalLoadCount)
                {
                    CurrentFrameIndex = iMeshManager.streamHandler.TotalLoadCount - 1;
                }

                try
                {
                    PlayerInstanceMesh.mesh = iMeshManager.streamContainer.Meshes[CurrentFrameIndex];
                }
                catch
                {
                    Debug.LogWarning("[IMeshStreamer - Player] Mesh not loaded yet");
                }
            }

            return;
        }


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

        //PlayerInstanceMaterial.SetTexture("baseColorTexture", iMeshManager.streamContainer.Textures[CurrentFrameIndex]);
        //PlayerInstanceMaterial.SetTexture("emissiveTexture", iMeshManager.streamContainer.Textures[CurrentFrameIndex]);
    }

    public void AVControlledFramePlay()
    {
        Debug.Log("[IMeshStreamer - Player] AVControlledFramePlay");
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
            if (IsAVControlledPlay)
            {
                BufferingPlay(isReverse);
                return;
            }

            FrameTimer += Time.deltaTime;
            if (FrameTimer >= 1.0f / TargetFPS)
            {
                FrameTimer -= 1.0f / TargetFPS;
                BufferingPlay(isReverse);
            }
        }
    }

    void ManualPlay()
    {
        if (isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                SwapFrame();
                /*
                RenderTexture rt = iMeshManager.streamContainer.VideoContainer.texture as RenderTexture;

                Texture2D tex = new Texture2D(rt.width, rt.height);
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply();
                Textures.Add(tex);
                */
                
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SwapFrame(true);
            }
        }
    }

}
