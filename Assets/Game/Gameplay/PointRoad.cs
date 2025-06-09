using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointRoad : MonoBehaviour
{
    public List<PointRoad> listConnect;
    public Transform tf;
    public float weight;
    public bool isEndRoadPoint;

    private void Awake()
    {
        tf = transform;
    }
}
