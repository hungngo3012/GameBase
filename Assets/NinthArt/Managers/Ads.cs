
using System;
using System.Collections;
using System.Collections.Generic;
using AppsFlyerSDK;
using Firebase.RemoteConfig;
using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
using Facebook.Unity;
#endif

namespace NinthArt
{
    internal enum RewardedVideoState
    {
#if !UNITY_EDITOR
		Closed, NotReady,
#endif
        Failed, Watched
    }

    internal class Ads : Singleton<Ads>
    {
        internal const string SdkName = "IronSource";

#if UNITY_IOS && !UNITY_EDITOR
		[DllImport("__Internal")] private static extern bool isIos14();
		[DllImport("__Internal")] private static extern bool advertiserTrackingPrompted();
		[DllImport("__Internal")] private static extern void promptAdvertiserTracking();
		[DllImport("__Internal")] private static extern bool advertiserTrackingEnabled();
#endif
        private const float InterstitialLoadDelayTime = 1.0f;
        private bool _interstitialShown;

        private float _lastInterstitialShowTime;
        private float _lastRewardedVideoShowTime;

        private bool InterstitialAllowed { get; set; } = true;
        private bool BannerAllowed { get; set; } = true;

        private enum State { NotInitialized, Initializing, Initialized }
        private State _state = State.NotInitialized;

        internal void Init()
        {
            if (_state != State.NotInitialized)
            {
                return;
            }
            _state = State.Initializing;
            _lastInterstitialShowTime = Time.realtimeSinceStartup;

            Debug.Log("ads init: ironsource event");
            //Add ImpressionSuccess Event
            IronSourceEvents.onImpressionDataReadyEvent += ImpressionDataSuccessEvent;
            //IronSourceEvents.onImpressionDataReadyEvent += OnImpressionDataReady;

            IronSourceEvents.onSdkInitializationCompletedEvent += OnSdkInitializationCompletedEvent;

            IronSourceRewardedVideoEvents.onAdShowFailedEvent += OnRewardedVideoAdShowFailed;
            IronSourceRewardedVideoEvents.onAdOpenedEvent += OnRewardedVideoAdOpened;
            IronSourceRewardedVideoEvents.onAdClosedEvent += OnRewardedVideoAdClosed;
            IronSourceRewardedVideoEvents.onAdRewardedEvent += OnRewardedVideoAdRewarded;
            IronSourceRewardedVideoEvents.onAdClickedEvent += OnRewardedVideoAdClicked;
            IronSourceRewardedVideoEvents.onAdAvailableEvent += OnRewardedVideoAvailabilityChanged;
            IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;

            IronSourceInterstitialEvents.onAdReadyEvent += OnInterstitialAdReady;
            IronSourceInterstitialEvents.onAdLoadFailedEvent += OnInterstitialAdLoadFailed;
            IronSourceInterstitialEvents.onAdOpenedEvent += OnInterstitialAdOpened;
            IronSourceInterstitialEvents.onAdClosedEvent += OnInterstitialAdClosed;
            IronSourceInterstitialEvents.onAdShowSucceededEvent += OnInterstitialAdShowSucceeded;
            IronSourceInterstitialEvents.onAdShowFailedEvent += OnInterstitialAdShowFailed;
            IronSourceInterstitialEvents.onAdClickedEvent += OnInterstitialAdClicked;
            #region Banner
            IronSourceBannerEvents.onAdLoadedEvent += BannerAdLoadedEvent;
            IronSourceBannerEvents.onAdLoadFailedEvent += BannerAdLoadFailedEvent;
            IronSourceBannerEvents.onAdClickedEvent += BannerAdClickedEvent;
            #endregion

            EventManager.Subscribe(EventType.NoAdsPurchased, HideBanner);

#if !UNITY_EDITOR && UNITY_IOS
			if (isIos14())
			{
				promptAdvertiserTracking();
			}
			else
			{
				InitSDK();
			}
#else
            InitSDK();
#endif
        }

#if !UNITY_EDITOR && UNITY_IOS
		private void Update()
		{
			if (!advertiserTrackingPrompted())
			{
				return;
			}
			InitSDK();
		}
#endif

