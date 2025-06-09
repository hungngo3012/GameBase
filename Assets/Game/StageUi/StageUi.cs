using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinthArt;
using TMPro;
using UnityEngine.UI;

internal class StageUi : Scene
{
    [SerializeField] Button closeBtn;
    [SerializeField] GridLayoutGroup contentParent;
    [SerializeField] SkinConfig skinConfig;
    [SerializeField] StageUiComponent stageComponentPrefab;
    [SerializeField] CollectRentBtn collectRentBtn;
    bool haveChange = false;

    List<StageUiComponent> createdUis = new List<StageUiComponent>();
    private void Start()
    {
        closeBtn.onClick.AddListener(() => { OnClickClose(); SoundManager.PlaySfx("BtnClick"); });
        EventManager.Subscribe(NinthArt.EventType.UpdateSkinEquip, UpdateEquip);
        EventManager.Subscribe(NinthArt.EventType.UpdateSkinEquip, HaveChange);
        EventManager.Subscribe(NinthArt.EventType.UpdateSkinUi, UpdateComponents);

        GetComponent<Canvas>().worldCamera = Camera.main;
    }
    private void OnDestroy()
    {
        EventManager.Unsubscribe(NinthArt.EventType.UpdateSkinEquip, UpdateEquip);
        EventManager.Unsubscribe(NinthArt.EventType.UpdateSkinEquip, HaveChange);
        EventManager.Unsubscribe(NinthArt.EventType.UpdateSkinUi, UpdateComponents);
    }
    private void OnEnable()
    {
        haveChange = false;
        UpdateComponents();
        UpdateCollectCoinBtn();
    }
    void UpdateCollectCoinBtn()
    {
        if (Profile.CanCollectRent)
        {
            int rentCoin = 0;
            foreach (SkinModelConfig skinModelConfig in skinConfig.skins)
            {
                if (!Profile.Skins.Contains(skinModelConfig.skinId))
                    continue;

                rentCoin += skinModelConfig.coinCollectPerDay;
            }
            collectRentBtn.Init(rentCoin);
        }
        else
        {
            collectRentBtn.Init(0);
        }    
    }    
    void OnClickClose()
    {
        //thông báo nếu có change
        if (haveChange)
        {
            //open popup thông báo
            GameManager.ShowNoti(GlobalDefine.confirmChangeSkinMess, ConfirmChange);
        }
        else
            Close();
    }    
    void HaveChange(object o = null)
    {
        haveChange = true;
    }    
    void Close()
    {
        if(Gameplay.Instance != null)
            Gameplay.Instance.ChangePlayingState(Gameplay.PlayingState.Normal);

        SceneManager.CloseScene(SceneID.StageUi);
    }
    void UpdateComponents(object o = null)
    {
        ClearCreatedUi();

        StageUiComponent dfSkin = Instantiate(stageComponentPrefab, contentParent.transform);
        dfSkin.Init(skinConfig.defaultSkins, contentParent);
        createdUis.Add(dfSkin);
        foreach (SkinModelConfig skinModelConfig in skinConfig.skins)
        {
            StageUiComponent skin = Instantiate(stageComponentPrefab, contentParent.transform);
            skin.Init(skinModelConfig, contentParent);
            createdUis.Add(skin);
        }
        UpdateEquip();
    }  
    void ClearCreatedUi()
    {
        foreach(StageUiComponent stageUiComponent in createdUis)
        {
            Destroy(stageUiComponent.gameObject);
        }
        createdUis.Clear();
    }  
    public void UpdateEquip(object o = null)
    {
        foreach(StageUiComponent stageUiComponent in createdUis)
        {
            stageUiComponent.UpdateEquip();
        }    
    }    
    void ConfirmChange(object o = null)
    {
        if(Gameplay.Instance != null)
            SceneManager.ReloadScene(SceneID.Gameplay);
        SceneManager.CloseScene(SceneID.StageUi);
    }       
}
