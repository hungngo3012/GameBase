using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/SkinConfig")]
[System.Serializable]
public class SkinConfig : ScriptableObject
{
    public List<SkinModelConfig> skins = new List<SkinModelConfig>();
    public SkinModelConfig defaultSkins;
    public List<AvatarConfig> avatars = new List<AvatarConfig>();
    public List<AvatarConfig> rankingAvas = new List<AvatarConfig>();
}
[System.Serializable]
public class SkinModelConfig
{
    public string skinId;
    public string skinName;
    public int numStarsToUnlock;
    public int coinCollectPerDay;
    public Sprite skinBg;

    public GameObject env;
    public List<GameObject> carModels;
    public GameObject passenger;
    public GameObject garage;
    public GameObject belt;

    public SkinModelConfig(SkinModelConfig skinModelConfig)
    {
        skinId = skinModelConfig.skinId;
        skinName = skinModelConfig.skinName;
        numStarsToUnlock = skinModelConfig.numStarsToUnlock;
        coinCollectPerDay = skinModelConfig.coinCollectPerDay;
        skinBg = skinModelConfig.skinBg;

        env = skinModelConfig.env;
        carModels = new List<GameObject>(skinModelConfig.carModels);
        passenger = skinModelConfig.passenger;
        garage = skinModelConfig.garage;
        belt = skinModelConfig.belt;
    }
}
[System.Serializable]
public class AvatarConfig
{
    public Sprite avatar;
}
