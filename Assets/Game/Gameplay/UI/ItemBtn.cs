using NinthArt;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemBtn : MonoBehaviour
{
    [SerializeField] string itemId;
    [SerializeField] Item item;
    [SerializeField] GameTool gameTool;
    [SerializeField] GameObject borderCount;
    [SerializeField] TextMeshProUGUI countTxt;

    [SerializeField] GameObject lockObj;
    int countItem;
    bool isLocked;
    private void Start()
    {
        UpdateBtnState();
        UpdateCountUi();
        EventManager.Subscribe(NinthArt.EventType.NumItemChange, UpdateCountUi);
    }
    private void OnDestroy()
    {
        EventManager.Unsubscribe(NinthArt.EventType.NumItemChange, UpdateCountUi);
    }
    void UpdateCountUi(object o = null)
    {
        if (isLocked)
        {
            borderCount.SetActive(false);
            return;
        }

        countItem = ItemManager.CountNumItem(itemId);
        if (countItem > 0)
        {
            borderCount.SetActive(true);
            countTxt.text = countItem.ToString();
        }
        else
        {
            borderCount.SetActive(false);
        }    
    }   
    void UpdateBtnState()
    {
        if(Profile.UnlockToolProgress > (int)gameTool)
        {
            lockObj.SetActive(false);
            isLocked = false;

            return;
        }
        int unlockLevel = Gameplay.Instance.levelConfig.unlockToolLevels[(int)gameTool];

        if(Profile.Level < unlockLevel)
        {
            //lock
            lockObj.SetActive(true);
            isLocked = true;
        }
        else
        {
            if (Profile.Level == unlockLevel)
                Gameplay.OpenPopupByName("Unlock" + ((item == null) ? itemId : item.itemId));
            //unlock
            lockObj.SetActive(false);
            isLocked = false;
        }
    }
    public void OnClickUseItem()
    {
        if (Gameplay.Instance == null || !Gameplay.Instance.IsPlayState() || Gameplay.Instance._playingState != Gameplay.PlayingState.Normal || isLocked)
            return;
        if (Gameplay.Instance.Level.inTransit > 0 || Gameplay.Instance.Level.carMoving > 0 || Gameplay.Instance.Level.passToCarCall > 0)
            return;
        if ((item is Shuffle || itemId == "Shuffle") && Gameplay.Instance != null && !Gameplay.Instance.Level.CanUseShuffle())
            return;

        bool useItemResult = false;
        bool useItemVar = (item != null);

        if (useItemVar)
            useItemResult = ItemManager.UseItem(item);
        else
            useItemResult = ItemManager.UseItem(itemId);

        if(useItemResult)
        {
            //Thành công
            Debug.Log("use item success");
            //UpdateCountUi();
        }
        else
        {
            //Thất bại
            Debug.Log("use item fail");
            //Hiển thị popup mua hoặc xem quảng cáo, có thể sử dụng event
            Gameplay.Instance.ChangePlayingState(Gameplay.PlayingState.Pause);
            if (useItemVar)
                Gameplay.OpenPopupByName(item.itemId);
            else
                Gameplay.OpenPopupByName(itemId);
        }    
    }
}
