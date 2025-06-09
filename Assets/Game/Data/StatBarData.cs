using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataStatHero", menuName = "ScriptableObjects/DataStat", order =3)]
public class StatBarData : Data
{
    public List<StatConfig> listStatConfig;
    public override void Init()
    {
        base.Init();
        AddItemDataFromList(listStatConfig);
    }
}

[Serializable]
public class StatConfig: IConfig
{
    public CardHeroType cardHeroType;
    public string nameStat;
    public Sprite avt;
    public Enum GetKey() => cardHeroType;
}