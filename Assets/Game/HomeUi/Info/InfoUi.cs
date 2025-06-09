using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NinthArt
{
    internal class InfoUi : Popup
    {
        [SerializeField] AvtSelectUi basePrefab;
        [SerializeField] Button changeNameBtn;
        [SerializeField] Button closeBtn;
        [SerializeField] Transform avtContent;

        [SerializeField] Image curAva;
        [SerializeField] TextMeshProUGUI curNameTxt;
        [SerializeField] TextMeshProUGUI tagTxt;

        [SerializeField] TextMeshProUGUI trophyTxt;
        [SerializeField] TextMeshProUGUI coinTxt;
        [SerializeField] TextMeshProUGUI starTxt;

        [SerializeField] SkinConfig skinConfig;
        public override void Start()
        {
            base.Start();
            closeBtn.onClick.AddListener(() => { SoundManager.PlaySfx("BtnClick"); ExecuteClose(); });
            changeNameBtn.onClick.AddListener(() => OnClickChangeName());

            Init();
            EventManager.Subscribe(EventType.UpdateInfo, UpdateInfo);
        }   
        void Init()
        {
            UpdateInfo();
            int i = 0;
            foreach(AvatarConfig avatarConfig in skinConfig.avatars)
            {
                AvtSelectUi avtSelectUi = Instantiate(basePrefab, avtContent);
                avtSelectUi.Init(i);

                i++;
            }
        }
        void OnClickChangeName()
        {
            SoundManager.PlaySfx("BtnClick");
            SceneManager.OpenPopup(SceneID.ChangeName);
        }
        void ExecuteClose()
        {
            SceneManager.ClosePopup();
        }
        void UpdateInfo(object o = null)
        {
            curAva.sprite = skinConfig.avatars[Profile.CurAvt].avatar;
            curNameTxt.text = Profile.UserName;
            tagTxt.text = Profile.UserTag;

            trophyTxt.text = Profile.Level.ToString();
            coinTxt.text = Profile.CoinCollected.ToString();
            starTxt.text = Profile.StarCollected.ToString();
        }
        private void OnDestroy()
        {
            EventManager.Unsubscribe(EventType.UpdateInfo, UpdateInfo);
        }
    }
}
