using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace NinthArt
{
	internal class CheatMenu : Singleton<CheatMenu>
	{
		[SerializeField] private Canvas canvas;
		[SerializeField] private Button restartButton;
		[SerializeField] private Button prevButton;
		[SerializeField] private Button nextButton;
		[SerializeField] private Button levelButton;
		[SerializeField] private LevelSelector levelSelector;
		[SerializeField] private Button experimentButton;
		[SerializeField] private ExperimentSelector experimentSelector;

		private void Awake()
		{
			restartButton.onClick.AddListener(() => { Config.replayLevel = true; SceneManager.ReloadScene(SceneID.Gameplay); });

			prevButton.onClick.AddListener(() =>
			{
				Profile.Level--;
				SceneManager.ReloadScene(SceneID.Gameplay);
			});

			nextButton.onClick.AddListener(() =>
			{
				Profile.Level++;
				SceneManager.ReloadScene(SceneID.Gameplay);
			});
			
			levelButton.onClick.AddListener(() => levelSelector.gameObject.SetActive(!levelSelector.gameObject.activeSelf));
			levelSelector.gameObject.SetActive(false);

			experimentButton.onClick.AddListener(() => experimentSelector.gameObject.SetActive(!experimentSelector.gameObject.activeSelf));
			experimentSelector.gameObject.SetActive(false);
		}

		private int _counter;

		private void Update()
		{
#if UNITY_EDITOR
			if (Input.GetKeyDown(KeyCode.F12))
			{
				TakeScreenShot();
			}
#endif
			if (canvas.enabled || !Config.CheatEnabled)
			{
				return;
			}

			if (!Input.GetMouseButtonDown(0)) return;
			var virtualWidth = 1080.0f / canvas.pixelRect.width;
			var x = Input.mousePosition.x * virtualWidth;
			var y = Input.mousePosition.y * virtualWidth;
			if (x < 200 && y > 1720 && _counter == 0 ||
				x < 200 && y < 200 && _counter >= 1 && _counter < 3 ||
				x > 880 && y < 200 && _counter >= 3 && _counter < 6 ||
				x > 880 && y > 1720 && _counter >= 6 && _counter < 10)
			{
				_counter++;
				if (_counter == 10)
				{
					canvas.enabled = true;
				}
			}
			else
			{
				_counter = 0;
			}
		}
		internal static void Show()
		{
			Instance.canvas.enabled = true;
		}
		internal static void Hide()
        {
			Instance.canvas.enabled = false;
		}
#if UNITY_EDITOR
		private static void TakeScreenShot()
		{
			try
			{
				Directory.CreateDirectory("Preview");
			}
			catch
			{
				// ignored
			}

			ScreenCapture.CaptureScreenshot($"Preview/{Profile.Level:000}.png");
		}
#endif
	}
}