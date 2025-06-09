using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableObject", menuName = "ScriptableObjects/CarNumSlotConfig")]
[System.Serializable]
public class CarNumSlotConfig : ScriptableObject
{
    public List<SlotConfig> carSlotConfig = new List<SlotConfig>();
}
[System.Serializable]
public class SlotConfig
{
    public CarType type;
    public int slots;

    public Vector3 boxCenter;
    public Vector3 boxSize;
}
