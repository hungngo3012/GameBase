using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NinthArt
{
	internal class Gameplay : Scene
	{
		[SerializeField] private TextMeshProUGUI levelText;
		[SerializeField] private TextMeshProUGUI timeText;
		[SerializeField] private Button settingButton;
		[SerializeField] private Button replayBtn;
		[SerializeField] private Button shopBtn;
		[SerializeField] private Button selectSkinBtn;
		[SerializeField] private Button wheelBtn;
		[SerializeField] private Button rankBtn;
		[SerializeField] private Button addCoinBtn;
		[SerializeField] private CarMoveController carMoveController;
		[SerializeField] private ItemBtn vipSlotBtn;
		[SerializeField] private TextMeshProUGUI coinTxt;
		[SerializeField] private TextMeshProUGUI starTxt;

		[SerializeField] List<GameObject> happyEmoji;
		[SerializeField] List<GameObject> sadEmoji;

		[SerializeField] private RectTransform starProgressBg;
		[SerializeField] private RectTransform starProgressBar;
		[SerializeField] private List<GameObject> disableStars;

		[SerializeField] private GameObject gameplayUi;

		[SerializeField] private Light mainLight;
		[SerializeField] private Light highlightVipSlotLight;

		[SerializeField] TextMeshProUGUI camOSizeTxt;
		[SerializeField] Camera gameplayMainCam;
		//[SerializeField] List<GameObject> starObj;

		public GameObject TutoHand;
		public LevelConfig levelConfig;
		public ColorConfig colorConfig;

		public string testSkin;
		private enum State
		{
			Init,
			Play,
			Win,
			Lose
		}
		internal enum PlayingState
		{
			Normal,
			UsingVipSlot,
			Pause,
		}

		private State _state;
		internal PlayingState _playingState { get; private set; }

		internal static Gameplay Instance { get; private set; }
		internal Level Level { get; private set; }

		internal static int CoinsEarned = 20;
		internal static int StarEarned = 3;
		[SerializeField] private bool isTest;
		[SerializeField] private string levelTest;

		internal LevelStats CurLevelStats;
		internal float LevelTimePlayed = 0;
		private void Init()
		{
			//SoundManager.PlayMusic("game_bgm_0" + UnityEngine.Random.Range(0, 10).ToString(), true);
			if (!string.IsNullOrEmpty(testSkin))
            {
				Profile.UnlockSkin(testSkin);
				Profile.SelectSkin(testSkin);
			}			
			// Load level
			var levelPrefab = Resources.Load<Level>($"Levels/level");
			Level = Instantiate(levelPrefab, transform);

			int countLevel = levelConfig.configs.Count;

			var levelIndex = Profile.Level - 1;
			bool dynamicLevel = false;
			int loopPhase = 0;

			if (Profile.Level > countLevel)
            {
				loopPhase = (Profile.Level - countLevel - 1) / GlobalDefine.numLevelLoop + 1;
				dynamicLevel = true;
				int tmpIndex = (Profile.Level % countLevel - 1) % GlobalDefine.numLevelLoop;
				levelIndex = countLevel - GlobalDefine.numLevelLoop + tmpIndex;
            }				

			if (isTest)
            {
				levelIndex = int.Parse(levelTest);
			}

			var levelJson = LevelManager.LevelsData[levelIndex];
			CurLevelStats = levelConfig.configs[levelIndex];
			levelText.text = $"Level {Profile.Level}";

			StartCoroutine(LoadLevel(levelJson, dynamicLevel, loopPhase));
		}
		IEnumerator LoadLevel(string levelJson, bool dynamicLevel, int loopPhase)
        {
			bool haveProgress = Level.CanLoadProgress();
			yield return StartCoroutine(LoadSkin());
			if(haveProgress)
            {
				yield return StartCoroutine(Level.LoadLevel(levelJson.ToString().Substring("level_".Length), Application.persistentDataPath + GlobalDefine.LevelProgressDataPath, !CurLevelStats.fixColor));
			}				
			else
            {	
				if (Profile.Level == 1)//Tutorial level 1 chỉ xuất hiện một lần, load lại thì không xuất hiện
					TutoHand.SetActive(true);

				yield return StartCoroutine(Level.LoadLevel(levelJson.ToString().Substring("level_".Length), null, !CurLevelStats.fixColor));
			}				

			Config.levelLoaded = true;
			if (dynamicLevel && !haveProgress)
			{
				int diff = (int)CurLevelStats.diff + loopPhase;
				if (diff >= Enum.GetValues(typeof(LevelDifficult)).Length - 1)
					diff = Enum.GetValues(typeof(LevelDifficult)).Length - 1;

				Level.GenLevelPass((LevelDifficult)diff);
				Debug.Log("level: " + Profile.Level + " - Loop phase: " + loopPhase);
			}

			yield return StartCoroutine(Level.Init());
			CoinsEarned = CurLevelStats.coinEarn;// = Level.levelNumPass / 4;
			//LevelTimePlayed = 0;
			Play();
			if (haveProgress)
            {
				yield return new WaitForSeconds(0.01f);
				StartCoroutine(Level.PassToCar());
			}				
			if (Profile.Level == GlobalDefine.showRatePopUpLevel && Config.RatingPopup && !Profile.RatingPopupShown)
			{
				SceneManager.OpenPopup(SceneID.RateUi);
			}
		}			
		internal static LevelSkin levelSkin = new LevelSkin();
		IEnumerator LoadSkin()
        {
			levelSkin.cars = new List<GameObject>();

			levelSkin.cars.Add(SkinConfigService.GetCarSkinModel(Level.skinConfig, CarType.Small, Profile.CurrentSkin));
			levelSkin.cars.Add(SkinConfigService.GetCarSkinModel(Level.skinConfig, CarType.Medium, Profile.CurrentSkin)); 
			levelSkin.cars.Add(SkinConfigService.GetCarSkinModel(Level.skinConfig, CarType.Big, Profile.CurrentSkin));
			levelSkin.cars.Add(SkinConfigService.GetCarSkinModel(Level.skinConfig, CarType.Bus, Profile.CurrentSkin));

			levelSkin.passenger = SkinConfigService.GetPassengerSkinModel(Level.skinConfig, Profile.CurrentSkin);
			levelSkin.basement = SkinConfigService.GetGarageSkinModel(Level.skinConfig, Profile.CurrentSkin);
			levelSkin.belt = SkinConfigService.GetBeltSkinModel(Level.skinConfig, Profile.CurrentSkin);

			if(!Config.levelLoaded)
				yield return null;
		}			
		internal bool WasLost()
        {
			return (_state == State.Lose);
        }			
		internal void Play()
		{
			ChangeState(State.Play);
		}
		internal bool IsPlayState()
        {
			return (_state == State.Play);
        }

		internal void Win()
		{
			Debug.Log("???");
			StarEarned = LevelConfig.GetStarEarn(CurLevelStats, LevelTimePlayed);
			ChangePlayingState(PlayingState.Pause);
			ChangeState(State.Win);
		}		
		internal void Lose()
		{
			Debug.Log("???");
			ChangePlayingState(PlayingState.Pause);
			ChangeState(State.Lose);
		}

		private void Awake()
		{
			Instance = this;
		}

		private void Start()
		{
			if (GameManager.Cheat)
				CheatMenu.Show();

			if (gameplayMainCam == null)
				gameplayMainCam = Camera.main;

			GameManager.Call(Init);
			settingButton.onClick.AddListener(()=> { ChangePlayingState(PlayingState.Pause); SceneManager.OpenPopup(SceneID.SettingUI);SoundManager.PlaySfx("BtnClick"); });
			replayBtn.onClick.AddListener(() => OnClickReplay());
			shopBtn.onClick.AddListener(() => { ChangePlayingState(PlayingState.Pause); SceneManager.OpenScene(SceneID.ShopUi); SoundManager.PlaySfx("BtnClick"); });
			selectSkinBtn.onClick.AddListener(() => { ChangePlayingState(PlayingState.Pause); SceneManager.OpenScene(SceneID.StageUi); SoundManager.PlaySfx("BtnClick"); });
			wheelBtn.onClick.AddListener(() => { ChangePlayingState(PlayingState.Pause); SceneManager.OpenPopup(SceneID.LuckyWheelUi); SoundManager.PlaySfx("BtnClick"); });
			rankBtn.onClick.AddListener(() => { ChangePlayingState(PlayingState.Pause); SceneManager.OpenScene(SceneID.RankingUi); SoundManager.PlaySfx("BtnClick"); });
			addCoinBtn.onClick.AddListener(() => { ChangePlayingState(PlayingState.Pause); SceneManager.OpenScene(SceneID.ShopUi); SoundManager.PlaySfx("BtnClick"); });

			UpdateCoin();
			UpdateStar();
			StartCoroutine(CreateEmojiCountDown());

			// Camera size
			float screenAspect = (float)Screen.width / (float)Screen.height;
			float targetAspect = 1080f / 1920f; // Tỷ lệ mục tiêu (bạn có thể đổi thành tỷ lệ gốc của game bạn)

			// Màn hình cao hơn tỷ lệ gốc
			float difference = targetAspect / screenAspect;
			float size = GlobalDefine.camOrSizeDefault * difference;

			if (size < 16)
				size = 16;

			gameplayMainCam.orthographicSize = size;
			//GameManager.ShowNoti("screen size: " + Screen.width + "x" + Screen.height + " - " + "orthographicSize: " + gameplayMainCam.orthographicSize);
			camOSizeTxt.text = gameplayMainCam.orthographicSize.ToString();

			AdmobBannerController.ShowBanner();
			EventManager.Subscribe(EventType.CoinAmountChanged, UpdateCoin);
			EventManager.Subscribe(EventType.StarAmountChanged, UpdateStar);
		}
		internal void OnClickReplay()
        {
			ChangePlayingState(PlayingState.Pause); 
			SceneManager.OpenPopup(SceneID.ReplayUi); 
			SoundManager.PlaySfx("BtnClick");
		}			
        private void OnDestroy()
        {
			EventManager.Unsubscribe(EventType.CoinAmountChanged, UpdateCoin);
			EventManager.Unsubscribe(EventType.StarAmountChanged, UpdateStar);
		}
        private void ChangeState(State newState)
		{
			if (_state == newState) return;
			ExitOldState();
			_state = newState;
			EnterNewState();
		}
		internal void ChangePlayingState(PlayingState newState)
		{
			if (_playingState == newState) return;
			ExitOldPlayingState();
			_playingState = newState;
			EnterNewPlayingState();
		}
		private void EnterNewPlayingState()
		{
			switch (_playingState)
			{
				case PlayingState.Normal:
					gameplayUi.SetActive(true);
					break;
				case PlayingState.Pause:
					gameplayUi.SetActive(false);
					break;
				case PlayingState.UsingVipSlot:
					gameplayUi.SetActive(false);
					highlightVipSlotLight.gameObject.SetActive(true);
					mainLight.gameObject.SetActive(false);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		private void ExitOldPlayingState()
		{
			switch (_playingState)
			{
				case PlayingState.Normal:
					break;
				case PlayingState.Pause:
					break;
				case PlayingState.UsingVipSlot:
					mainLight.gameObject.SetActive(true);
					highlightVipSlotLight.gameObject.SetActive(false);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void EnterNewState()
		{
			switch (_state)
			{
				case State.Init:
					break;
				case State.Play:
					ChangePlayingState(PlayingState.Normal);
					break;
				case State.Win:
					Analytics.LogLevelCompleteEvent();
					gameplayUi.SetActive(false);
					SceneManager.ClosePopups();
					ChangePlayingState(PlayingState.Pause);
					SceneManager.OpenPopup(SceneID.WinUI);
					break;
				case State.Lose:
					Analytics.LogLevelFailEvent();

					SceneManager.ClosePopups();
					ChangePlayingState(PlayingState.Pause);
					SceneManager.OpenPopup(SceneID.LoseUI);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void ExitOldState()
		{
			switch (_state)
			{
				case State.Init:
					break;
				case State.Play:
					break;
				case State.Win:
					break;
				case State.Lose:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void Update()
		{
			//SoundManager.PlayRandomMusics();
			switch (_state)
			{
				case State.Init:
					break;
				case State.Play:
					if(_playingState != PlayingState.Pause)
                    {
						carMoveController.DoFrame(_playingState == PlayingState.UsingVipSlot);
						LevelTimePlayed += Time.deltaTime;
						UpdateTimeTxt();
						UpdateStarDisplay();
					}						
					//Emoji
					if(!emojiCreated)
						CreateEmoji();
					break;
				case State.Win:
					break;
				case State.Lose:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		bool emojiCreated = true;
		internal void UpdateTimeTxt()
        {
			timeText.text = GeneralCalculate.FormatTime(LevelTimePlayed);
		}
		internal void UpdateStarDisplay()
		{
			var width = starProgressBg.sizeDelta.x;
			var size = starProgressBar.sizeDelta;
			float ratio = LevelConfig.GetStarProgress(CurLevelStats, LevelTimePlayed);

			size.x = width * ratio;
			if (ratio <= 0.5f && !disableStars[0].activeSelf)
				disableStars[0].SetActive(true);
			if(ratio <= 0.0f && !disableStars[1].activeSelf)
				disableStars[1].SetActive(true);

			starProgressBar.sizeDelta = size;
		}
		void CreateEmoji()
        {
			emojiCreated = true;
			bool emojiType = UnityEngine.Random.Range(0, 2) < 1;
			Vector3 pos = new Vector3();
			GameObject prefab = null;
			Passanger emotionPass = null;
			if(emojiType)
            {
				prefab = happyEmoji[UnityEngine.Random.Range(0, happyEmoji.Count)];
				PointStop rndPointStop = Level.GetRandomHavePassPointStop();
				if(rndPointStop != null)
                {
					PosPassCar rndPosPassCar = rndPointStop.CarController.GetRndBusySlot();
					if(rndPosPassCar != null)
                    {
						emotionPass = rndPosPassCar.sittingPass;
					}						
				}					
			}
			
			if(emotionPass == null)
            {
				prefab = sadEmoji[UnityEngine.Random.Range(0, sadEmoji.Count)];
				emotionPass = Level.GetRndInQueuePassenger();
			}		
			if(emotionPass != null)
            {
				GameObject obj = Instantiate(prefab, emotionPass.transform);
				obj.transform.localPosition = new Vector3(0.0f, 2.0f, 0.0f);
				obj.transform.rotation = Quaternion.Euler(45.0f, 0.0f, 0.0f);
				StartCoroutine(CreateEmojiCountDown());
			}
		}
		IEnumerator CreateEmojiCountDown()
        {
			yield return new WaitForSeconds(UnityEngine.Random.Range(GlobalDefine.emojiWaitCreat, GlobalDefine.emojiWaitCreat * 1.5f));
			emojiCreated = false;
        }			
		public static CarController GetMostPriorityCar(List<CarController> carControllers)
		{
			List<CarController> shortList = new List<CarController>();
			int minEmptySlot = 10000;

			foreach(CarController carController in carControllers)
            {
				int numEmptySlot = carController.GetNumEmptySlot();
				if (numEmptySlot > minEmptySlot)
					continue;
				if (numEmptySlot == minEmptySlot)
                {
					shortList.Add(carController);
					continue;
				}

				shortList.Clear();
				shortList.Add(carController);
				minEmptySlot = numEmptySlot;
            }				

			if(shortList.Count <= 0)
				return null;

			List<CarController> shortList1 = new List<CarController>();
			int minCarType = (int)CarType.Bus;
			foreach (CarController car in shortList)
            {
				if ((int)car.carType > minCarType)
					continue;
				if ((int)car.carType == minCarType)
                {
					shortList1.Add(car);
					continue;
                }

				shortList1.Clear();
				shortList1.Add(car);
				minCarType = (int)car.carType;
            }

			if (shortList1.Count > 0)
				return shortList1[0];
			return null;
		}
		internal void UseSort()
        {
			Level.SortTool();// ko cần lưu vì sẽ có xe hoàn thành -> lưu luôn khi xe hoàn thành
		}
		internal void UseShuffle()
        {
			//Đảo màu các xe cùng loại
			Level.ShuffleCars();
		}
		internal void UseVipSlot(bool inGame = true)
        {
			Level.UnlockVipSlot();
			if(inGame)
				ChangePlayingState(PlayingState.UsingVipSlot);
			vipSlotBtn.gameObject.SetActive(false);
			if (inGame)
				Level.SaveGameProgress();
        }
		public void ContinuePlay()
        {
			Play();
			ChangePlayingState(PlayingState.Pause);
			UseSort();
			PointStop pointStop = Level.GetNearestLockedPointStop();
			if(pointStop != null)
				pointStop.UnlockPointStop();
		}	
		public static void OpenPopupByName(string itemId)
        {
			SceneManager.OpenPopup(SceneManager.SceneId(itemId));
        }	
		public void TestSelectSkin(string id)
        {
			Profile.UnlockSkin(id);
			Profile.SelectSkin(id);
        }
		public void TestUnlockAllSkin()
		{
			foreach(SkinModelConfig skinModelConfig in Level.skinConfig.skins)
            {
				Profile.UnlockSkin(skinModelConfig.skinId);
			}				
		}
		void UpdateCoin(object o = null)
        {
			coinTxt.text = GeneralCalculate.FormatPoints(Profile.CoinAmount, 1);
		}
		void UpdateStar(object o = null)
		{
			starTxt.text = GeneralCalculate.FormatPoints(Profile.StarAmount);
		}		
		[ContextMenu("cheat star")]
		public void CheatStar()
        {
			Profile.StarAmount += 100;
			Profile.StarCollected += 100;
        }	
		internal void OnClickVipSlot()
        {
			vipSlotBtn.OnClickUseItem();
        }	
		public void ChangeCamOSize(int val)
        {
			gameplayMainCam.orthographicSize += val;
			camOSizeTxt.text = gameplayMainCam.orthographicSize.ToString();
		}
	}
}
[Serializable]
public class LevelSkin
{
	public List<GameObject> cars = new List<GameObject>();

	public GameObject passenger;
	public GameObject basement;
	public GameObject belt;
}