
using AppsFlyerSDK;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NinthArt
{
	internal class ItemAdsUi : Popup
	{
		[SerializeField] Item item;
		[SerializeField] Button buyBtn;
		[SerializeField] Button watchAdsBtn;
		[SerializeField] Button closeBtn;
		[SerializeField] TextMeshProUGUI priceTxt;

		public override void Start()
        {
			base.Start();

			buyBtn.onClick.AddListener(() => OnClickBuy());
			watchAdsBtn.onClick.AddListener(() => OnClickAds());
			closeBtn.onClick.AddListener(() => OnClickClose());
			priceTxt.text = item.price.ToString();
        }
		bool used;
		public void OnClickBuy()
        {
			if (used)
				return;

			if (Profile.CoinAmount < item.price)
            {
				if (string.IsNullOrEmpty(GameManager.Notify))
					GameManager.ShowNoti("You don't have enough money!");

				return;
            }

			used = true;
			Profile.CoinAmount -= item.price;
			UseItem();
		}
		public void OnClickAds()
        {
			if (used)
				return;

			AppsFlyer.sendEvent($"af_rewarded_ad_eligible", null);
			Ads.ShowRewardedVideo("GetItem", result =>
            {
                if (result != RewardedVideoState.Watched) return;

				used = true;
				UseItem();
            });
        }			
		void UseItem()
        {
			SceneManager.ClosePopup();
			ItemManager.AddItem(item);
			item.UseItem();
		}			
		public void OnClickClose()
        {
			Gameplay.Instance.ChangePlayingState(Gameplay.PlayingState.Normal);
			SceneManager.ClosePopup();
        }			
	}
}