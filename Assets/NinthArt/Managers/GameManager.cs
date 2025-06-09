
using System;
using System.Collections.Generic;
using Facebook.Unity;
using UnityEngine;
using GameAnalyticsSDK;
using Unity.Services.Core;
using GoogleMobileAds.Api;

#if !UNITY_EDITOR
using Firebase;
using Firebase.Analytics;
#endif

namespace NinthArt
{
	internal class GameManager : Singleton<GameManager>
	{
		public IapItems iapItems;
#if !UNITY_EDITOR
		internal static bool FirebaseOk { get; private set; }
#endif
		private enum State
		{
			None,
			InitializingFirebase,
			InitializingConfig,
			Initialized
		}

		private State _state = State.None;
		private readonly Queue<Action> _queue = new();

		private void Start()
		{
			if (_state != State.Initialized && _queue.Count <= 0)
			{ // Don't open Gameplay if we don't start from the scene GameManager
				UMPManager.Instance.InitUMP(() =>
				{
					Call(() =>
					{
						SceneManager.OpenScene(SceneID.LoadingScene);
					});

				}, () =>
				{
					Time.timeScale = 0;
				}, () =>
				{
					Time.timeScale = 1;
				});
			}
		}
		public bool cheat;
		public static bool Cheat => Instance.cheat;
		internal static void Call(Action function)
		{
			switch (Instance._state)
			{
				case State.None:
					Instance._state = State.InitializingFirebase;
					Application.targetFrameRate = 60;
					GameAnalytics.Initialize();

					MobileAds.Initialize((InitializationStatus initStatus) =>
					{
						AppOpenAdController.Instance.LoadAOA();
					});

					//FB.Init(LogFBVersion);
					IAP.Instance.Init();
#if UNITY_EDITOR
					//CheatMenu.Show();
#else
					FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
					{
						if (task.Result != DependencyStatus.Available)
						{
							return;
						}
						FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
						FirebaseOk = true;
						
						FirebaseApp app = FirebaseApp.DefaultInstance;
						// Log ra th�ng tin Name v� AppId
						/*
						ShowNoti("Firebase Name: " + app.Name.ToString() + "\n" + 
						"Firebase App ID: " + app.Options.AppId.ToString() + "\n" + 
						"Project ID: + app.Options.ProjectId.ToString()");*/
					});
#endif
					if (function != null) Instance._queue.Enqueue(function);
					break;
				case State.InitializingFirebase:
				case State.InitializingConfig:
					if (function != null) Instance._queue.Enqueue(function);
					break;
				case State.Initialized:
					function?.Invoke();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		static void LogFBVersion()
		{
			Debug.Log("Facebook SDK Version: " + FacebookSdkVersion.Build);
		}
		private void Update()
		{
			switch (_state)
			{
				case State.None:
					break;
				case State.InitializingFirebase:
#if !UNITY_EDITOR
					if (FirebaseOk)
#endif
					{
						_state = State.InitializingConfig;
						Config.Init();
					}
					break;
				case State.InitializingConfig:
					if (Config.Initialized)
					{
						_state = State.Initialized;
						enabled = false;
						while (_queue.Count > 0)
						{
							var onComplete = _queue.Dequeue();
							onComplete?.Invoke();
						}
					}
					break;
				case State.Initialized:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		string notify;
		Action<object> confirmCallBack;

		public static string Notify => Instance.notify;
		public static Action<object> ConfirmCallBack => Instance.confirmCallBack;
		public static void ShowNoti(string mess, Action<object> callback = null)
		{
			Instance.notify = mess;
			if (callback != null)
				Instance.confirmCallBack = callback;
			//open Noti Popup
			SceneManager.OpenPopup(SceneID.NotiUi);
		}
		public static void ClearNotify()
		{
			Instance.notify = null;
			Instance.confirmCallBack = null;
		}
	}
}