using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Data : ScriptableObject
{
    private readonly Dictionary<Enum, IConfig> itemDictionary = new();
    
    public virtual void Init()
    {
        
    }
    protected void AddItemDataFromList<T>(List<T> list) where T : IConfig
    {
        foreach (var item in list)
        {
            var key = item.GetKey();
            if (!itemDictionary.ContainsKey(key))
            {
                itemDictionary.Add(key, item);
            }
            else
            {
                itemDictionary[key] = item;
            }
        }
    }

    public T FindItemData<T>(Enum itemType) where T : class, IConfig
    {
        return itemDictionary.TryGetValue(itemType, out var itemData) ? itemData as T : null;
    }
}
