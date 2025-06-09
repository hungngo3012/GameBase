using NinthArt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiAvoidBannerAds : MonoBehaviour
{
    // Start is called before the first frame update
    RectTransform rectTransform;
    bool avoided = false;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (Config.ShowingBanner)
            Avoid();

        EventManager.Subscribe(NinthArt.EventType.ShowBannerAds, Avoid);
    }
    private void OnDestroy()
    {
        EventManager.Unsubscribe(NinthArt.EventType.ShowBannerAds, Avoid);
    }
    [ContextMenu("TestAvoid")]
    void Avoid(object o = null)
    {
        if (rectTransform == null || avoided || Ads.BannerHeight <= 0)
            return;

        float ratio = GeneralCalculate.GetResolutionRatio();
        if (ratio < 1.0f)
            ratio = 1.0f;

        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y + Ads.BannerHeight * 2.0f * ratio);
        //Debug.Log("avoid: " + gameObject.name + " - " + Ads.BannerHeight * 2.0f);
        avoided = true;
    }
}
