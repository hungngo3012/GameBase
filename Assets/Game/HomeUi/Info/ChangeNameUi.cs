using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NinthArt
{
    internal class ChangeNameUi : Popup
    {
        [SerializeField] Button confirmBtn;
        [SerializeField] Button closeBtn;
        [SerializeField] TMP_InputField newNameTxt;

        public override void Start()
        {
            base.Start();
            newNameTxt.text = Profile.UserName;
            closeBtn.onClick.AddListener(() => { SoundManager.PlaySfx("BtnClick"); ExecuteClose();});
            confirmBtn.onClick.AddListener(() => { SoundManager.PlaySfx("BtnClick"); ConFirmChangeName();});
        }    

        void ExecuteClose()
        {
            SceneManager.ClosePopup();
        }
        void ConFirmChangeName()
        {
            Profile.UserName = newNameTxt.text;
            EventManager.Annouce(EventType.UpdateInfo);
            ExecuteClose();
        }
    }
}
