using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NinthArt
{
    internal class RateUi : Popup
    {
        [SerializeField] Button yesBtn;
        [SerializeField] Button noBtn;

        public override void Start()
        {
            base.Start();
            Profile.RatingPopupShown = true;
            if (Gameplay.Instance != null)
                Gameplay.Instance.ChangePlayingState(Gameplay.PlayingState.Pause);

            noBtn.onClick.AddListener(() => { SoundManager.PlaySfx("BtnClick"); ExecuteClose(); });
            yesBtn.onClick.AddListener(() => { SoundManager.PlaySfx("BtnClick"); ExecuteClose(); Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);});
        }    

        void ExecuteClose()
        {
            if (Gameplay.Instance != null)
                Gameplay.Instance.ChangePlayingState(Gameplay.PlayingState.Normal);
            SceneManager.ClosePopup();
        }
    }
}
