using System;
using System.Collections;
using System.Collections.Generic;
using AppsFlyerSDK;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using UnityEngine;
using static NinthArt.AdmobBannerController;

namespace NinthArt
{
    internal class AdmobBannerController : Singleton<AdmobBannerController>
    {
        // These ad units are configured to always serve test ads.
        [SerializeField] private string androidUnitId;
        [SerializeField] private string iosUnitId;
        [SerializeField] private BANNER_POSITION _bannerPosition;
        [SerializeField] private bool IsDebug = false;

        private Coroutine _countDownReloadAd = null;
        float _lastCollapBannerShowTime = 0;

        public enum BANNER_POSITION : int
        {
            TOP = 0,
            BOTTOM = 1
        }

        public enum BANNER_TYPE : int
        {
            NORMAL = 0,
            COLLAPSIBLE = 1
        }

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
        BannerView _bannerView;
        public bool bannerLoaded;

        /// <summary>
        /// Creates a 320x50 banner view at top of the screen.
        /// </summary>
        public void CreateBannerView()
        {
            //Debug.Log("Creating banner view");

            // If we already have a banner, destroy the old one.
            if (_bannerView != null)
            {
                DestroyBannerView();
            }

            AdSize bannerSize = AdSize.MediumRectangle;
            if (IsDebug)
            {
                _bannerView = new BannerView("ca-app-pub-3940256099942544/2014213617", bannerSize, AdPosition.Bottom);
            }
            else
            {
                _bannerView = new BannerView(_adUnitId, bannerSize, AdPosition.Bottom);
            }    
        }
        
        /// <summary>
        /// Creates the banner view and loads a banner ad.
        /// </summary>
        public void LoadAd(BANNER_TYPE type = BANNER_TYPE.NORMAL)
        {
            // create an instance of a banner view first.
            if(_bannerView == null)
            {
                CreateBannerView();
            }
            
            // create our request used to load the ad.
            var adRequest = new AdRequest();
            if (type == BANNER_TYPE.COLLAPSIBLE)
            {
                string bannerPos = _bannerPosition == BANNER_POSITION.TOP ? "top" : "bottom";
                adRequest.Extras.Add("collapsible", bannerPos);
            }
            else if (type == BANNER_TYPE.NORMAL)
            {
                adRequest.Extras.Add("collapsible_request_id", Guid.NewGuid().ToString());
            }
            ListenToAdEvents();

            /*
            if (_countDownReloadAd != null)
                StopCoroutine(_countDownReloadAd);

            _countDownReloadAd = StartCoroutine(CountDownReloadAd(Config.Instance.time_reload_collap_ad));*/

            _bannerView.LoadAd(adRequest);
        }

        private IEnumerator CountDownReloadAd(float time)
        {
            yield return new WaitForSeconds(time);
            LoadAd(BANNER_TYPE.COLLAPSIBLE);
        }
        /// <summary>
        /// listen to events the banner view may raise.
        /// </summary>
        private void ListenToAdEvents()
        {
            // Raised when an ad is loaded into the banner view.
            _bannerView.OnBannerAdLoaded += () =>
            {
                Debug.Log("Banner view loaded an ad with response : " + _bannerView.GetResponseInfo());
                bannerLoaded = true;
            };
            // Raised when an ad fails to load into the banner view.
            _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                Debug.LogError("Banner view failed to load an ad with error : " + error);
                bannerLoaded = false;
            };
            // Raised when the ad is estimated to have earned money.
            _bannerView.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Banner view paid {0} {1}.", adValue.Value, adValue.CurrencyCode));
                
