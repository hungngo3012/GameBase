using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NinthArt
{
    internal class NotiUi : Popup
    {
        [SerializeField] Button confirmBtn;
        [SerializeField] Button cancleBtn;
        [SerializeField] TextMeshProUGUI desTxt;

        public override void Start()
        {
            base.Start();
            //if (Gameplay.Instance != null)
                //Gameplay.Instance.ChangePlayingState(Gameplay.PlayingState.Pause);
            if(!string.IsNullOrEmpty(GameManager.Notify))
                desTxt.text = GameManager.Notify;

            cancleBtn.onClick.AddListener(() =>
            {
                cancleBtn.enabled = false;
                SoundManager.PlaySfx("BtnClick");
                ExecuteClose();
            });
            if (GameManager.ConfirmCallBack != null)
            {
                confirmBtn.onClick.AddListener(() =>
                {
                    confirmBtn.enabled = false;
                    SoundManager.PlaySfx("BtnClick");
                    GameManager.ConfirmCallBack.Invoke(null);

                    ExecuteClose();
                });
            }  
            else
            {
                confirmBtn.gameObject.SetActive(false);
            }    
        }    

        void ExecuteClose()
        {
            GameManager.ClearNotify();
            //if (Gameplay.Instance != null)
                //Gameplay.Instance.ChangePlayingState(Gameplay.PlayingState.Normal);
            SceneManager.ClosePopup();
        }
    }
}
