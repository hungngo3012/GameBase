using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NinthArt;

[CreateAssetMenu(fileName = "DataCardNewConfig", menuName = "ScriptableObjects/DataCardNewConfig", order = 1)]
public class DataCardNewConfig : Data
{
    public List<CardConfigNew> listCardConfigNews;

    public override void Init()
    {
        base.Init();
        AddItemDataFromList(listCardConfigNews);
    }
}

[Serializable]
public class CardConfigNew:IConfig
{
    public ColorCar cardType;
    public Enum GetKey() => cardType;
} 
public interface IConfig
{
    Enum GetKey();
}

public enum TypeBonus
{
    Plus,
    Percent
}

public enum StatBonusType
{
    None,
    Hp,
    Attack
}