using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class LocalPlaySample : MonoBehaviour
{
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    public Transform MeshSequenceRoot;

    public AudioClip Audio;

    public List<Transform> MeshSequenceList = new List<Transform>();

    public float FPS = 30.0f;

    float Timer = 0.0f; 

    bool IsPlaying = false;

    void Start()
    {
        stopwatch.Start();

        StartCoroutine(Load());
    }

    IEnumerator Load()
    {
        foreach (Transform child in MeshSequenceRoot)
        {
            MeshSequenceList.Add(child);
            child.gameObject.SetActive(false);
        }

        if (Audio != null)
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = Audio;
            audioSource.Play();
        }

        yield return null;

        Play();

    }

    void Play()
    {
        IsPlaying = true; 
        stopwatch.Stop();
        Debug.Log("Load Time: " + stopwatch.ElapsedMilliseconds + "ms");
    }

    // Update is called once per frame
    void Update()
    {
        if (IsPlaying)
        {
            Timer += Time.deltaTime;

            if (Timer >= 1.0f / FPS)
            {
                Timer = 0.0f;

                foreach (Transform child in MeshSequenceList)
                {
                    child.gameObject.SetActive(false);
                }

                MeshSequenceList[(int)(Time.time * FPS) % MeshSequenceList.Count].gameObject.SetActive(true);
            }
        }
    }
}
