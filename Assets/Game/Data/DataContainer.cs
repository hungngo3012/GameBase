using System;
using System.Collections;
using System.Collections.Generic;
using NinthArt;
using UnityEngine;

public class DataContainer : Singleton<DataContainer>
{
    public List<Data> listData;
    private void Start()
    {
        foreach (var data in listData)
        {
            data.Init();
        }
    }
    public T GetData<T>() where T : Data
    {
        foreach (var data in listData)
        {
            if (data is T)
            {
                return data as T;
            }
        }
        return null;
    }
} 
