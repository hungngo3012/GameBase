using NinthArt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Shuffle")]
public class Shuffle : Item
{
    public override void UseItem()
    {
        if (Gameplay.Instance != null)
            Gameplay.Instance.UseShuffle();

        base.UseItem();
    }
}
