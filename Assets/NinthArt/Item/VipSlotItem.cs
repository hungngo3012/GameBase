using NinthArt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/VipSlotItem")]
public class VipSlotItem : Item
{
    public override void UseItem()
    {
        if (Gameplay.Instance != null)
            Gameplay.Instance.UseVipSlot();

        base.UseItem();
        //user Vip Slot item
    }
}
