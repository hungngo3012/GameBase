using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeRankTypeBtn : MonoBehaviour
{
    [SerializeField] GameObject showUser;
    [SerializeField] GameObject showTop;
    public Button btn;
    internal void UpdateState(bool top)
    {
        showTop.SetActive(top);
        showUser.SetActive(!top);
    }    
}
