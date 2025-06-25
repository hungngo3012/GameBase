using NinthArt;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointStop : MonoBehaviour
{
    [SerializeField] private Transform posStop;
    [SerializeField] private Transform posStopRoad;
    [SerializeField] private bool isUnlocked;
    [SerializeField] private PointStopSkin curSkin;
    [SerializeField] private bool isBusy;
    [SerializeField] private CarController carController;

    public bool isVipSlot;
    public CarController CarController => carController;
    public bool IsUnlock => isUnlocked;
    public bool IsBusy => isBusy;
    private void Start()
    {
        UpdateSlotState();
    }
    internal void UpdateSlotState()
    {
        curSkin.activeObj.SetActive(isUnlocked);
        curSkin.lockObj.SetActive(!isUnlocked);
    }    
    public void SetBusy()
    {
        isBusy = true;
    }

    public void SetCar(CarController c)
    {
        carController = c;
    }
    public ColorCar? ColorCarCurrent()
    {
        if (carController != null)
            return carController.ColorCar;
        return null;
    }
    public void SetFree()
    {
        isBusy = false;
        carController = null;
    }
    
    public Vector3 RStop()
    {
        return posStopRoad.position;
    }
    public Vector3 VStop()
    {
        return posStop.position;
    }
    public Vector3 VStop(Vector3 offset)
    {
        Vector3 newLocalPosStop = posStop.localPosition + offset;
        return posStop.parent.TransformPoint(newLocalPosStop);
    }
    internal void UnlockPointStop(bool vipSlot = false, bool saveProgress = true)
    {
        isUnlocked = true;
        UpdateSlotState();
        if (!vipSlot)
            Gameplay.Instance.Level.numSlotUnlocked++;
        if(saveProgress)
            Gameplay.Instance.Level.SaveGameProgress();
    }    
    #region on click
    bool mouseDown;
    private void OnMouseDown()
    {
        if (isUnlocked || Gameplay.Instance._playingState == Gameplay.PlayingState.Pause)
            return;

        mouseDown = true;
    }
    private void OnMouseUp()
    {
        if (isUnlocked)
            return;

        if (mouseDown)
            OnClickWatchAds();
    }
    private void OnMouseExit()
    {
        mouseDown = false;
    }
    void OnClickWatchAds()
    {
        if(isVipSlot)
        {
            Gameplay.Instance.OnClickVipSlot();
        }   
        else
        {
            UnlockPointStop();
        }    
    }
    #endregion
}
