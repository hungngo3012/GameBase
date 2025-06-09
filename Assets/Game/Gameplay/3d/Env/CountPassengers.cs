using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountPassengers : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI numTxt;

    internal void UpdateNumText(int val)
    {
        numTxt.text = val.ToString();
    }
}
