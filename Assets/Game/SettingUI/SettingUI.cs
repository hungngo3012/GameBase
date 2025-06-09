
using UnityEngine;
using UnityEngine.UI;

namespace NinthArt
{
	internal class SettingUI : Popup
	{
#if UNITY_IOS
		private const string TermsUrl = "https://www.apple.com/legal/internet-services/itunes/dev/stdeula/";
#else
		private const string TermsUrl = "https://funzilla.io/terms";
#endif
		[SerializeField] private ToggleButton sfxToggle;
		[SerializeField] private ToggleButton musicToggle;
		[SerializeField] private ToggleButton vibrateToggle;
		[SerializeField] private Button closeButton;
		[SerializeField] private GameObject gameStateBtns;
		[SerializeField] private Button homeBtn;
		[SerializeField] private Button replayBtn;

		private void Awake()
		{

		}

		private void OnEnable()
		{
#if UNITY_IOS
			rateButton.onClick.AddListener(() => UnityEngine.iOS.Device.RequestStoreReview());
#else
			//rateButton.onClick.AddListener(() => Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier));
#endif
			//privacyPolicyButton.onClick.AddListener(() => Application.OpenURL("https://www.funzilla.io/games/privacy"));
			//termOfUseButton.onClick.AddListener(() => Application.OpenURL(TermsUrl));
			RestartClick();
			UpdateState();
			sfxToggle.Init(Preference.SfxOn, active => Preference.SfxOn = active);
			musicToggle.Init(Preference.MusicOn, active => Preference.MusicOn = active);
			vibrateToggle.Init(Preference.VibrationOn, active => Preference.VibrationOn = active);
			closeButton.onClick.AddListener(() => { OnBackButtonPressed(); });
			homeBtn.onClick.AddListener(() => OnClickHome());
			replayBtn.onClick.AddListener(() => OnClickRestart());
		}

		internal override void OnBackButtonPressed()
		{
			SoundManager.PlaySfx("BtnClick");
			if(Gameplay.Instance != null)
				Gameplay.Instance.ChangePlayingState(Gameplay.PlayingState.Normal);

			SceneManager.ClosePopup();
		}
		void OnClickHome()
        {
			//Gameplay.Instance.Level.SaveGameProgress();
			SoundManager.PlaySfx("BtnClick");
			SceneManager.CloseScene(SceneID.Gameplay);
			if (GameManager.Cheat)
				CheatMenu.Hide();
			SceneManager.OpenScene(SceneID.Home);
			SceneManager.ClosePopup();
		}
		bool clickRestart;
		void OnClickRestart()
        {
			if (clickRestart)
				return;
			clickRestart = true;
			SoundManager.PlaySfx("BtnClick");
			if (Profile.Level > Config.LevelShowInter)
				Ads.ShowInterstitial("clickRestartLevel_" + Profile.Level + "_interstitial");
			Gameplay.Instance.OnClickReplay();
        }			
		void UpdateState()
        {
			if(Gameplay.Instance != null)
            {
				gameStateBtns.SetActive(true);

			}	
			else
            {
				gameStateBtns.SetActive(false);
			}				
        }		
		void RestartClick(object o = null)
        {
			clickRestart = false;
        }			
	}
}