using System.Collections.Generic;
using UnityEngine;

public class PlaneTest : MonoBehaviour
{
    void Start()
    {
        var points = new List<Vector3>();
        foreach (Transform child in transform)
        {
            points.Add(child.position);
        }

        var plane = MathUtils.FitPlane(points.ToArray(), out var discardedPointCount);
        Debug.Log("discarded points: " + discardedPointCount);
        var planeObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        var planePos = plane.ClosestPointOnPlane(transform.position);
        planeObj.transform.position = planePos;
        var forwardProjection = Vector3.ProjectOnPlane(planeObj.transform.forward, plane.normal);
        planeObj.transform.LookAt(planePos + forwardProjection * 100f, plane.normal);
    }
}
