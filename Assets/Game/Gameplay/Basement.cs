using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinthArt;
using TMPro;

[System.Serializable]
public class Basement : MonoBehaviour
{
    public Transform carSpawnPos;
    public SkinConfig skinConfig;
    public List<HiddenCar> hiddenCars = new List<HiddenCar>();

    [SerializeField] ColorConfig colorConfig;
    [SerializeField] CarSlot carSlotPrefab;
    GarageMesh garageMesh;
    internal void Init(bool reload = false)
    {
        GameObject model = Instantiate(Gameplay.levelSkin.basement, transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.rotation = transform.rotation;

        garageMesh = model.GetComponent<GarageMesh>();

        if (!reload)
            SpawnCar();
        else
        {
            if (garageMesh != null && garageMesh.countTxt != null)
                garageMesh.countTxt.text = hiddenCars.Count.ToString();

            SetGateColor();
        }    
    }
    void SetGateColor()
    {
        if (garageMesh == null)
            return;

        Color curColor = Color.white;
        if(hiddenCars.Count > 0)
        {
            ColorCar colorCar = hiddenCars[0].color;
            if (Gameplay.Instance != null)
                curColor = colorConfig.colors[Gameplay.Instance.Level.shuffleColors[(int)colorCar]];
            else
                curColor = colorConfig.colors[(int)colorCar];
        }    
        garageMesh.ChangeColor(curColor);
    }    
    internal void SpawnCar(int num = 1)
    {
        if (hiddenCars.Count < num)
            return;

        for(int i = num; i > 0; i--)
        {
            HiddenCar hiddenCar = hiddenCars[0];
            CarSlot newCarSlot = Instantiate(carSlotPrefab, transform.position, transform.rotation, Gameplay.Instance.Level.carSlotPar);
            newCarSlot.transform.localScale = Vector3.zero;

            newCarSlot.carController.carType = hiddenCar.type;
            newCarSlot.carController.ColorCar = hiddenCar.color;
            newCarSlot.curBasement = this;
            newCarSlot.Init();

            hiddenCars.RemoveAt(0);

            newCarSlot.CarMoveOutFromBasement(carSpawnPos.transform.position);
        }

        if(garageMesh != null)
        {
            garageMesh.countTxt.text = hiddenCars.Count.ToString();
            SetGateColor();
        }
    }
}

[System.Serializable]
public class HiddenCar
{
    public CarType type;
    public ColorCar color;
}
[System.Serializable]
public class BasementData
{
    public string Id;

    public float posX;
    public float posZ;

    public float rotation;
    public List<HiddenCar> hiddenCars = new List<HiddenCar>();
}
