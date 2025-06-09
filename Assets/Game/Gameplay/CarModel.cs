using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinthArt;

public class CarModel : MonoBehaviour
{
    public CarType type;
    internal CarMesh carMesh;
    private void OnEnable()
    {
        if (carMesh != null)
            return;

        GameObject obj = Instantiate(Gameplay.levelSkin.cars[(int)type], transform);
        carMesh = obj.GetComponent<CarMesh>();
    }
}
