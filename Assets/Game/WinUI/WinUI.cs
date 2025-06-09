using AppsFlyerSDK;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Funzilla;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NinthArt
{
	internal class WinUI : Popup
	{
		[SerializeField] private Button watchAdButton;
		[SerializeField] private Button noThanksButton;
		[SerializeField] private TextMeshProUGUI coinEanrTxt;
		[SerializeField] private List<GameObject> stars = new List<GameObject>();

		[SerializeField] private List<GameObject> confettiVfxs = new List<GameObject>();

		private void Awake()
		{
			if(Gameplay.Instance == null)
            {
				SceneManager.ClosePopup();
				return;
			}				
			coinEanrTxt.text = Gameplay.CoinsEarned.ToString();
			DisplayStar(Gameplay.StarEarned);
			SoundManager.PlaySfx("win");
			SoundManager.PlaySfx("confetti");
			watchAdButton.transform.HeartBeat(1.1f);
			watchAdButton.onClick.AddListener(() =>
			{
				SoundManager.PlaySfx("BtnClick");
				AppsFlyer.sendEvent($"af_rewarded_ad_eligible", null);
				Ads.ShowRewardedVideo("WinX2Gems", result =>
				{
					if (result != RewardedVideoState.Watched) return;
					Profile.CoinAmount += Gameplay.CoinsEarned * 2;
					Profile.StarAmount += Gameplay.StarEarned * 2;

					Profile.CoinCollected += Gameplay.CoinsEarned * 2;
					Profile.StarCollected += Gameplay.StarEarned * 2;
					//Gameplay.CoinsEarned = 0;
					Close();
				});
			});
			noThanksButton.onClick.AddListener(() =>
			{
				SoundManager.PlaySfx("BtnClick");
				Profile.CoinAmount += Gameplay.CoinsEarned;
				Profile.StarAmount += Gameplay.StarEarned;

				Profile.CoinCollected += Gameplay.CoinsEarned;
				Profile.StarCollected += Gameplay.StarEarned;
				//Gameplay.CoinsEarned = 0;
				Close();
			});
			StartCoroutine(PlayConfettiVfx());
		}
		private void Close()
		{
			DOTween.KillAll();
			SceneManager.ShowLoading( () =>
			{
				if (Profile.Level <= 200)
				{
					AppsFlyer.sendEvent($"completed_level_{Profile.Level}", null);
				}
				LogArchivedLevel(Profile.Level.ToString());

				Profile.Level++;
				SceneManager.ReloadScene(SceneID.Gameplay);
				SceneManager.HideLoading();
				SceneManager.ClosePopup();
			});
		}
		void DisplayStar(int num)
        {
			if (num > stars.Count)
				return;

			for(int i = 0; i < num; i++)
            {
				stars[i].SetActive(true);
            }				
        }		
		IEnumerator PlayConfettiVfx()
        {
			yield return new WaitForSeconds(GlobalDefine.delayWinConfetti);
			confettiVfxs[0].SetActive(true);
			SoundManager.PlaySfx("confetti");
			yield return new WaitForSeconds(GlobalDefine.delayWinConfetti);
			confettiVfxs[1].SetActive(true);
			SoundManager.PlaySfx("confetti");
			yield return new WaitForSeconds(GlobalDefine.delayWinConfetti);
			confettiVfxs[2].SetActive(true);
			SoundManager.PlaySfx("confetti");

			yield return new WaitForSeconds(GlobalDefine.delayWinConfetti * 5.0f);
			if (Profile.Level >= Config.LevelShowInter)
			{
				Ads.ShowInterstitial("completeLevel" + "_interstitial");
			}
		}
		public static void LogArchivedLevel(string level)
		{
			Dictionary<string, string> levels = new Dictionary<string, string>();
			levels.Add("level_pass", level);
			AppsFlyer.sendEvent("af_achieved_level", levels);
		}
	}
}