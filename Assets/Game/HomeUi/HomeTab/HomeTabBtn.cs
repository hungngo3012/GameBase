using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeTabBtn : MonoBehaviour
{
    [SerializeField] internal Button btn;
    [SerializeField] Image notSelectedImg;
    [SerializeField] Button notSelectedButton;
    [SerializeField] GameObject selectedObj;

    internal void SetState(bool selected)
    {
        selectedObj.SetActive(selected);
        notSelectedImg.enabled = !selected;
        notSelectedButton.enabled = !selected;
    } 
}
