using NinthArt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

[CreateAssetMenu(menuName = "ScriptableObjects/FakeLeaderboardData")]
public class FakeLeaderboardData : ScriptableObject
{
    public int topPoint = 1200;
    public int maxPlusStar = 2800;
    public int maxPlusCoin = 1000000;
    public int numPlayer = 1055;

    [ContextMenu("GenFakeData")]
    public void GenRandomData()
    {
        List<PlayerInfo> players = new List<PlayerInfo>();
        for (int i = 0; i < numPlayer; i++)
        {
            PlayerInfo player = new PlayerInfo();
            player.userName = GeneralCalculate.GenerateUsername();

            float rarityFactor = Mathf.Pow(Random.value, 5);
            player.point = Mathf.RoundToInt(topPoint * rarityFactor);
            if (player.point <= 0)
                player.point = 1;

            rarityFactor = Mathf.Pow(Random.value, 8);
            player.coinReward = Mathf.RoundToInt((player.point - 1) * Random.Range(50, 180) + Random.Range(0, maxPlusCoin) * rarityFactor);
            player.starReward = Mathf.RoundToInt((player.point - 1) * Random.Range(1.5f, 2.7f) + Random.Range(0, maxPlusStar) * rarityFactor);
            player.avt = Random.Range(0, 24);

            players.Add(player);
        }

        // Chuyển danh sách sang JSON
        string data = JsonConvert.SerializeObject(players);
        GeneralCalculate.SaveToJSON(data, Path.Combine(Application.dataPath, "Resources/" + GlobalDefine.rankingDataPath + ".json"));
    }

}

[System.Serializable]
public class PlayerInfo
{
    public string userName;

    public int point;
    public int coinReward;
    public int starReward;
    public int avt;

    internal bool isCurPlayer;
}
