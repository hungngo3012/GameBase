using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinthArt;
using TMPro;
using UnityEngine.UI;

internal class ShopUi : Scene
{
    [SerializeField] TextMeshProUGUI coinTxt;
    [SerializeField] Button closeBtn;

    private void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (Gameplay.Instance != null)
        {
            closeBtn.gameObject.SetActive(true);
            closeBtn.onClick.AddListener(() => { Close(); SoundManager.PlaySfx("BtnClick"); });
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        else
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
        }    

        UpdateCoin();
        EventManager.Subscribe(NinthArt.EventType.CoinAmountChanged, UpdateCoin);
    }
    private void OnDestroy()
    {
        EventManager.Unsubscribe(NinthArt.EventType.CoinAmountChanged, UpdateCoin);
    }
    void UpdateCoin(object o = null)
    {
        coinTxt.text = GeneralCalculate.FormatPoints(Profile.CoinAmount, 1);
    }
    void Close()
    {
        Gameplay.Instance.ChangePlayingState(Gameplay.PlayingState.Normal);
        SceneManager.CloseScene(SceneID.ShopUi);
    }     
    public void OnClickFreeCoinBtn(int val)
    {
        SoundManager.PlaySfx("coin");
        Profile.CoinAmount += val;
        Profile.CoinCollected += val;
    }   
}
