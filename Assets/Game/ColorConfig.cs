using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableObject", menuName = "ScriptableObjects/ColorConfig")]
[System.Serializable]
public class ColorConfig : ScriptableObject
{
    public List<Color> colors = new List<Color>();
    public Color hiddenColor;
}
public enum ColorCar
{
    Red = 0,
    Green = 1,
    Blue = 2,
    Cyan = 3,
    Yellow = 4,
    Pink = 5,
    Purple = 6,
    Orange = 7,
    White = 8,
    Black = 9,
}
