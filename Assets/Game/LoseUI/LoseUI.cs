using UnityEngine;
using UnityEngine.UI;

namespace NinthArt
{
	internal class LoseUI : Popup
	{
		[SerializeField] private Button buyBtn;
		[SerializeField] private Button adsBtn;
		[SerializeField] private Button closeBtn;

		private void Awake()
		{
			buyBtn.onClick.AddListener(OnClickBuy);
			adsBtn.onClick.AddListener(OnClickWatchAds);
			closeBtn.onClick.AddListener(() => { SoundManager.PlaySfx("BtnClick"); Close();});
			SoundManager.PlaySfx("lose");
		}

		private void Close()
		{
			closeBtn.enabled = false;
			Config.replayLevel = true;
			SceneManager.ShowLoading( () =>
			{
				SceneManager.ReloadScene(SceneID.Gameplay);
				SceneManager.HideLoading();
				SceneManager.ClosePopup();
			});
		}
		bool bought;
		bool continuePlay;
		void OnClickBuy()
        {
			if (continuePlay)
				return;
			SoundManager.PlaySfx("BtnClick");
			if (Profile.CoinAmount < GlobalDefine.continuePlayPrice)
            {
				if(string.IsNullOrEmpty(GameManager.Notify))
					GameManager.ShowNoti("You don't have enough money!");
				return;
            }
			continuePlay = true;
			Profile.CoinAmount -= GlobalDefine.continuePlayPrice;

			Gameplay.Instance.ContinuePlay();
			SceneManager.ClosePopup();
		}
		void OnClickWatchAds()
        {
			if (continuePlay)
				return;
			SoundManager.PlaySfx("BtnClick");
			
			continuePlay = true;
			Gameplay.Instance.ContinuePlay();
			SceneManager.ClosePopup();
		}
	}
}