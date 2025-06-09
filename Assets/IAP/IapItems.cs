using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinthArt;

[CreateAssetMenu(fileName = "ScriptableObject", menuName = "ScriptableObjects/IapItems")]
public class IapItems : ScriptableObject
{
    [SerializeField] internal List<Pack> items = new List<Pack>();
}
[System.Serializable]
public class Pack
{
    [SerializeField] internal IAP.Item iapItem;

    public Item item;
    public int num;
    public Sprite purchasedIcon;

    public bool isRemoveAds;
}

