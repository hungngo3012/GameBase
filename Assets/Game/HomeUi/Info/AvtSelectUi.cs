using NinthArt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvtSelectUi : MonoBehaviour
{
    [SerializeField] SkinConfig skinConfig;
    [SerializeField] internal Image avtImg;
    [SerializeField] GameObject tick;
    internal int avtIndex;

    internal void Init(int index)
    {
        if (skinConfig.avatars.Count <= index)
            return;

        avtIndex = index;
        avtImg.sprite = skinConfig.avatars[index].avatar;

        UpdateSelect();
        EventManager.Subscribe(NinthArt.EventType.UpdateInfo, UpdateSelect);
    }
    public void OnClickSelect()
    {
        Profile.CurAvt = avtIndex;
        EventManager.Annouce(NinthArt.EventType.UpdateInfo);
    }
    internal void UpdateSelect(object o = null)
    {
        if (Profile.CurAvt == avtIndex)
            tick.SetActive(true);
        else
            tick.SetActive(false);
    }
    private void OnDestroy()
    {
        EventManager.Unsubscribe(NinthArt.EventType.UpdateInfo, UpdateSelect);
    }
}
