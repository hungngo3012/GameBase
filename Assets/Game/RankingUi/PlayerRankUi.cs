using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRankUi : MonoBehaviour
{
    [SerializeField] Sprite userBgSprite;
    [SerializeField] List<Sprite> topSprites = new List<Sprite>();
    [SerializeField] Sprite levelSprite;
    [SerializeField] Sprite coinSprite;
    [SerializeField] Sprite starSprite;

    [SerializeField] Image bg;
    [SerializeField] Image rankImg;
    [SerializeField] Image pointImg;
    [SerializeField] Image avtImage;

    [SerializeField] TextMeshProUGUI rankTxt;
    [SerializeField] TextMeshProUGUI nameTxt;
    [SerializeField] TextMeshProUGUI pointTxt;

    internal void Init(bool isUser, int rank, int point, Sprite avatar, string name, SortPlayerType type)
    {
        if (isUser)
            bg.sprite = userBgSprite;
        if(rank > 0)
        {
            if(rank <= topSprites.Count)
            {
                rankTxt.gameObject.SetActive(false);
                rankImg.sprite = topSprites[rank - 1];
                rankImg.enabled = true;
                rankImg.SetNativeSize();
            }
            rankTxt.text = rank.ToString();
        }
        pointTxt.text = point.ToString();
        nameTxt.text = name;
        avtImage.sprite = avatar;

        switch(type)
        {
            case SortPlayerType.POINT:
                pointImg.sprite = levelSprite;
                break;
            case SortPlayerType.COIN:
                pointImg.sprite = coinSprite;
                break;
            case SortPlayerType.STAR:
                pointImg.sprite = starSprite;
                break;
        }
    }
}
