using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NinthArt;

public class StageUiComponent : MonoBehaviour
{
    [SerializeField] Image unlockImage;
    [SerializeField] TextMeshProUGUI nameLock;
    [SerializeField] TextMeshProUGUI progressTxt;
    [SerializeField] GameObject lockObj;
    [SerializeField] private RectTransform progressBar;
    [SerializeField] private RectTransform progressBg;
    [SerializeField] Button btn;
    [SerializeField] GameObject equippedObj;
    [SerializeField] TextMeshProUGUI coinRentTxt;
    SkinModelConfig curSkinModel;
    internal void Init(SkinModelConfig skinModelConfig, GridLayoutGroup content = null)
    {
        curSkinModel = new SkinModelConfig(skinModelConfig);
        if (skinModelConfig.skinBg != null)
            unlockImage.sprite = skinModelConfig.skinBg;
        nameLock.text = skinModelConfig.skinName;
        if(skinModelConfig.coinCollectPerDay > 0)
        {
            coinRentTxt.gameObject.SetActive(true);
            coinRentTxt.text = skinModelConfig.coinCollectPerDay.ToString() + "/D";
        }    
        if (Profile.Skins.Contains(skinModelConfig.skinId))
        {
            Unlock();
        }   
        else
        {
            UpdateProgressUi();
        }
        EventManager.Subscribe(NinthArt.EventType.StarAmountChanged, UpdateProgressUi);
        if (content != null)
            lockObj.GetComponent<RectTransform>().sizeDelta = content.cellSize;
    }
    private void OnDestroy()
    {
        EventManager.Unsubscribe(NinthArt.EventType.StarAmountChanged, UpdateProgressUi);
    }
    internal void UpdateProgressUi(object o = null)
    {
        progressTxt.text = Profile.StarAmount + "/" + curSkinModel.numStarsToUnlock.ToString();

        var width = progressBg.sizeDelta.x;
        var size = progressBar.sizeDelta;
        float ratio = ((float)Profile.StarAmount / (float)curSkinModel.numStarsToUnlock);
        if (ratio > 1.0f)
            ratio = 1.0f;
        size.x = width * ratio;
        progressBar.sizeDelta = new Vector2(size.x, size.y);
    }
    internal void Unlock()
    {
        lockObj.SetActive(false);
        btn.enabled = true;
    }
    public void OnSelectSkin()
    {
        if(curSkinModel != null)
        {
            SoundManager.PlaySfx("BtnClick");

            if(Profile.CurrentSkin != curSkinModel.skinId)
            {
                Profile.SelectSkin(curSkinModel.skinId);
                EventManager.Annouce(NinthArt.EventType.UpdateSkinEquip);
            }    
        }    
    }
    public void OnClickUnlockSkin()
    {
        if (curSkinModel == null)
            return;

        SoundManager.PlaySfx("BtnClick");
        if (curSkinModel.numStarsToUnlock <= Profile.StarAmount)
        {
            GameManager.ShowNoti(GlobalDefine.confirmUnlockSkinMess + " " + curSkinModel.skinName + "?", ConfirmUnlock);
        }    
        else
        {
            //thông báo
            GameManager.ShowNoti(GlobalDefine.unlockSkinFailMess + " " + curSkinModel.skinName, null);
        }    
    }    
    void ConfirmUnlock(object o = null)
    {
        Profile.StarAmount -= curSkinModel.numStarsToUnlock;
        Profile.UnlockSkin(curSkinModel.skinId);
        Unlock();
        EventManager.Annouce(NinthArt.EventType.UpdateSkinEquip);
    }    
    internal void UpdateEquip()
    {
        if (curSkinModel == null)
        {
            equippedObj.SetActive(false);
            return;
        }

        if (Profile.CurrentSkin == curSkinModel.skinId)
            equippedObj.SetActive(true);
        else
            equippedObj.SetActive(false);
    }
}
