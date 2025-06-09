using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinthArt;

public class TimeManager : MonoBehaviour
{
    public static bool IsNewDay()
    {
        DateTime currentDate = DateTime.Now.Date;

        if (currentDate != Profile.LastCheckedTime.Date)
        {
            Profile.LastCheckedTime = currentDate;
            return true;
        }

        return false;
    }
}