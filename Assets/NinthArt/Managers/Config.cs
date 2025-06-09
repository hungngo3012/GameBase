using System;
using System.Threading.Tasks;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using GameAnalyticsSDK;
using UnityEngine;

#if !UNITY_EDITOR
using System.Globalization;
using GameAnalyticsSDK;
#endif

namespace NinthArt
{
    internal class Config : Singleton<Config>
    {
        public bool isTest;
#if UNITY_ANDROID
        private const string IronSrcID = "20343b1ed";
#else
		public const string IronSrcID = "179482e85";
#endif

        internal static float InterstitialCappingTime { get; private set; } = 25f;
        internal static float FirstInterstitialCappingTime { get; private set; } = 25f;
        internal static float InterstitialRewardedVideoCappingTime { get; private set; } = 25f;

        internal static string IronSourceId { get; private set; } = IronSrcID;

        internal static bool CheatEnabled { get; private set; } = true;
        internal static bool BannerEnabled { get; private set; } = true;

        public float ads_interval;
        public bool resume_ads;
        public bool show_open_ads_resume;
        public bool rating_popup; //TODO: lam popUp rating ui
        public bool show_open_ads;
        public bool show_open_ads_first_open;
        public float level_show_inter;
        public bool hien_qc;

        public bool isAoa, bannerClicked;
        public bool internetConnected;
        public bool isLoading = true;
        public bool returnHome;
        public bool onShop;

        public float af_net_IAP_android;
        public float af_net_IAP_ios;

        //public bool show_collab_ad = true;
        //public float time_reload_collap_ad = 30;
        //public float collaps_interval = 1;
        //public bool show_mrec_admob = true;
        //public bool show_native_admob = true;

        public static bool configLoaded = false;
        public static bool canShowResumeAfterShowReward = true;
        public static bool canShowResumeAfterShowAOA = true;
        public static bool levelLoaded;
        public static bool replayLevel;
        public static bool RatingPopup
        {
            get => Instance.rating_popup;
            set => Instance.rating_popup = value;
        }
        public static bool InternetConnected
        {
            get => Instance.internetConnected;
            set => Instance.internetConnected = value;
        }
        public static bool IsLoading
        {
            get => Instance.isLoading;
            set => Instance.isLoading = value;
        }
        public bool showingBanner;
        public static bool ShowingBanner
        {
            get => Instance.showingBanner;
            set => Instance.showingBanner = value;
        }
        public static float LevelShowInter
        {
            get => Instance.level_show_inter;
            set => Instance.level_show_inter = value;
        }
        internal enum SettingPhase
        {
            OnHome,
            OnGamePlay,
        }

        private SettingPhase _settingPhase = SettingPhase.OnHome;

        public SettingPhase sPhase
        {
            get => _settingPhase;

            set => _settingPhase = value;
        }

        private enum State
        {
            None,
            Initializing,
            Initialized
        }

        private State _state = State.None;
        internal static bool Initialized => Instance._state == State.Initialized;

        private void Awake()
        {
            SetConfigDefault();
            if(!isTest)
                FetchDataAsync();
            else
                configLoaded = true;
        }

        private void SetConfigDefault()
        {
            ads_interval = 25;
            resume_ads = true;
            show_open_ads_resume = false;
            rating_popup = true;
            show_open_ads = true; // temp to test => release is false
            show_open_ads_first_open = true;
            level_show_inter = 10;
            hien_qc = true;
            InterstitialCappingTime = ads_interval;
            FirstInterstitialCappingTime = ads_interval;

            //show_collab_ad = true;
            //time_reload_collap_ad = 30;
            //collaps_interval = 1;
            //show_mrec_admob = true;
            //show_native_admob = true;
        }

        internal static void Init()
        {
            if (Instance._state == State.None)
            {
                Instance._state = State.Initializing;
            }
        }

        private void Update()
        {
            switch (_state)
            {
                case State.Initializing:
                    Ads.Instance.Init();
                    enabled = false;
                    _state = State.Initialized;
                    break;
                case State.Initialized:
                    break;
                case State.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Task FetchDataAsync()
        {
            Debug.Log("Fetching data...");
            Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
            return fetchTask.ContinueWithOnMainThread(FetchComplete);
        }
        private void FetchComplete(Task fetchTask)
        {
            configLoaded = true;
            if (!fetchTask.IsCompleted)
            {
                Debug.LogError("Retrieval hasn't finished.");
                return;
            }

            var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
            var info = remoteConfig.Info;
            if (info.LastFetchStatus != LastFetchStatus.Success)
            {
                Debug.LogError($"{nameof(FetchComplete)} was unsuccessful\n{nameof(info.LastFetchStatus)}: {info.LastFetchStatus}");
                return;
            }

            // Fetch successful. Parameter values must be activated to use.
            remoteConfig.ActivateAsync()
                .ContinueWithOnMainThread(
                    task => {
                        Debug.Log($"Remote data loaded and ready for use. Last fetch time {info.FetchTime}.");

                        ads_interval = remoteConfig.GetValue("ads_interval").LongValue;
                        resume_ads = remoteConfig.GetValue("resume_ads").BooleanValue;
                        show_open_ads_resume = remoteConfig.GetValue("show_open_ads_resume").BooleanValue;
                        rating_popup = remoteConfig.GetValue("rating_popup").BooleanValue;
                        show_open_ads = remoteConfig.GetValue("show_open_ads").BooleanValue;
                        show_open_ads_first_open = remoteConfig.GetValue("show_open_ads_first_open").BooleanValue;
                        level_show_inter = remoteConfig.GetValue("level_show_inter").LongValue;
                        hien_qc = remoteConfig.GetValue("hien_qc").BooleanValue;

                        InterstitialCappingTime = ads_interval;
                        FirstInterstitialCappingTime = ads_interval;
                        InterstitialRewardedVideoCappingTime = ads_interval;

                        Debug.Log(ads_interval + " / " + resume_ads + " / " + rating_popup + " / " + show_open_ads + " / " + show_open_ads_first_open);
                    });
        }
    }
}