        private void InitSDK()
        {
            enabled = false;
            var userId = IronSource.Agent.getAdvertiserId();
            IronSource.Agent.setUserId(userId);
            //test suite
            //IronSource.Agent.setMetaData("is_test_suite", "enable"); 

            IronSource.Agent.setConsent(true);
            IronSource.Agent.setMetaData("do_not_sell", "false");
            IronSource.Agent.setMetaData("is_child_directed", "false");

#if !UNITY_EDITOR
			Firebase.Analytics.FirebaseAnalytics.SetUserId(userId);
#if UNITY_IOS
			FB.Mobile.SetAdvertiserTrackingEnabled(advertiserTrackingEnabled());
#endif
#endif
            Adjust.Init();
            IronSource.Agent.init(Config.IronSourceId);
            IronSource.Agent.validateIntegration();
            StartCoroutine(LoadInterstitialWithDelay(InterstitialLoadDelayTime));
            //LoadBanner();
            _state = State.Initialized;
#if DEBUG_ENABLED
			IronSource.Agent.validateIntegration();
#endif
        }

        private static void LoadInterstitial()
        {
            //Debug.Log("call load Interstitial ads");
            IronSource.Agent.loadInterstitial();
            AppsFlyer.sendEvent($"af_inters_api_called", null);
        }

        private static IEnumerator LoadInterstitialWithDelay(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            LoadInterstitial();
        }

        private Action<RewardedVideoState> _rewardedVideoCallback;
        private RewardedVideoState _rewardedVideoState;
        private string _interstitialPlace;
        private string _rewardedVideoPlace;

#if UNITY_EDITOR
        internal static bool RewardedVideoReady => true;
#else
		internal static bool RewardedVideoReady => Instance._state == State.Initialized && IronSource.Agent.isRewardedVideoAvailable();
#endif

        private bool InterstitialValid => _state == State.Initialized;

        private void ShowReadyInterstitial(Action onFinished)
        {
            //Debug.LogError("call show ready inter");
            if (!InterstitialAllowed)
            {
                onFinished();
                return;
            }

            try
            {
                _onIntersitialRequestProcessed = onFinished;
                //Debug.LogError("call irs show inter");
                //GameManager.ShowNoti("call irs show inter");
                IronSource.Agent.showInterstitial("FS" + Config.InterstitialCappingTime);
                AppsFlyer.sendEvent($"af_inters_displayed", null);
            }
            catch
            {
                onFinished?.Invoke();
            }
        }

