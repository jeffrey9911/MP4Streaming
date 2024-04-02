using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandRotate : MonoBehaviour
{
    public float rotationSpeed = 45.0f;

    void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
