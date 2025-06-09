using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NinthArt
{
    internal class UnlockUi : Popup
    {
        [SerializeField] Button getBtn;
        [SerializeField] Item unlockItem;
        [SerializeField] string unlockItemId;
        bool gotItem;
        public override void Start()
        {
            base.Start();

            getBtn.onClick.AddListener(() => OnClickGet());
        }
        void OnClickGet()
        {
            if (gotItem)
                return;
            gotItem = true;
            SoundManager.PlaySfx("prize");

            if(unlockItem == null)
                ItemManager.AddItem(unlockItemId);
            else
                ItemManager.AddItem(unlockItem);

            Profile.UnlockToolProgress++;
            ExecuteClose();
        }
        void ExecuteClose()
        {
            SceneManager.ClosePopup();
        }
    }
}
