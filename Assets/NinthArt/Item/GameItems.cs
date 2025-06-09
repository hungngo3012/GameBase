using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/GameItems")]
public class GameItems : ScriptableObject
{
    public List<Item> items = new List<Item>();
}
