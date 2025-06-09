
using UnityEngine;
using UnityEngine.UI;

namespace NinthArt
{
	internal class ReplayUi : Popup
	{
		[SerializeField] Button confirmBtn;
		[SerializeField] Button closeBtn;

		public override void Start()
        {
			base.Start();

			confirmBtn.onClick.AddListener(() => OnClickConfirm());
			closeBtn.onClick.AddListener(() => OnClickClose());
        }
		bool restart;
		public void OnClickConfirm()
        {
			if (restart)
				return;
			restart = true;

			Config.replayLevel = true;
			Gameplay.Instance.Level.DisableProgress();

			SoundManager.PlaySfx("BtnClick");
			SceneManager.ReloadScene(SceneID.Gameplay);
			SceneManager.ClosePopups();
		}			
		public void OnClickClose()
        {
			SoundManager.PlaySfx("BtnClick");
			//Gameplay.Instance.ChangePlayingState(Gameplay.PlayingState.Normal);
			SceneManager.ClosePopup();
        }			
	}
}