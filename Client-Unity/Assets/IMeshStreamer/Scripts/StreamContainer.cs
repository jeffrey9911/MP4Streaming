using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreamContainer : MonoBehaviour
{
    public List<Mesh> Meshes {get; private set;} = new List<Mesh>();
    public List<Material> Materials {get; private set;} = new List<Material>();

    public void LoadMesh(Mesh mesh)
    {
        Meshes.Add(mesh);
    }

    public void LoadMaterial(Material material)
    {
        Materials.Add(material);
    }

    public void Clear()
    {
        Meshes.Clear();
        Materials.Clear();
    }
}
