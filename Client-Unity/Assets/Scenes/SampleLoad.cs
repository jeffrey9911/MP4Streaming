using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SampleLoad : MonoBehaviour
{
    public List<string> segments = new List<string>();

    int playerIndex = 0;

    float timer = 0.0f;
    public float switchInterval = 1.0f;

    bool isLoaded = false;

    GLTFast.GltfAsset gltfAsset;

    public string TestUrl;

    void Start()
    {
        gltfAsset = this.GetComponent<GLTFast.GltfAsset>();
    }

    public void TriggerLoad()
    {
        isLoaded = true;
    }

    void Update()
    {
        if (isLoaded)
        {
            timer += Time.deltaTime;
            if (timer > switchInterval)
            {
                if (playerIndex < segments.Count)
                {
                    playerIndex++;
                }
                else
                {
                    playerIndex = 0;
                }

                
                var success = gltfAsset.Load(segments[playerIndex]);
                DisableAll();

                timer -= switchInterval;
            }
        }
    }

    void DisableAll()
    {
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    [ContextMenu("TEST")]
    public async void TestFuntion()
    {
        GLTFast.GltfImport gltfImport = new GLTFast.GltfImport();
        var sucess = await gltfImport.Load(TestUrl);
        if(sucess)
        {
            this.GetComponent<MeshFilter>().sharedMesh = gltfImport.GetMeshes()[0];
            this.GetComponent<MeshRenderer>().material = gltfImport.GetMaterial();
            
        }
    }
}
