using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinthArt;

[CreateAssetMenu(menuName = "ScriptableObjects/Sort")]
public class Sort : Item
{
    public override void UseItem()
    {
        if (Gameplay.Instance != null)
            Gameplay.Instance.UseSort();

        base.UseItem();
    }
}