        public static bool CanShowInterstitial
        {
            get
            {
                if (!Config.InternetConnected)
                {
                    //GameManager.ShowNoti("cant show inter: internet not connected");
                    return false;
                }
                if (Profile.Vip)
                {
#if DEBUG_ENABLED
					Debug.LogError("Cannot show interstitial to VIP");
#endif
                    //GameManager.ShowNoti("cant show inter: vip");
                    return false;
                }

                // Check availability
                if (!Instance.InterstitialValid)
                { // Not ready yet
#if DEBUG_ENABLED
					Debug.LogError("Interstitial is not either initialized or loaded");
#endif
                    //GameManager.ShowNoti("cant show inter: !InterstitialValid");
                    return false;
                }

                //Check AOA
                if (Config.Instance.isAoa)
                {
                    Config.Instance.isAoa = false;
                    //GameManager.ShowNoti("show inter: isAoa");
                    return true;
                }

                // Check capping
                if (Time.realtimeSinceStartup - Instance._lastRewardedVideoShowTime < Config.InterstitialRewardedVideoCappingTime)
                {
#if DEBUG_ENABLED
					var t = Time.realtimeSinceStartup - _lastRewardedVideoShowTime;
					Debug.LogError("Rewarded video opened " + t + " seconds ago. Need to wait " +
						(Config.InterstitialRewardedVideoCappingTime - t) + " seconds to show interstitial");
#endif
                    //GameManager.ShowNoti("cant show inter: capping reward");
                    return false;
                }
                if (!Instance._interstitialShown)
                {
                    if (Time.realtimeSinceStartup - Instance._lastInterstitialShowTime < Config.FirstInterstitialCappingTime)
                    {
#if DEBUG_ENABLED
							var t = Time.realtimeSinceStartup - _lastInterstitialShowTime;
							Debug.LogError("Need wait " +
								(Config.FirstInterstitialCappingTime - t) + " seconds to show interstitial");
#endif
                        //GameManager.ShowNoti("cant show inter: first capping");
                        return false;
                    }
                }
                else
                {
                    if (Time.realtimeSinceStartup - Instance._lastInterstitialShowTime < Config.InterstitialCappingTime)
                    {
#if DEBUG_ENABLED
							var t = Time.realtimeSinceStartup - _lastInterstitialShowTime;
							Debug.LogError("Interstitial opened " + t + " seconds ago. Need to wait " +
								(Config.InterstitialRewardedVideoCappingTime - t) + " seconds to show interstitial");
#endif
                        //GameManager.ShowNoti("cant show inter: capping - " + (Time.realtimeSinceStartup - Instance._lastInterstitialShowTime));
                        return false;
                    }
                }
                return true;
            }
        }
        internal static void ShowReadyInter(Action onFinished = null)
        {
            Instance.ShowReadyInterstitial(onFinished);
        }
        public static bool CappingAOAResume = false;
        internal static bool ShowInterstitial(string place, Action onFinished = null, bool loadAds = false)
        {
            //Debug.Log("call show inter");
            //GameManager.ShowNoti("call show inter");
            if (!CanShowInterstitial || Config.IsLoading || CappingAOAResume)
            {
                //Debug.Log("cant show inter: " + Config.InternetConnected + " - " + Profile.Vip + " - " + Instance.InterstitialValid + " - " + Config.Instance.isAoa + " - " 
                //+ (Time.realtimeSinceStartup - Instance._lastRewardedVideoShowTime < Config.InterstitialRewardedVideoCappingTime) + " - " + Instance._interstitialShown);
                onFinished?.Invoke();
                return false;
            }

            Instance._interstitialPlace = place;
            Debug.Log(place);
            if (IronSource.Agent.isInterstitialReady())
            {
                if (loadAds)
                    SceneManager.OpenScene(SceneID.LoadResumeAds);
                else
                    Instance.ShowReadyInterstitial(onFinished);

                AppsFlyer.sendEvent($"af_inters_displayed", null);
                if (Profile.InterCount < 20)
                {
                    AppsFlyer.sendEvent($"af_inters_displayed_{Profile.InterCount + 1}_times", null);
                    Profile.InterCount++;
                }
                return true;
            }
            onFinished?.Invoke();
            return false;
        }
        private static void ShowMessage(string msg)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
			AndroidJavaObject activity =
			new AndroidJavaClass("com.unity3d.player.UnityPlayer").
			GetStatic<AndroidJavaObject>("currentActivity");

			AndroidJavaObject toastClass = new AndroidJavaClass("android.widget.Toast");
			toastClass.CallStatic<AndroidJavaObject>("makeText", activity, msg, toastClass.GetStatic<int>("LENGTH_SHORT")).Call("show");
#else
            Debug.LogWarning(msg);
#endif
        }
        bool displayingShowRewardedVideoFailMessage;
        private static void ShowRewardedVideoFailMessage()
        {
            if (Instance.displayingShowRewardedVideoFailMessage)
                return;
            Instance.displayingShowRewardedVideoFailMessage = true;
            ShowMessage(Application.internetReachability == NetworkReachability.NotReachable
                ? "No internet connection. Try again"
                : "No video available at the moment. Try again later");
            Instance.ResetDisplayingShowRewardedVideoFailMessage();
        }
        void ResetDisplayingShowRewardedVideoFailMessage()
        {
            Invoke("DisplayRewardedVideoFailMessageComplete", 3.0f);
        }
        void DisplayRewardedVideoFailMessageComplete()
        {
            displayingShowRewardedVideoFailMessage = false;
        }
        internal static void ShowRewardedVideo(string place, Action<RewardedVideoState> callback)
        {
            Instance._rewardedVideoPlace = place;
            /*
            if(Profile.Vip)
            {
                callback(RewardedVideoState.Watched);
                return;
            }*/    
#if UNITY_EDITOR
            callback(RewardedVideoState.Watched);
#else
			if (Instance._rewardedVideoCallback != null)
			{ // Previous rewarded video request is not finished yet
				callback(RewardedVideoState.Closed);
				return;
			}
			if (!RewardedVideoReady || !Config.Instance.internetConnected)
			{
				Analytics.LogRewardedVideoFailedEvent(place);
				callback(RewardedVideoState.NotReady);
				ShowRewardedVideoFailMessage();
				return;
			}
			Analytics.LogRewardVideoClickedEvent(place);
			Adjust.TrackEvent(Adjust.RwClicked);
			AppsFlyer.sendEvent($"af_rewarded_api_called", null);
			Instance._rewardedVideoState = RewardedVideoState.Closed;
			Instance._rewardedVideoCallback = callback;
			SceneManager.ShowLoading();
			if (IronSource.Agent.isRewardedVideoAvailable())
			{
				IronSource.Agent.showRewardedVideo();
                Debug.Log("show rewared video");
			}
#endif
        }

