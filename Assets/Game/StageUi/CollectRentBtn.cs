using NinthArt;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectRentBtn : MonoBehaviour
{
    [SerializeField] Image collectImg;
    [SerializeField] GameObject lockObj;
    [SerializeField] TextMeshProUGUI coinTxt;
    [SerializeField] Button clickBtn;

    int coinCollect;
    internal void Init(int coin)
    {
        if(coin > 0)
        {
            UnLock();
            coinTxt.text = coin.ToString();
            coinCollect = coin;
        }   
        else
        {
            Lock();
            coinTxt.text = "0";
            coinCollect = 0;
        }    
    }
    void Lock()
    {
        collectImg.enabled = false;
        clickBtn.enabled = false;
        lockObj.SetActive(true);
    }
    void UnLock()
    {
        collectImg.enabled = true;
        clickBtn.enabled = true;
        lockObj.SetActive(false);
    }  
    public void OnClickCollect()
    {
        SoundManager.PlaySfx("coin");
        Profile.CoinAmount += coinCollect;
        Profile.CoinCollected += coinCollect;
        Profile.CanCollectRent = false;
        Lock();
        coinTxt.text = "0";
        coinCollect = 0;
    }
}
