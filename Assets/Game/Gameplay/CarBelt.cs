using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinthArt;

public class CarBelt : MonoBehaviour
{
    public List<HiddenCar> hiddenCars = new List<HiddenCar>();
    [SerializeField] CarSlot carSlotPrefab;
    internal List<CarSlot> cars = new List<CarSlot>();

    [SerializeField] private float carSpacing = 3.0f;  // Customize spacing as desired
    [SerializeField] private float speed = 5.0f;       // Set movement speed
    [SerializeField] private float startPositionX = 10.0f;
    [SerializeField] private float limitPosX = -10.0f;
    [SerializeField] SkinConfig skinConfig;
    [SerializeField] CarBeltMesh carBeltMesh;

    private float resetPositionX;

    [ContextMenu("Init")]
    internal void Init()
    {
        GameObject curBelt = Instantiate(Gameplay.levelSkin.belt, transform);
        carBeltMesh = curBelt.GetComponent<CarBeltMesh>();

        int i = 0;
        foreach (HiddenCar hiddenCar in hiddenCars)
        {
            CarSlot newCarSlot = Instantiate(carSlotPrefab, new Vector3(transform.position.x, 0.25f, transform.position.z), transform.rotation, transform);

            // Set initial position for each car with calculated spacing
            newCarSlot.transform.localPosition = new Vector3(startPositionX + i * carSpacing, newCarSlot.transform.localPosition.y, 0);
            newCarSlot.transform.position = newCarSlot.transform.position + new Vector3(0, 0, 1.0f);

            newCarSlot.carController.carType = hiddenCar.type;
            newCarSlot.carController.ColorCar = hiddenCar.color;
            newCarSlot.carController.curCarBelt = this;
            newCarSlot.Init();

            cars.Add(newCarSlot);
            i++;
        }

        // Calculate reset position based on total number of cars and spacing
        resetPositionX = startPositionX - (hiddenCars.Count * carSpacing);
        if (resetPositionX > limitPosX)
            resetPositionX = limitPosX;

        UpdateNumPassText();
    }
    int carCrash = 0;
    private void Update()
    {
        if (carCrash > 0)
            return;
        carCrash = 0;
        // Move each car to the left
        foreach (CarSlot carSlot in cars)
        {
            carSlot.transform.Translate(Vector3.left * speed * Time.deltaTime);

            // Check if the car has gone past the reset position on the left
            if (carSlot.transform.position.x <= resetPositionX)
            {
                // Reset car to starting position on the right
                carSlot.transform.position = new Vector3(startPositionX, carSlot.transform.position.y, carSlot.transform.position.z);
            }
        }
    }
    internal void StopRoll()
    {
        carCrash++;
        if (carBeltMesh != null && carBeltMesh.anim.enabled)
            carBeltMesh.anim.enabled = false;

    }
    internal void ContinueRoll()
    {
        carCrash--;
        if (carCrash <= 0 && carBeltMesh != null && !carBeltMesh.anim.enabled)
            carBeltMesh.anim.enabled = true;
    }
   
    internal void FreeCar(int index)
    {
        if (cars.Count <= index)
            return;

        cars[index].transform.parent = Gameplay.Instance.Level.carSlotPar;
        cars.RemoveAt(index);
        hiddenCars.RemoveAt(index);

        UpdateNumPassText();
    }
    internal void FreeCar(CarSlot carSlot)
    {
        if (!cars.Contains(carSlot))
            return;

        int index = cars.IndexOf(carSlot);
        carSlot.transform.parent = Gameplay.Instance.Level.carSlotPar;

        cars.Remove(carSlot);
        hiddenCars.RemoveAt(index);

        UpdateNumPassText();
    }
    internal void UpdateCarColorInHiddenList(CarSlot carSlot)
    {
        if (!cars.Contains(carSlot))
            return;

        int index = cars.IndexOf(carSlot);
        hiddenCars[index].color = carSlot.ColorCar;
    }
    void UpdateNumPassText()
    {
        carBeltMesh.numPassTxt.text = cars.Count.ToString();
    }
}
[System.Serializable]
public class CarBeltData
{
    public float posZ;
    public List<HiddenCar> hiddenCars = new List<HiddenCar>();
}
