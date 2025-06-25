using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NinthArt;

internal class HomeUi : Scene
{
    [SerializeField] Button playBtn;
    [SerializeField] Button wheelBtn;
    [SerializeField] Button rankBtn;
    [SerializeField] Button removeAdsBtn;
    [SerializeField] Button settingBtn;
    [SerializeField] GameObject homeObj;

    [SerializeField] TextMeshProUGUI coinTxt;
    [SerializeField] TextMeshProUGUI starTxt;

    [SerializeField] Image avatar;
    [SerializeField] SkinConfig skinConfig;

    [SerializeField] GameObject vipTag;
    private void Start()
    {
        SceneManager.OpenScene(SceneID.HomeTab);
        wheelBtn.onClick.AddListener(() =>
        {
            SoundManager.PlaySfx("BtnClick");
            SceneManager.OpenPopup(SceneID.LuckyWheelUi);
        });
        rankBtn.onClick.AddListener(() =>
        {
            SoundManager.PlaySfx("BtnClick");
            SceneManager.OpenScene(SceneID.RankingUi);
        });
        playBtn.onClick.AddListener(() =>
        {
            if (homeObj != null)
                homeObj.SetActive(false);
            SoundManager.PlaySfx("BtnClick");
            SceneManager.OpenScene(SceneID.Gameplay);
            SceneManager.CloseScene(SceneID.HomeTab);
            SceneManager.CloseScene(SceneID.Home);
        });
        settingBtn.onClick.AddListener(() => { SoundManager.PlaySfx("BtnClick"); SceneManager.OpenPopup(SceneID.SettingUI); });

        if(avatar != null)
            avatar.GetComponent<Button>().onClick.AddListener(() => { SoundManager.PlaySfx("BtnClick"); SceneManager.OpenPopup(SceneID.InfoUi); });
        
        DisplayCoinAmount();
        DisplayStarAmount();

        if (!Profile.IsPlayed)
        {
            Profile.IsPlayed = true;
        }
        UpdateAvt();

        if (TimeManager.IsNewDay())
        {
            Profile.LuckyWheelProgress = 0;
            Profile.CanCollectRent = true;
        }
        ActiveVip();

        EventManager.Subscribe(NinthArt.EventType.CoinAmountChanged, DisplayCoinAmount);
        EventManager.Subscribe(NinthArt.EventType.StarAmountChanged, DisplayStarAmount);
        EventManager.Subscribe(NinthArt.EventType.UpdateInfo, UpdateAvt);
        EventManager.Subscribe(NinthArt.EventType.VipChanged, ActiveVip);
    }
    void ActiveVip(object o = null)
    {
        vipTag.SetActive(Profile.Vip);
        removeAdsBtn.gameObject.SetActive(!Profile.Vip);
    }    
    void DisplayCoinAmount(object o = null)
    {
        if (coinTxt == null)
            return;
        coinTxt.text = GeneralCalculate.FormatPoints(Profile.CoinAmount, 1);
    }
    void DisplayStarAmount(object o = null)
    {
        if (starTxt == null)
            return;
        starTxt.text = GeneralCalculate.FormatPoints(Profile.StarAmount);
    }
    private void OnDestroy()
    {
        EventManager.Unsubscribe(NinthArt.EventType.CoinAmountChanged, DisplayCoinAmount);
        EventManager.Unsubscribe(NinthArt.EventType.StarAmountChanged, DisplayStarAmount);
        EventManager.Unsubscribe(NinthArt.EventType.UpdateInfo, UpdateAvt);
        EventManager.Unsubscribe(NinthArt.EventType.VipChanged, ActiveVip);
    }
    void UpdateAvt(object o = null)
    {   
        if(avatar != null)
            avatar.sprite = skinConfig.avatars[Profile.CurAvt].avatar;
    }   
    public void OnClickAddCoin()
    {
        if (GameManager.Cheat)
            Profile.CoinAmount += 1000;
        else
            ToShopTab();
    }    
    public void ToShopTab()
    {
        EventManager.Annouce(NinthArt.EventType.GoToShop);
    }    
}