                HandleAdPaidEvent(null, adValue);
            };
            // Raised when an impression is recorded for an ad.
            _bannerView.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Banner view recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            _bannerView.OnAdClicked += () =>
            {
                Debug.Log("Banner view was clicked.");
                Config.Instance.bannerClicked = true;
            };
            // Raised when an ad opened full screen content.
            _bannerView.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Banner view full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            _bannerView.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Banner view full screen content closed.");
            };
        }

        public static void ShowBanner()
        {
            /*
            if(!Config.Instance.internetConnected) return;
            if(Profile.Vip) return;
            if(!Instance.bannerLoaded)
            {
                Debug.Log("Banner not loaded yet");
                return;
            }
            Debug.Log("Show banner view.");
            Instance._bannerView.Show();*/

            //if(!Config.Instance.show_collab_ad || (Time.realtimeSinceStartup - Instance._lastCollapBannerShowTime < Config.Instance.collaps_interval))
            if (Profile.Vip || Config.ShowingBanner) return;

            Ads.ShowBanner();
            //Debug.Log("show irs banner");
            return;
            
            /*
            if (Instance._bannerView != null)
            {
                Instance._bannerView.Show();
                Instance._lastCollapBannerShowTime = Time.realtimeSinceStartup;
                Debug.Log("show collap banner");
            }*/
        }

        private void SetBannerPosition()
        {
            GameObject bannerObject = GameObject.FindGameObjectWithTag("Admob_Banner");
            if (bannerObject == null)
            {
                Debug.LogError("Admob_Banner object not found. Please ensure it is tagged correctly.");
                return;
            }

            RectTransform rect = bannerObject.GetComponent<RectTransform>();
            Canvas canvas = bannerObject.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas not found. Ensure the Admob_Banner is within a Canvas.");
                return;
            }

            Debug.Log($"Canvas size: {rect.sizeDelta}");
            Debug.Log($"Banner size: ({_bannerView.GetWidthInPixels()},{_bannerView.GetHeightInPixels()})");

            // Calculate the position in pixels
            float canvasScaleFactor = canvas.scaleFactor;
            var xBanner = Mathf.RoundToInt((rect.rect.width * canvasScaleFactor - _bannerView.GetWidthInPixels()) / 2);
            var yBanner = Mathf.RoundToInt((rect.rect.height * canvasScaleFactor - _bannerView.GetHeightInPixels()) / 2 - 100 * canvasScaleFactor);

            Debug.Log($"Calculated Position: ({xBanner},{yBanner})");

            _bannerView.SetPosition(xBanner, yBanner);
        }

        public void HideBanner()
        {
            if(!Config.InternetConnected) return;
            //if(Profile.Vip) return;
            if (_bannerView == null)
            {
                StartCoroutine(WaitBannerCreate());
                return;
            }
            _bannerView.Hide();
        }

        public IEnumerator WaitBannerCreate()
        {
            yield return new WaitUntil(() => _bannerView != null);
            _bannerView.Hide();
        }
        
        /// <summary>
        /// Destroys the banner view.
        /// </summary>
        public void DestroyBannerView()
        {
            if (_bannerView != null)
            {
                _bannerView.Destroy();
                _bannerView = null;
            }
        }
        
        public void HandleAdPaidEvent(object sender, AdValue adValue)
        {
            // TODO: Send the impression-level ad revenue information to your
            // preferred analytics server directly within this callback.
            
            double valueMicros = adValue.Value  / 1000000f;
            string currencyCode = adValue.CurrencyCode;
            AdValue.PrecisionType precision = adValue.Precision;

            ResponseInfo responseInfo = _bannerView.GetResponseInfo();
            string responseId = responseInfo.GetResponseId();

            AdapterResponseInfo loadedAdapterResponseInfo = responseInfo.GetLoadedAdapterResponseInfo();
            string adSourceId = loadedAdapterResponseInfo.AdSourceId;
            string adSourceInstanceId = loadedAdapterResponseInfo.AdSourceInstanceId;
            string adSourceInstanceName = loadedAdapterResponseInfo.AdSourceInstanceName;
            string adSourceName = loadedAdapterResponseInfo.AdSourceName;
            string adapterClassName = loadedAdapterResponseInfo.AdapterClassName;
            long latencyMillis = loadedAdapterResponseInfo.LatencyMillis;
            Dictionary<string, string> credentials = loadedAdapterResponseInfo.AdUnitMapping;

            /*Dictionary<string, string> extras = responseInfo.GetResponseExtras();
            string mediationGroupName = extras["mediation_group_name"];
            string mediationABTestName = extras["mediation_ab_test_name"];
            string mediationABTestVariant = extras["mediation_ab_test_variant"];*/
            
            Firebase.Analytics.Parameter[] AdParameters = {
                new Firebase.Analytics.Parameter("ad_platform", "Admob"),
                new Firebase.Analytics.Parameter("ad_source", adSourceName),
                new Firebase.Analytics.Parameter("ad_format", "MREC"),
                new Firebase.Analytics.Parameter("ad_unit_name", adSourceId),
                new Firebase.Analytics.Parameter("currency",currencyCode),
                new Firebase.Analytics.Parameter("value", (double) valueMicros)
            };
            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", AdParameters);
            
            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            additionalParams.Add(AFAdRevenueEvent.AD_TYPE, "Banner");
				
            AppsFlyerAdRevenue.logAdRevenue("Admob", AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob, 
                valueMicros, adValue.CurrencyCode, additionalParams);
        }
    }
}
