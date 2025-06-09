using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarInLevelCreator : MonoBehaviour
{
    [SerializeField] CarController carController;
    [SerializeField] CarMesh carMeshPreview;
    [SerializeField] internal ColorCar color;
    [SerializeField] internal bool isHidden;
    [SerializeField] ColorConfig colorConfig;

    [ContextMenu("SetColor")]//editor
    public void SetTestColor()
    {
        carController.HiddenColorCar = isHidden;
        carController.ColorCar = color;
        carMeshPreview.SetColor(colorConfig, isHidden, color);
    }
    [ContextMenu("SetHidden")]//editor
    public void SetHidden()
    {
        carController.HiddenColorCar = isHidden;
        carMeshPreview.SetColor(colorConfig, isHidden, color);
    }
}
