using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupRewardUi : MonoBehaviour
{
    public Button closeBtn;
    public Image icon;
    public TextMeshProUGUI numTxt;
    public float animDuration = 0.5f;
    public Transform popObj;
    private void OnEnable()
    {
        AnimateIn();
    }
    public void AnimateIn()
    {
        popObj.localScale = Vector3.zero;
        popObj.DOScale(1, animDuration).SetEase(Ease.OutBack);
    }

    public void AnimateOut()
    {
        popObj.localScale = Vector3.one;
        popObj.DOScale(0, animDuration).SetEase(Ease.InBack);
    }
    public void CloseWithAnimation()
    {
        StartCoroutine(AnimatedClose());
    }

    IEnumerator AnimatedClose()
    {
        popObj.localScale = Vector3.one;
        yield return popObj.DOScale(0, animDuration).SetEase(Ease.InBack).WaitForCompletion();
        gameObject.SetActive(false);
    }
}
