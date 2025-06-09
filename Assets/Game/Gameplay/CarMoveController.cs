using System.Collections;
using System.Collections.Generic;
using NinthArt;
using UnityEditor;
using UnityEngine;

public class CarMoveController : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Camera cam;
    [SerializeField] private CarSlot selectedItem;
    public void DoFrame(bool selectVipCar = false)
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000, layerMask))
            {
                selectedItem = hit.collider.GetComponent<CarSlot>();
                if (selectedItem != null)
                {
                    if (Gameplay.Instance != null && Gameplay.Instance.TutoHand.activeSelf)
                        Gameplay.Instance.TutoHand.SetActive(false);
                    selectedItem.ItemGrabbed(selectVipCar);
                }
            }
        }
    }
}