        private void OnApplicationPause(bool isPaused)
        {
            IronSource.Agent.onApplicationPause(isPaused);
        }

        private void OnRewardedVideoFailed()
        {
            ShowRewardedVideoFailMessage();
            SceneManager.HideLoading();
            _rewardedVideoCallback?.Invoke(RewardedVideoState.Failed);
            _rewardedVideoCallback = null;
            ShowMessage("Video failed to show. Please retry");
        }

        private void OnRewardedVideoAdShowFailed(IronSourceError error, IronSourceAdInfo irsAdInfo)
        {
            Analytics.LogRewardedVideoFailedEvent(_rewardedVideoPlace);
            OnRewardedVideoFailed();
            ShowMessage("Video failed to show. Please retry");
        }

        private void OnRewardedVideoAdOpened(IronSourceAdInfo irsAdInfo)
        {
            Analytics.LogRewardedVideoShownEvent(_rewardedVideoPlace);
            Adjust.TrackEvent(Adjust.RwShown);
            AppsFlyer.sendEvent($"af_rewarded_ad_displayed", null);
            SceneManager.HideLoading();
            SoundManager.Pause();
            Config.canShowResumeAfterShowReward = false;
            Debug.Log("rewarded video opened");
        }

        private void OnRewardedVideoAdClosed(IronSourceAdInfo irsAdInfo)
        {
            Invoke("ResetShowResumeAfterReward", GlobalDefine.resetShowResumeAfterReward);
            if (_rewardedVideoState == RewardedVideoState.Watched)
            {
                _lastRewardedVideoShowTime = Time.realtimeSinceStartup;
                Analytics.LogRewardVideoWatchedEvent(_rewardedVideoPlace);
                Adjust.TrackEvent(Adjust.RwWatched);
                Debug.Log("reward from ads");
            }
            SceneManager.HideLoading();
            SoundManager.Resume();
            _rewardedVideoCallback?.Invoke(_rewardedVideoState);
            _rewardedVideoCallback = null;
            Debug.Log("close reward ads: " + irsAdInfo.auctionId);
        }
        void ResetShowResumeAfterReward()
        {
            Config.canShowResumeAfterShowReward = true;
        }
        private static void OnRewardedVideoAdStarted()
        {
            Debug.Log("rewarded video ad started");
        }

        private void OnRewardedVideoAdEnded()
        {
            _rewardedVideoState = RewardedVideoState.Watched; Debug.Log("rewarded video ad ended");
        }

        private void OnRewardedVideoAdRewarded(IronSourcePlacement placement, IronSourceAdInfo irsAdInfo)
        {
            _rewardedVideoState = RewardedVideoState.Watched; Debug.Log("rewarded video ad rewarded");
        }

        private static void OnRewardedVideoAdClicked(IronSourcePlacement placement, IronSourceAdInfo irsAdInfo)
        {
            Debug.Log("rewarded video clicked");
        }

