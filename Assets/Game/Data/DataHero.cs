using System;
using System.Collections;
using System.Collections.Generic;
using NinthArt;
using UnityEngine;
[CreateAssetMenu(fileName = "DataCardHero", menuName = "ScriptableObjects/DataCardHero", order =2)]
public class DataHero : Data
{
    public List<CardHeroConfig> listCardHeroConfig;
    public override void Init()
    {
        base.Init();
        AddItemDataFromList(listCardHeroConfig);
    }

    public int MaxLvCardByType(CardHeroType type)
    {
        return FindItemData<CardHeroConfig>(type).maxLv;
    }

    
}
[Serializable]
public class CardHeroConfig:IConfig
{
    public CardHeroType cardType;
    public string nameCard;
    public string desCard;
    public string statCard;
    public int baseStat;
    public int bonusStatPerLv;
    public int cardNeedToUpPerLv;
    public Sprite avt;
    public int maxLv = 100;
    public TypeBonus typeBonus;
    public StatBonusHeroType statBonusType;
    public TypeUnlock typeUnlock;
    public int countUnlock;
    public Enum GetKey() => cardType;
    public string GetUnlockCondition()
    {
        var uls = typeUnlock switch
        {
            TypeUnlock.Level => $"Unlock at Level {countUnlock}",
            TypeUnlock.Coin => $"Unlock with {countUnlock} Coins",
            TypeUnlock.Gem => $"Unlock with {countUnlock} Gems",
            TypeUnlock.Ads => $"Unlock by watching {countUnlock} Ads",
            _ => throw new ArgumentOutOfRangeException(nameof(typeUnlock), typeUnlock, null)
        };

        return uls;
    }

    public int CardNeedToUp(int level)
    {
        return level * cardNeedToUpPerLv;
    }
}

public enum CardHeroType
{
    Attack,
    Hp,
    Range,
    Shield,
    Speed
}

public enum StatBonusHeroType
{
    Attack,
    Hp,
    Range,
    Shield,
    Speed
}

public enum TypeUnlock
{
    Level,
    Coin,
    Gem,
    Ads
}