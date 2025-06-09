using NinthArt;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardService
{
    public static List<PlayerInfo> GetAllPlayerList()
    {
        string data = GeneralCalculate.LoadJsonFromResource(GlobalDefine.rankingDataPath);
        List<PlayerInfo> playerInfos = JsonConvert.DeserializeObject<List<PlayerInfo>>(data);
        if (playerInfos == null)
        {
            playerInfos = new List<PlayerInfo>();
        }

        return playerInfos;
    }
    public static Dictionary<int, PlayerInfo> GetSurroundingPlayers(List<PlayerInfo> players, SortPlayerType sortType, int num)
    {
        Dictionary<int, PlayerInfo> result = new Dictionary<int, PlayerInfo>();
        PlayerInfo user = new PlayerInfo();
        user.userName = Profile.UserName + "_" + Profile.UserTag;//TODO: thay bằng username khi có sửa info người chơi
        user.point = Profile.Level;
        user.coinReward = Profile.CoinCollected;
        user.starReward = Profile.StarCollected;
        user.avt = Profile.CurAvt;
        user.isCurPlayer = true;

        players.Add(user);

        switch (sortType)
        {
            case SortPlayerType.POINT:
                players.Sort((a, b) => b.point.CompareTo(a.point));
                break;
            case SortPlayerType.COIN:
                players.Sort((a, b) => b.coinReward.CompareTo(a.coinReward));
                break;
            case SortPlayerType.STAR:
                players.Sort((a, b) => b.starReward.CompareTo(a.starReward));
                break;
        }

        int currentIndex = players.FindIndex(player => player.isCurPlayer == true);

        int startIndex = Mathf.Max(0, currentIndex - num / 2); // Lấy tối đa 10 người chơi phía trên
        int endIndex = Mathf.Min(players.Count, currentIndex + num / 2); // Lấy tối đa 10 người chơi phía dưới

        List<PlayerInfo> selectedPlayers = players.GetRange(startIndex, endIndex - startIndex);

        int rank = 0;
        foreach (PlayerInfo playerInfo in selectedPlayers)
        {
            if (rank == 0)
                rank = players.IndexOf(playerInfo) + 1;

            result.Add(rank, playerInfo);
            rank++;
        }
        return result;
    }
    public static Tuple<List<PlayerInfo>, int> GetTopPlayers(List<PlayerInfo> players, SortPlayerType sortType, int num)
    {
        // Create a new PlayerInfo for the current user
        PlayerInfo user = new PlayerInfo
        {
            userName = Profile.UserName + "_" + Profile.UserTag, // TODO: replace with actual username when available
            point = Profile.Level,
            coinReward = Profile.CoinCollected,
            starReward = Profile.StarCollected,
            avt = Profile.CurAvt,
            isCurPlayer = true
        };

        // Add the current user to the players list
        players.Add(user);

        // Sort the list based on the selected sort type
        switch (sortType)
        {
            case SortPlayerType.POINT:
                players.Sort((a, b) => b.point.CompareTo(a.point));
                break;
            case SortPlayerType.COIN:
                players.Sort((a, b) => b.coinReward.CompareTo(a.coinReward));
                break;
            case SortPlayerType.STAR:
                players.Sort((a, b) => b.starReward.CompareTo(a.starReward));
                break;
        }
        int userRank = 0;
        if (players.Contains(user))
            userRank = players.IndexOf(user) + 1;
        // Get the top players up to the specified number
        return new Tuple<List<PlayerInfo>, int>(players.GetRange(0, Mathf.Min(num, players.Count)), userRank);
    }

}
[System.Serializable]
public enum SortPlayerType
{
    POINT = 0,
    COIN = 1,
    STAR = 2,
}
