using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameBtn : MonoBehaviour
{
    [SerializeField] GameObject listMiniGamesBtn;
    [SerializeField] GameObject openIcon;
    [SerializeField] GameObject closeIcon;
    public void OnClickMiniGameBtn()
    {
        listMiniGamesBtn.SetActive(!listMiniGamesBtn.activeSelf);
        openIcon.SetActive(listMiniGamesBtn.activeSelf);
        closeIcon.SetActive(!openIcon.activeSelf);
    }
}
