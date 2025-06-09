using System;
using System.Collections;
using System.Collections.Generic;
using AppsFlyerSDK;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine.Networking;

namespace NinthArt
{
    /// <summary>
    /// Demonstrates how to use the Google Mobile Ads app open ad format.
    /// </summary>
    [AddComponentMenu("GoogleMobileAds/Samples/AppOpenAdController")]
    internal class AppOpenAdController : Singleton<AppOpenAdController>
    {

        // These ad units are configured to always serve test ads.
        [SerializeField] private string androidUnitId;
        [SerializeField] private string iosUnitId;

        private string _adUnitId
        {
#if UNITY_ANDROID
            get => androidUnitId;
#elif UNITY_IOS
            get => iosUnitId;
#else
            get => "unused";
#endif
        }

        private AppOpenAd appOpenAd;
        private bool isAoaOpening;
        private bool loadAdComplete;
        private bool showedAOA;
        private LoadingUI loadingUi;
        public bool canAction;

        public static LoadingUI LoadingUi
        {
            get => Instance.loadingUi; 
            set => Instance.loadingUi = value;
        }    

        public static bool CanAction
        {
            get => Instance.canAction;
            set => Instance.canAction = value;
        }
        public static bool AOACompleted;

        private void Awake()
        {
            // Use the AppStateEventNotifier to listen to application open/close events.
            // This is used to launch the loaded ad when we open the app.
            AppStateEventNotifier.AppStateChanged += OnAppStateChanged;

            StartCoroutine(CheckInternetOverTime());
        }

        private void Start()
        {
            if (Config.Instance.show_open_ads && !showedAOA)
                StartCoroutine(CheckInternetConnection(OnInternetConnectionCheckedBeforeShowAOA));
        }

