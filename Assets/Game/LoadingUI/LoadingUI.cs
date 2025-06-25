using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NinthArt
{
	internal class LoadingUI : MonoBehaviour
	{
        //[SerializeField] private Image background;
        public bool loadResume;
        [SerializeField] List<RectTransform> scaleWithScreenSize = new List<RectTransform>(); 
		[SerializeField] private RectTransform progressBar;
		[SerializeField] private RectTransform progressBg;
		[SerializeField] private float fakeLoadTime = 1.0f;

        [SerializeField] float nearlyEndValue = 0.9f;
        [SerializeField] private float loadProcess;
        [SerializeField] TextMeshProUGUI progressTxt;

        [SerializeField] GameObject uiGroup;
        private Tweener tweener;
        private void Awake()
        {
            foreach(RectTransform rectTransform in scaleWithScreenSize)
            {
                GeneralCalculate.UiScaleWithScreenSize(rectTransform);
            }
            if(!loadResume)
            {
                Config.IsLoading = true;
                Config.levelLoaded = false;
            }
            var width = progressBg.sizeDelta.x;
            var size = progressBar.sizeDelta;
            tweener = DOVirtual
                .Float(0, 1, fakeLoadTime, t =>
                {
                    loadProcess = t;
                    size.x = width * t * nearlyEndValue;
                    progressBar.sizeDelta = size;
                    progressTxt.text = "Loading... " + $"{Mathf.RoundToInt(t * nearlyEndValue * 100)}%";
                })
                .OnComplete(() =>
                {
                    size.x = width * nearlyEndValue;
                    progressBar.sizeDelta = size;

                    //AppOpenAdController.CanAction = true;
                    tweener.Kill();
                    StartCoroutine(WaitToEnterGame());
                });
        }
        public void CloseLoadingAfterShowAds()
        {
            uiGroup.SetActive(false);
            SceneManager.CloseScene(SceneID.LoadResumeAds);
        }
        IEnumerator WaitToEnterGame()
        {
            float timeout = GlobalDefine.waitLoadConfigTimeout;

            //wait load config
            float elapsedTime = 0.0f;
            while (Config.InternetConnected && !Config.configLoaded && (elapsedTime < timeout))
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            Config.levelLoaded = true;

            var width = progressBg.sizeDelta.x;
            var size = progressBar.sizeDelta;
            size.x = (width + width * nearlyEndValue)/2;
            progressBar.sizeDelta = size;
            progressTxt.text = "Loading... " + $"{Mathf.RoundToInt((nearlyEndValue + 1) * 0.5f * 100)}%";

            //wait load aoa
            elapsedTime = 0.0f;
            /*while (Config.InternetConnected && !AppOpenAdController.AOACompleted && (elapsedTime < timeout))
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }*/

            yield return new WaitForSeconds(0.5f);

            size.x = width;
            progressBar.sizeDelta = size;
            progressTxt.text = "Loading... 100%";

            yield return new WaitForSeconds(0.2f);
            SceneManager.ShowLoading(() =>
            {
                StartCoroutine(EnterGame());
            });
        }
        IEnumerator EnterGame()
        {
            Config.IsLoading = false;
            SceneManager.OpenScene(SceneID.Home);
            uiGroup.SetActive(false);
            SceneManager.HideLoading();

            yield return new WaitForSeconds(0.5f);
            SceneManager.CloseScene(SceneID.LoadingScene);
        }
        internal void JoinGame()
        {
            tweener.Kill();
            var width = progressBg.sizeDelta.x;
            var size = progressBar.sizeDelta;
            tweener = DOVirtual
                .Float(loadProcess, 1, 1f, t =>
                {
                    loadProcess = t;
                    size.x = width * t * nearlyEndValue;
                    progressBar.sizeDelta = size * nearlyEndValue;
                    progressTxt.text = "Loading... " + $"{Mathf.RoundToInt(t * nearlyEndValue * 100)}%";
                })
                .OnComplete(() =>
                {
                    size.x = width * nearlyEndValue;
                    progressBar.sizeDelta = size * nearlyEndValue;

                    //AppOpenAdController.CanAction = true;
                    tweener.Kill();
                    StartCoroutine(WaitToEnterGame());
                });
        }
    }
}