        private static void OnRewardedVideoAvailabilityChanged(IronSourceAdInfo adInfo)
        {
            Debug.Log("rewarded video availability changed");
        }

        private Action _onIntersitialRequestProcessed;

        private void OnInterstitialAdReady(IronSourceAdInfo irsAdInfo)
        {
            Debug.Log("Interstitial Ads ready");
        }

        private void OnInterstitialAdLoadFailed(IronSourceError error)
        {
            Debug.LogError("load Interstitial Ads fail");
            LoadInterstitial();
        }

        private static void OnInterstitialAdOpened(IronSourceAdInfo irsAdInfo)
        {
            Debug.Log("Interstitial Ads opened");
        }

        private void OnInterstitialAdClosed(IronSourceAdInfo irsAdInfo)
        {
            SoundManager.Resume();
            LoadInterstitial();
            _lastInterstitialShowTime = Time.realtimeSinceStartup;
            _onIntersitialRequestProcessed?.Invoke();
            _onIntersitialRequestProcessed = null;
        }
        public void ResetLastInterShowTime()
        {
            _lastInterstitialShowTime = Time.realtimeSinceStartup;
            //GameManager.ShowNoti("Reset after show aoa resume");
        }

        private void OnInterstitialAdShowSucceeded(IronSourceAdInfo irsAdInfo)
        {
            SoundManager.Pause();
            Analytics.LogInterstitialShownEvent(_interstitialPlace);
            _interstitialShown = true;
        }

        private void OnInterstitialAdShowFailed(IronSourceError error, IronSourceAdInfo irsAdInfo)
        {
            Analytics.LogInterstitialFailedEvent(_interstitialPlace);
            _onIntersitialRequestProcessed?.Invoke();
            _onIntersitialRequestProcessed = null;
        }

        private static void OnInterstitialAdClicked(IronSourceAdInfo irsAdInfo)
        {
            Debug.Log("Interstitial Ads clicked");
        }

        public static float ShowLastInterstitialShowTime()
        {
            return Instance._lastInterstitialShowTime;
        }  

        #region Banner

        private bool _isBannerReady;

        public bool CanShowBanner =>
            !Profile.Vip &&
            Config.BannerEnabled &&
            _state == State.Initialized &&
            _isBannerReady;

        float _bannerHeight;
        public static float BannerHeight
        {
            get => Instance._bannerHeight;
            set => Instance._bannerHeight = value;
        }
        public static void LoadBanner()
        {
            if (Profile.Vip) return;
            if (!Config.BannerEnabled)
            {
                Debug.LogError("Banner Enabled: " + Config.BannerEnabled);
                return;
            }
            if (Instance._isBannerReady)
            {
                Debug.LogError("Banner Ready: " + Instance._isBannerReady);
                return;
            }
            //Debug.Log("load banner ironsource");
            IronSourceBannerSize ironSourceBannerSize = IronSourceBannerSize.SMART;

            float Width = IronSource.Agent.getDeviceScreenWidth();
            //float Width = Screen.width;
            float Height = IronSource.Agent.getMaximalAdaptiveHeight(Width);
            if (Height > BannerHeight)
                BannerHeight = Height;

            ISContainerParams isContainerParams = new ISContainerParams { Width = Width, Height = Height };
            ironSourceBannerSize.setBannerContainerParams(isContainerParams);
            ironSourceBannerSize.SetAdaptive(true);

            Debug.Log("banner size: " + Width + " - " + BannerHeight);
            IronSource.Agent.loadBanner(ironSourceBannerSize, IronSourceBannerPosition.BOTTOM);
        }
        internal static void ShowBanner()
        {
            if (!Config.InternetConnected) return;
            if (!Instance.BannerAllowed)
            {
                Debug.LogError("Banner allowed: " + Instance.BannerAllowed);
                return;
            }
            if (!Instance.CanShowBanner)
            {
                Debug.LogError("Can Show Banner: " + Instance.CanShowBanner + " - is banner ready: " + Instance._isBannerReady);
                LoadBanner();
                return;
            }
            if (Instance._isBannerReady)
            {
                Debug.Log("Banner Ready: " + Instance._isBannerReady);
                try
                {
                    // TODO: Show Banner shield
                    IronSource.Agent.displayBanner();
                }
                catch
                {
                    // ignored
                    //Debug.Log("Show banner catch");
                }

                Analytics.LogBannerShownedEvent();

                if (!Profile.Vip)
                {
                    EventManager.Annouce(EventType.ShowBannerAds);

                    Config.ShowingBanner = true;
                    Debug.Log("banner loaded: " + Config.ShowingBanner);
                }
            }
            else
            {
                //LoadBanner();
            }
        }

