using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayPoint : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private float distance=100;
    public float Distance=>distance;
    public void InitBox(BoxCollider box)
    {
        boxCollider = box;
    }
    public bool IsCar()
    {
        var ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out var hit, 100, layerMask))
        {
            if (hit.collider != boxCollider)
            {
                return hit.collider.CompareTag("Car");
            }
        }
        return true;
    }
    public CarMesh GetCarMesh()
    {
        if (hitObj == null)
            return null;

        CarSlot carSlot = hitObj.GetComponent<CarSlot>();
        if (carSlot == null)
            return null;

        Debug.Log(carSlot.gameObject.name + " - impact");
        return carSlot.carController.GetCarMesh();
    }
    GameObject hitObj;
    public Vector3 PointToMove(Transform tf)
    {
        var ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, layerMask))
        {
            if (hit.collider != boxCollider)
            {
                distance = Vector3.Distance(hit.point, transform.position);
                hitObj = hit.collider.gameObject;
                return hit.point;
            }
        }
        return tf.position;
    }

    public float CheckDistance(Vector3 point)
    {
        return Vector3.Distance(transform.position, point);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

// Vẽ một ray dài 5 đơn vị từ vị trí của đối tượng theo hướng forward
        Vector3 direction = transform.TransformDirection(Vector3.forward) * 5;
        Gizmos.DrawRay(transform.position, direction);
    }
}
