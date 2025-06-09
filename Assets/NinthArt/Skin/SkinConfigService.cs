using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinConfigService
{
    public static SkinModelConfig GetSkinModelConfig(SkinConfig skinConfig, string skinId)
    {
        SkinModelConfig result = skinConfig.skins.Find(skinModel => skinModel.skinId == skinId);
        if (result != null)
            return result;

        return skinConfig.defaultSkins;
    }
    public static GameObject GetCarSkinModel(SkinConfig skinConfig, CarType carType, string skinId = "")
    {
        if(string.IsNullOrEmpty(skinId))
            return skinConfig.defaultSkins.carModels[(int)carType];

        SkinModelConfig skinModelConfig = GetSkinModelConfig(skinConfig, skinId);

        if(skinModelConfig.carModels.Count <= (int)carType)
            return skinConfig.defaultSkins.carModels[(int)carType];

        if (skinModelConfig.carModels[(int)carType] != null)
            return skinModelConfig.carModels[(int)carType];

        return skinConfig.defaultSkins.carModels[(int)carType];
    }
    public static GameObject GetPassengerSkinModel(SkinConfig skinConfig, string skinId = "")
    {
        if (string.IsNullOrEmpty(skinId))
            return skinConfig.defaultSkins.passenger;

        SkinModelConfig skinModelConfig = GetSkinModelConfig(skinConfig, skinId);
        if (skinModelConfig.passenger != null)
            return skinModelConfig.passenger;

        return skinConfig.defaultSkins.passenger;
    }
    public static GameObject GetEnvSkinModel(SkinConfig skinConfig, string skinId = "")
    {
        if (string.IsNullOrEmpty(skinId))
            return skinConfig.defaultSkins.passenger;

        SkinModelConfig skinModelConfig = GetSkinModelConfig(skinConfig, skinId);
        GameObject result = skinModelConfig.env;

        if (result != null)
            return result;

        return skinConfig.defaultSkins.env;
    }
    public static GameObject GetGarageSkinModel(SkinConfig skinConfig, string skinId = "")
    {
        if (string.IsNullOrEmpty(skinId))
            return skinConfig.defaultSkins.garage;

        SkinModelConfig skinModelConfig = GetSkinModelConfig(skinConfig, skinId);
        GameObject result = skinModelConfig.garage;

        if (result != null)
            return result;

        return skinConfig.defaultSkins.garage;
    }
    public static GameObject GetBeltSkinModel(SkinConfig skinConfig, string skinId = "")
    {
        if (string.IsNullOrEmpty(skinId))
            return skinConfig.defaultSkins.belt;

        SkinModelConfig skinModelConfig = GetSkinModelConfig(skinConfig, skinId);
        GameObject result = skinModelConfig.belt;

        if (result != null)
            return result;

        return skinConfig.defaultSkins.belt;
    }
}
