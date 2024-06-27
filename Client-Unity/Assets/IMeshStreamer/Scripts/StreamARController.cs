using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Experimental.XR;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARSubsystems;

public class StreamARController : MonoBehaviour
{
    private ARRaycastManager arRaycastManager;
    private Pose PlacementPose;
    private bool IsPlacementValid = false;

    [SerializeField]
    private GameObject PlacementIndicator;

    [SerializeField]
    private GameObject MovableObject;

    void Start()
    {
        arRaycastManager = FindFirstObjectByType<ARRaycastManager>();
    }

    void Update()
    {
        RaycastPlacementPose();
        UpdatePlacementIndicator();
        MoveObjectByTap();
    }

    private void RaycastPlacementPose()
    {
        var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();

        arRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);

        IsPlacementValid = hits.Count > 0;

        if (IsPlacementValid)
        {
            PlacementPose = hits[0].pose;

            var cameraForward = -Camera.current.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;

            PlacementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }

    private void UpdatePlacementIndicator()
    {
        if (IsPlacementValid)
        {
            PlacementIndicator.SetActive(true);
            PlacementIndicator.transform.SetPositionAndRotation(PlacementPose.position, PlacementPose.rotation);
        }
        else
        {
            PlacementIndicator.SetActive(false);
        }
    }

    private void MoveObjectByTap()
    {
        if (IsPlacementValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            MovableObject.transform.SetPositionAndRotation(PlacementPose.position, PlacementPose.rotation);
        }
    }
}
