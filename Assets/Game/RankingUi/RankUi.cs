using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinthArt;
using TMPro;
using UnityEngine.UI;

internal class RankUi : Scene
{
    [SerializeField] PlayerRankUi playerRankUiPref;
    [SerializeField] Transform content;
    [SerializeField] Transform userContent;

    [SerializeField] Button closeBtn;
    [SerializeField] Button levelBtn;
    [SerializeField] Button coinBtn;
    [SerializeField] Button starBtn;

    [SerializeField] ChangeRankTypeBtn changeRankTypeBtn;

    public List<GameObject> marks = new List<GameObject>();

    [SerializeField] ScrollRect scrollRect;
    [SerializeField] GridLayoutGroup contentGrid;
    [SerializeField] SkinConfig skinConfig;
    List<PlayerRankUi> created = new List<PlayerRankUi>();
    public int numTopShow = 20;
    private void Start()
    {
        closeBtn.onClick.AddListener(() => { Close(); SoundManager.PlaySfx("BtnClick"); });
        levelBtn.onClick.AddListener(() => { OnDisplayRank(SortPlayerType.POINT); SoundManager.PlaySfx("BtnClick"); });
        coinBtn.onClick.AddListener(() => { OnDisplayRank(SortPlayerType.COIN); SoundManager.PlaySfx("BtnClick"); });
        starBtn.onClick.AddListener(() => { OnDisplayRank(SortPlayerType.STAR); SoundManager.PlaySfx("BtnClick"); });

        changeRankTypeBtn.btn.onClick.AddListener(() => { OnClickChangeRankType(); SoundManager.PlaySfx("BtnClick"); });
        OnDisplayRank(SortPlayerType.POINT);
    }
    void Close()
    {
        //Gameplay.Instance.ChangePlayingState(Gameplay.PlayingState.Normal);
        SceneManager.CloseScene(SceneID.RankingUi);
    }
    void OnDisplayRank(SortPlayerType type, bool getTopRank = true)
    {
        UpdateMark(type);
        ShowTop(type, getTopRank);
        topRank = getTopRank;
        curSortType = type;
        changeRankTypeBtn.UpdateState(!getTopRank);
    }
    bool topRank = true;
    SortPlayerType curSortType = SortPlayerType.POINT;
    void OnClickChangeRankType()
    {
        OnDisplayRank(curSortType, !topRank);
    }
    void ShowTop(SortPlayerType type, bool getTopRank)
    {
        ClearAllRankUi();
        if (getTopRank)
        {
            var get = LeaderboardService.GetTopPlayers(LeaderboardService.GetAllPlayerList(), type, numTopShow);
            List<PlayerInfo> playerInfos = new List<PlayerInfo>(get.Item1);
            int userRank = get.Item2;
            GenPlayerRankUi(playerInfos, type, userRank);
        }
        else
        {
            var get = LeaderboardService.GetSurroundingPlayers(LeaderboardService.GetAllPlayerList(), type, numTopShow);
            GenPlayerRankUi(get, type);
        }
    }
    void GenPlayerRankUi(Dictionary<int, PlayerInfo> players, SortPlayerType type)
    {
        foreach (var playerInfo in players)
        {
            PlayerRankUi newPlayer = Instantiate(playerRankUiPref, content);
            created.Add(newPlayer);
            switch (type)
            {
                case SortPlayerType.POINT:
                    newPlayer.Init(playerInfo.Value.isCurPlayer, playerInfo.Key, playerInfo.Value.point, skinConfig.rankingAvas[playerInfo.Value.avt].avatar, playerInfo.Value.userName, type);
                    break;
                case SortPlayerType.COIN:
                    newPlayer.Init(playerInfo.Value.isCurPlayer, playerInfo.Key, playerInfo.Value.coinReward, skinConfig.rankingAvas[playerInfo.Value.avt].avatar, playerInfo.Value.userName, type);
                    break;
                case SortPlayerType.STAR:
                    newPlayer.Init(playerInfo.Value.isCurPlayer, playerInfo.Key, playerInfo.Value.starReward, skinConfig.rankingAvas[playerInfo.Value.avt].avatar, playerInfo.Value.userName, type);
                    break;
            }
        }

        GeneralCalculate.ScrollToIndex(players.Count / 4 + 1, scrollRect, contentGrid);
    }
    void GenPlayerRankUi(List<PlayerInfo> players, SortPlayerType type, int userRank)
    {
        bool hadPlayerOnTop = false;
        int i = 0;
        foreach (PlayerInfo playerInfo in players)
        {
            i++;
            PlayerRankUi newPlayer = Instantiate(playerRankUiPref, content);
            created.Add(newPlayer);
            switch (type)
            {
                case SortPlayerType.POINT:
                    newPlayer.Init(playerInfo.isCurPlayer, i, playerInfo.point, skinConfig.rankingAvas[playerInfo.avt].avatar, playerInfo.userName, type);
                    break;
                case SortPlayerType.COIN:
                    newPlayer.Init(playerInfo.isCurPlayer, i, playerInfo.coinReward, skinConfig.rankingAvas[playerInfo.avt].avatar, playerInfo.userName, type);
                    break;
                case SortPlayerType.STAR:
                    newPlayer.Init(playerInfo.isCurPlayer, i, playerInfo.starReward, skinConfig.rankingAvas[playerInfo.avt].avatar, playerInfo.userName, type);
                    break;
            }

            if (playerInfo.isCurPlayer)
                hadPlayerOnTop = true;
        }

        if (!hadPlayerOnTop)
        {
            PlayerRankUi userRankUi = Instantiate(playerRankUiPref, userContent);
            created.Add(userRankUi);
            switch (type)
            {
                case SortPlayerType.POINT:
                    userRankUi.Init(true, userRank, Profile.Level, skinConfig.rankingAvas[Profile.CurAvt].avatar, Profile.UserName + "_" + Profile.UserTag, type);
                    break;
                case SortPlayerType.COIN:
                    userRankUi.Init(true, userRank, Profile.CoinCollected, skinConfig.rankingAvas[Profile.CurAvt].avatar, Profile.UserName + "_" + Profile.UserTag, type);
                    break;
                case SortPlayerType.STAR:
                    userRankUi.Init(true, userRank, Profile.StarCollected, skinConfig.rankingAvas[Profile.CurAvt].avatar, Profile.UserName + "_" + Profile.UserTag, type);
                    break;
            }
        }

        GeneralCalculate.ScrollToIndex(0, scrollRect, contentGrid);
    }
    void ClearAllRankUi()
    {
        foreach (PlayerRankUi playerRankUi in created)
        {
            Destroy(playerRankUi.gameObject);
        }
        created.Clear();
    }
    void UpdateMark(SortPlayerType type)
    {
        int i = 0;
        foreach (GameObject mark in marks)
        {
            if ((int)type == i)
                mark.SetActive(true);
            else
                mark.SetActive(false);
            i++;
        }
    }
}
