using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreamContainer : MonoBehaviour
{
    public List<Mesh> Meshes {get; private set;} = new List<Mesh>();
    public List<Material> Materials {get; private set;} = new List<Material>();
    public List<Texture2D> Textures = new List<Texture2D>();

    public void LoadMesh(Mesh mesh)
    {
        Meshes.Add(mesh);
    }

    public void LoadMaterial(Material material)
    {
        Materials.Add(material);
    }

    public void LoadTexture(Texture2D texture)
    {
        Textures.Add(texture);
    }

    public void Clear()
    {
        Meshes.Clear();
        Materials.Clear();
        Textures.Clear();
    }
}
