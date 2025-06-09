using NinthArt;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseUiComponent : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI priceTxt;
    [SerializeField] int iapIndex;

    [SerializeField] Button interactBtn;
    [SerializeField] GameObject soldOut;
    
    public List<FontSizeConfig> fontSizes;
    private void Start()
    {
        GetPriceText();
        CheckSoldOut();

        EventManager.Subscribe(NinthArt.EventType.IapInitialized, GetPriceText);
        EventManager.Subscribe(NinthArt.EventType.CheckIAPSoldOut, CheckSoldOut);
    }

    void GetPriceText(object o = null)
    {
        if (priceTxt == null)
            return;

        string price = IAP.GetProductPrice(iapIndex);

        if (!string.IsNullOrEmpty(price))
            priceTxt.text = price;

        foreach(FontSizeConfig fontSizeConfig in fontSizes)
        {
            if(priceTxt.text.Length >= fontSizeConfig.textLength)
            {
                priceTxt.fontSize = fontSizeConfig.fontSize;
                Debug.Log("change font size: " + iapIndex + " - " + priceTxt.fontSize);
                break;
            }    
        }    
    }
    public void OnClickPurchase()
    {
        SoundManager.PlaySfx("BtnClick");
        IAP.Purchase(iapIndex);
    }    
    [ContextMenu("GetText")]
    void GetText()
    {
        priceTxt = GetComponentInChildren<TextMeshProUGUI>();
    }        
    void CheckSoldOut(object o = null)
    {
        if (interactBtn == null || soldOut == null || !IAP.IsNonConsumPurchased(iapIndex))
            return;

        interactBtn.enabled = false;
        soldOut.SetActive(true);
    }
    private void OnDestroy()
    {
        EventManager.Unsubscribe(NinthArt.EventType.IapInitialized, GetPriceText);
        EventManager.Unsubscribe(NinthArt.EventType.CheckIAPSoldOut, CheckSoldOut);
    }
}
[System.Serializable]
public class FontSizeConfig
{
    public int textLength;
    public int fontSize;
}    