        internal void HideBanner(object o = null)
        {
            try
            {
                // TODO: Hide Banner shield
                //if (Profile.Vip) return;
                IronSource.Agent.hideBanner();
                Config.ShowingBanner = false;
            }
            catch
            {
                // ignored
            }
        }

        private void BannerAdClickedEvent(IronSourceAdInfo irsAdInfo)
        {
            Config.Instance.bannerClicked = true;
            StartCoroutine(RejectBannerClick());
            Analytics.LogBannerClickedEvent();
        }
        private void BannerAdLoadFailedEvent(IronSourceError obj)
        {
            Debug.LogError("load banner fail");
            Analytics.LogBannerFailedEvent();
            _isBannerReady = false;

            /*
            if (!waitToReload)
                StartCoroutine(ReloadBanner());*/
        }

        bool waitToReload = false;
        private IEnumerator ReloadBanner()
        {
            if (waitToReload)
                yield break;

            waitToReload = true;
            yield return new WaitForSeconds(GlobalDefine.reloadBannerTime);
            waitToReload = false;
            LoadBanner();
        }

        private void BannerAdLoadedEvent(IronSourceAdInfo irsAdInfo)
        {
            _isBannerReady = true;
            Debug.Log($"isBannerReady {_isBannerReady}");

            AdmobBannerController.ShowBanner();
        }

        private IEnumerator RejectBannerClick()
        {
            yield return new WaitForSeconds(1f);
            Config.Instance.bannerClicked = false;
        }

        #endregion

        private void ImpressionDataSuccessEvent(IronSourceImpressionData impressionData)
        {
            Debug.Log("call impression data ironsource");
            if (impressionData != null)
            {
                Firebase.Analytics.Parameter[] AdParameters = {
                    new Firebase.Analytics.Parameter("ad_platform", "ironSource"),
                    new Firebase.Analytics.Parameter("ad_source", impressionData.adNetwork),
                    new Firebase.Analytics.Parameter("ad_format", impressionData.adUnit),
                    new Firebase.Analytics.Parameter("ad_unit_name", impressionData.instanceName),
                    new Firebase.Analytics.Parameter("currency","USD"),
                    new Firebase.Analytics.Parameter("value", (double) impressionData.revenue.Value)
                };
                Debug.Log("unity-script:  Impression Data Success Event impressionData = " + impressionData);
                Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", AdParameters);

                Dictionary<string, string> additionalParams = new Dictionary<string, string>();
                additionalParams.Add(AFAdRevenueEvent.AD_TYPE, impressionData.adUnit);

                AppsFlyerAdRevenue.logAdRevenue("ironsource", AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeIronSource,
                    impressionData.revenue.Value, "USD", additionalParams);
            }
        }
        void OnImpressionDataReady(IronSourceImpressionData impressionData)
        {
            Debug.Log("unity - script: I got ImpressionDataReadyEvent ToString(): " + impressionData.ToString());
            Debug.Log("unity - script: I got ImpressionDataReadyEvent allData: " + impressionData.allData);
        }
        private void OnSdkInitializationCompletedEvent()
        {
            Debug.Log("Ironsource Sdk Initialization Completed");
            //test suite
            //IronSource.Agent.launchTestSuite();
        }
        void RewardedVideoOnAdUnavailable()
        {
            Debug.Log("unity-script: I got RewardedVideoOnAdUnavailable");
        }
        private void OnDestroy()
        {
            EventManager.Unsubscribe(EventType.NoAdsPurchased, HideBanner);
        }
    }
}
