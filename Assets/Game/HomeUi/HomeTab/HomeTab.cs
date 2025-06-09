using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinthArt;
using TMPro;
using UnityEngine.UI;

internal class HomeTab : Scene
{
    [SerializeField] internal Canvas canvas;
    [SerializeField] HomeTabBtn homeBtn;
    [SerializeField] HomeTabBtn mapBtn;
    [SerializeField] HomeTabBtn shopBtn;

    private void Start()
    {
        SetState(0);
        homeBtn.btn.onClick.AddListener(() => OnClickHome());
        mapBtn.btn.onClick.AddListener(() => OnClickMap());
        shopBtn.btn.onClick.AddListener(() => OnClickShop());

        canvas.worldCamera = Camera.main;
        EventManager.Subscribe(NinthArt.EventType.GoToShop, OnClickShop);
    }
    private void OnDestroy()
    {
        EventManager.Unsubscribe(NinthArt.EventType.GoToShop, OnClickShop);
    }
    void OnClickHome()
    {
        //SoundManager.PlaySfx("BtnClick");
        SetState(0);
        SceneManager.OpenScene(SceneID.Home);
    }
    void OnClickMap()
    {
        //SoundManager.PlaySfx("BtnClick");
        SetState(1);
        SceneManager.OpenScene(SceneID.StageUi);
    }
    void OnClickShop(object o = null)
    {
        //SoundManager.PlaySfx("BtnClick");
        SetState(2);
        SceneManager.OpenScene(SceneID.ShopUi);
    }
    void ExitOldState()
    {
        if (curState == 0)
        {

        }
        else if (curState == 1)
        {
            SceneManager.CloseScene(SceneID.StageUi);
        }
        else if (curState == 2)
        {
            SceneManager.CloseScene(SceneID.ShopUi);
        }
    }
    int curState = 0;
    void SetState(int state)
    {
        ExitOldState();
        if (state == 0)
        {
            homeBtn.SetState(true);
            mapBtn.SetState(false);
            shopBtn.SetState(false);
        }
        else if(state == 1)
        {
            homeBtn.SetState(false);
            mapBtn.SetState(true);
            shopBtn.SetState(false);
        }
        else if (state == 2)
        {
            homeBtn.SetState(false);
            mapBtn.SetState(false);
            shopBtn.SetState(true);
        }
        curState = state;
    }
}