        /// <summary>
        /// Loads the app open ad.
        /// </summary>
        private void LoadAppOpenAd()
        {
            // Clean up the old ad before loading a new one.
            if (appOpenAd != null)
            {
                appOpenAd.Destroy();
                appOpenAd = null;
            }

            //Debug.Log("Loading the app open ad.");

            // Create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            AppOpenAd.Load(_adUnitId, adRequest,
                (AppOpenAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("app open ad failed to load an ad " + "with error : " + error);
                        //GameManager.ShowNoti("app open ad failed to load an ad " + "with error : " + error);
                        return;
                    }
                    
                    Debug.Log("App open ad loaded with response : " + ad.GetResponseInfo());

                    appOpenAd = ad;
                    RegisterEventHandlers(ad);
                    loadAdComplete = true;
                });
        }
        public void LoadAOA()
        {
            StartCoroutine(CheckInternetConnection(isConnected =>
            {
                if (isConnected)
                {
                    LoadAppOpenAd();
                }
            }));
        }
        public void ShowAOA(LoadingUI loading = null)
        {
            StartCoroutine(ShowAppOpenAdWithDelay(loading));
        }    
        private void RegisterEventHandlers(AppOpenAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("App open ad paid {0} {1}.", adValue.Value, adValue.CurrencyCode));
                
                HandleAdPaidEvent(null, adValue);
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("App open ad recorded an impression."); 
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () => { Debug.Log("App open ad was clicked."); };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("App open ad full screen content opened.");
                isAoaOpening = true;
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("App open ad full screen content closed.");
                isAoaOpening = false;
                if(loadingUi != null)
                    loadingUi.JoinGame();

                if(showingAOAResume)
                {
                    showingAOAResume = false;
                    Invoke("ResetAOAResumeCapping", Config.InterstitialCappingTime);
                }
                else
                    Invoke("ResetShowResumeAfterShowAOA", Config.InterstitialCappingTime);
                // Reload the ad so that we can show another as soon as possible.
                //LoadAppOpenAd();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("App open ad failed to open full screen content " + "with error : " + error);
                
                // Reload the ad so that we can show another as soon as possible.
                LoadAppOpenAd();
            };
        }
        void ResetAOAResumeCapping()
        {
            Ads.CappingAOAResume = false;
        }    
        /// <summary>
        /// Shows the app open ad.
        /// </summary>
        private void ShowAppOpenAd()
        {
            //GameManager.ShowNoti(Profile.FirstAoa + " - " + Config.Instance.show_open_ads_first_open + " - " + Config.Instance.show_open_ads);
            if(Profile.Vip)
            {
                AOACompleted = true;
                return;
            }    
            if (Profile.FirstAoa)
            {
                Profile.FirstAoa = false;
                if (!Config.Instance.show_open_ads_first_open)
                {
                    AOACompleted = true;
                    return;
                }
            }
            if (!Config.Instance.show_open_ads)
            {
                AOACompleted = true;
                return;
            }
            if (appOpenAd != null && appOpenAd.CanShowAd())
            {
                if (!Config.IsLoading)
                {
                    //GameManager.ShowNoti("Not in loading");
                    AOACompleted = true;
                    return;
                }
                //Debug.Log("Showing app open ad.");
                //GameManager.ShowNoti("Showing app open ad.");
                appOpenAd.Show();
                showedAOA = true;
                Config.canShowResumeAfterShowAOA = false;
                AOACompleted = true;
            }
            else
            {
                Debug.LogError("App open ad is not ready yet.");
                AOACompleted = true;
                //GameManager.ShowNoti("App open ad is not ready yet.");
            }
        }
        bool showingAOAResume;
        private void ShowAppOpenAdResume()
        {
            if (appOpenAd != null && appOpenAd.CanShowAd())
            {
                showingAOAResume = true;
                Ads.CappingAOAResume = true;
                appOpenAd.Show();
            }
            else
            {
                Config.Instance.isAoa = false;
                LoadAppOpenAd();
                Debug.LogError("App open ad is not ready yet.");
            }
        }

        public IEnumerator ShowAppOpenAdWithDelay(LoadingUI loading = null)
        {
            //GameManager.ShowNoti("Call Show AOA - " + loadAdComplete + " - " + Config.configLoaded);
            if(loading != null)
                loadingUi = loading;

            while (!loadAdComplete)
                yield return null;
            while (!Config.configLoaded)
                yield return null;
            /*
            while (!Config.IsLoading)
                yield return null;*/

            loadAdComplete = false;
            ShowAppOpenAd();
        }
        
        IEnumerator ShowAppResumeAdWithDelay()
        {
            if (Profile.Vip)
            {
                yield break;
            }
            if (Config.IsLoading || !Config.canShowResumeAfterShowReward || !Config.canShowResumeAfterShowAOA)
            {
                yield break;
            }

            Config.Instance.isAoa = true;
            yield return new WaitForSeconds(0.2f);
            //Debug.Log("Show resume ads");
            //GameManager.ShowNoti("Show resume ads");
            AppsFlyer.sendEvent($"af_inters_ad_eligible", null);

            if (Config.Instance.show_open_ads_resume)
            {
                ShowAppOpenAdResume();
            }    
            else
            {
                Ads.ShowInterstitial($"level_{Profile.Level}_AOA", null, true);
            }
        }
        void ResetShowResumeAfterShowAOA()
        {
            Config.canShowResumeAfterShowAOA = true;
        }    
        private void OnAppStateChanged(AppState state)
        {
            Debug.Log("App State changed to : " + state);

            // if the app is Foregrounded and the ad is available, show it.
            if (state == AppState.Foreground)
            {
                StartCoroutine(AppStateAction());
            }
        }

        private IEnumerator AppStateAction()
        {
            yield return new WaitForSeconds(0.5f);
            
            if (Profile.Vip) yield break;
            
            if (!Profile.IsPlayed)
            {
                yield break;
            }
            
            if (Config.Instance.bannerClicked)
            {
                Config.Instance.bannerClicked = false;
                yield break;
            }
            
            if (isAoaOpening)
            {
                yield break;
            }
            
            if (Config.Instance.resume_ads && Profile.Level > 1)
            {
                StartCoroutine(ShowAppResumeAdWithDelay());
            }
        }

        #region CheckInternetAvaiable

        public void StartCheckInternet()
        {
            //StartCoroutine(CheckInternetOverTime());
        }
        bool _isCheckingInternet = false;
        private IEnumerator CheckInternetOverTime()
        {
            while (true)
            {
                if (CanAction && !_isCheckingInternet)
                {
                    //Debug.Log("call check internet");
                    StartCoroutine(CheckInternetConnection(OnInternetConnectionChecked));
                }
                yield return new WaitForSeconds(10.0f);
            }
        }
        
        private IEnumerator CheckInternetConnection(Action<bool> action)
        {
            _isCheckingInternet = true;
            using (UnityWebRequest request = UnityWebRequest.Get("http://www.google.com"))
            {
                request.timeout = 10; // Set a timeout to prevent hanging indefinitely

                yield return request.SendWebRequest();

                //Debug.Log("checking internet result: " + request.result.ToString());
                switch (request.result)
                {
                    // Check for different types of errors
                    case UnityWebRequest.Result.ConnectionError:
                        Debug.LogError("Connection error: " + request.error);
                        action?.Invoke(false);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError("Protocol error: " + request.error);
                        action?.Invoke(false);
                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError("Data processing error: " + request.error);
                        action?.Invoke(false);
                        break;
                    case UnityWebRequest.Result.InProgress:
                        break;
                    case UnityWebRequest.Result.Success:
                        action?.Invoke(true);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            _isCheckingInternet = false;
        }

        private void OnInternetConnectionChecked(bool isConnected)
        {
            if (isConnected)
            {
                //Debug.Log("Internet is available. is load AOA: " + (Config.Instance.show_open_ads && !showedAOA && !(Profile.FirstAoa && !Config.Instance.show_open_ads_first_open)) + 
                    //" - internet connected: " + (Config.InternetConnected) + " - can show banner: " + (Ads.Instance.CanShowBanner) + " - banner loaded: " + AdmobBannerController.Instance.bannerLoaded);
                
                //if(Profile.Vip) return;

                if (Config.InternetConnected)
                {
                    if (!Ads.Instance.CanShowBanner)
                    {
                        //Debug.Log("Load banner when internet connected");
                        Ads.LoadBanner();
                    }

                    if (!AdmobBannerController.Instance.bannerLoaded)
                    {
                        //Debug.Log("Load admob when internet connected");
                        AdmobBannerController.Instance.LoadAd(AdmobBannerController.BANNER_TYPE.NORMAL);
                        AdmobBannerController.Instance.HideBanner();
                    }
                    return;
                }

                //GameManager.ShowNoti("Internet Connected");
                Config.InternetConnected = true;

                //Debug.Log("load ads when internet not connected before");
                Ads.LoadBanner();
                //if(Config.Instance.show_collab_ad)
                AdmobBannerController.Instance.LoadAd(AdmobBannerController.BANNER_TYPE.NORMAL);
                AdmobBannerController.Instance.HideBanner();
                //Debug.Log("load ads done");
            }
            else
            {
                Debug.Log("Internet is not available.");
                Config.InternetConnected = false;
            }
        }
        private void OnInternetConnectionCheckedBeforeShowAOA(bool isConnected)
        {
            if (isConnected)
            {
                Debug.Log("Internet is available.");

                Config.Instance.internetConnected = true;

                if (Profile.Vip) return;

                StartCoroutine(ShowAppOpenAdWithDelay(loadingUi));

            }
            else
            {
                Debug.Log("Internet is not available.");
                Config.Instance.internetConnected = false;
            }
        }
        #endregion

        public void HandleAdPaidEvent(object sender, AdValue adValue)
        {
            // TODO: Send the impression-level ad revenue information to your
            // preferred analytics server directly within this callback.
            Debug.Log("call impression data AOA");

            double valueMicros = adValue.Value  / 1000000f;
            string currencyCode = adValue.CurrencyCode;
            AdValue.PrecisionType precision = adValue.Precision;

            ResponseInfo responseInfo = appOpenAd.GetResponseInfo();
            string responseId = responseInfo.GetResponseId();

            AdapterResponseInfo loadedAdapterResponseInfo = responseInfo.GetLoadedAdapterResponseInfo();
            string adSourceId = loadedAdapterResponseInfo.AdSourceId;
            string adSourceInstanceId = loadedAdapterResponseInfo.AdSourceInstanceId;
            string adSourceInstanceName = loadedAdapterResponseInfo.AdSourceInstanceName;
            string adSourceName = loadedAdapterResponseInfo.AdSourceName;
            string adapterClassName = loadedAdapterResponseInfo.AdapterClassName;
            long latencyMillis = loadedAdapterResponseInfo.LatencyMillis;
            Dictionary<string, string> credentials = loadedAdapterResponseInfo.AdUnitMapping;
            
            
            Firebase.Analytics.Parameter[] AdParameters = {
                new Firebase.Analytics.Parameter("ad_platform", "Admob"),
                new Firebase.Analytics.Parameter("ad_source", adSourceName),
                new Firebase.Analytics.Parameter("ad_format", "Open Ads"),
                new Firebase.Analytics.Parameter("ad_unit_name", adSourceId),
                new Firebase.Analytics.Parameter("currency",currencyCode),
                new Firebase.Analytics.Parameter("value", (double) valueMicros)
            };
            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", AdParameters);
            
            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            additionalParams.Add(AFAdRevenueEvent.AD_TYPE, "AppOpenAd");
				
            AppsFlyerAdRevenue.logAdRevenue("Admob", AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob, 
                valueMicros, adValue.CurrencyCode, additionalParams);
        }

        private void OnDestroy()
        {
            // Always unlisten to events when complete.
            AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;
        }
    }
}