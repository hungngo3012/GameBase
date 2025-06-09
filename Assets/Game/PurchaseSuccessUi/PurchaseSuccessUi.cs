using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NinthArt
{
    internal class PurchaseSuccessUi : Popup
    {
        [SerializeField] Button claimBtn;
        [SerializeField] IapItems iapItems;

        [SerializeField] Image icon;
        [SerializeField] TextMeshProUGUI numTxt;
        public override void Start()
        {
            base.Start();
            claimBtn.onClick.AddListener(() => Claim());

            if (Profile.CurPurchaseProduct < iapItems.items.Count)
            {
                if(iapItems.items[Profile.CurPurchaseProduct].item.icon != null)
                {
                    icon.sprite = iapItems.items[Profile.CurPurchaseProduct].purchasedIcon;
                    icon.SetNativeSize();
                }
                numTxt.text = iapItems.items[Profile.CurPurchaseProduct].num.ToString();
            }
            else
                return;

            ClaimPurchased();
        }    
        internal void Claim()
        {
            SoundManager.PlaySfx("prize");
            ExecuteClose();
        }
        internal void ClaimPurchased()
        {
            /*
            SendAFPurchaseEvent(purchaseList.bundles[Profile.CurPurchaseProduct].revenue);

            if (Profile.CurPurchaseProduct == 10)
            {
                Profile.PiggyLevel++;
                Profile.CoinAmount += (GlobalDefine.firstPiggyBank + GlobalDefine.plusPiggyBankPerLevel * Profile.PiggyLevel);
                Profile.PiggyCoin = 0;
                EventManager.Annouce(EventType.UpdateMiniGameNews);
                return;
            }*/
            iapItems.items[Profile.CurPurchaseProduct].item.AddItem(iapItems.items[Profile.CurPurchaseProduct].num);
        }
        /*
        void SendAFPurchaseEvent(float revenue)
        {
            Dictionary<string, string> eventValues = new Dictionary<string, string>();
            eventValues.Add(AFInAppEvents.CURRENCY, "USD");
            eventValues.Add(AFInAppEvents.REVENUE, (revenue * 0.8f).ToString());
            eventValues.Add("af_quantity", "1");
            AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, eventValues);
        }*/
        void ExecuteClose()
        {
            SceneManager.ClosePopup();
        }
    }
}
