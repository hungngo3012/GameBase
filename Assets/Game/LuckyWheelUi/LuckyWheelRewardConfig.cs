using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/LuckyWheelRewardConfig")]
[System.Serializable]
public class LuckyWheelRewardConfig : ScriptableObject
{
    public List<LuckyWheelReward> rewards = new List<LuckyWheelReward>();
}
[System.Serializable]
public class LuckyWheelReward
{
    public Item item;
    public int num;
    [Range(0f, 1f)] public float ratio;
}

