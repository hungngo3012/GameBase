using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinthArt;

[CreateAssetMenu(menuName = "ScriptableObjects/BaseItem")]
public class BaseItem : Item
{
    [SerializeField]
    internal BaseItemType itemType;
    internal enum BaseItemType
    {
        COINT,
        REMOVE_ADS,
        STAR,
    }
    public override void AddItem(int num)
    {
        base.AddItem(num);
        switch(itemType)
        {
            case BaseItemType.COINT:    
                Profile.CoinAmount += num;
                Profile.CoinCollected += num;
                break;
            case BaseItemType.REMOVE_ADS:
                Profile.Vip = true;
                EventManager.Annouce(NinthArt.EventType.NoAdsPurchased);
                break;
            case BaseItemType.STAR:
                Profile.StarAmount += num;
                Profile.StarCollected += num;
                break;
        }    
    }
}
