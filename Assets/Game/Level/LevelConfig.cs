using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public List<LevelStats> configs = new List<LevelStats>() { new LevelStats() };
    public List<LevelDifficultConfig> diffs = new List<LevelDifficultConfig>() { new LevelDifficultConfig() };
    public List<int> unlockToolLevels = new List<int>();
    public static int GetStarEarn(LevelStats levelStats, float time)
    {
        if (time < levelStats.threeStarTime)
            return 3;
        else if (time < levelStats.twoStarTime)
            return 2;
        else
            return 1;
    }
    public static float GetStarProgress(LevelStats levelStats, float time)
    {
        if (time <= levelStats.threeStarTime)
        {
            //return 1/2 + ((levelStats.threeStarTime - time) / levelStats.threeStarTime);
            return 0.5f + ((levelStats.threeStarTime - time) / levelStats.threeStarTime) * 0.5f;
        }
        else if (time <= levelStats.twoStarTime)
        {
            return (levelStats.twoStarTime - time) / (levelStats.twoStarTime - levelStats.threeStarTime) * 0.5f;
        }
        return 0.0f;
    }
    public static LevelDifficultConfig GetDiffConfig(LevelDifficult diff, LevelConfig levelConfig)
    {
        if ((int)diff >= levelConfig.diffs.Count)
            return new LevelDifficultConfig();

        return levelConfig.diffs[(int)diff];
    }
    [ContextMenu("AutoSetIndex")]
    public void AutoSetIndex()
    {
        int i = 0;
        foreach (LevelStats levelStats in configs)
        {
            levelStats.levelIndex = i + 1;
            i++;
        }
    }
    [ContextMenu("AutoRoundNumCoinEarn")]
    public void AutoRoundNumCoinEarn()
    {
        foreach (LevelStats levelStats in configs)
        {
            int baseValue = levelStats.coinEarn / 5;
            int remainder = levelStats.coinEarn % 5;

            if (remainder <= 2)
                levelStats.coinEarn = baseValue * 5;
            else
                levelStats.coinEarn = (baseValue + 1) * 5;
        }
    }
    [ContextMenu("AutoChangeCoinEarn")]
    public void AutoChangeCoinEarn()
    {
        foreach (LevelStats levelStats in configs)
        {
            levelStats.coinEarn = levelStats.coinEarn / 2;
        }

        AutoRoundNumCoinEarn();
    }    
}
[System.Serializable]
public class LevelStats
{
    public int levelIndex = 1;
    public int coinEarn = 20;

    public int twoStarTime = 300;
    public int threeStarTime = 180;

    public LevelDifficult diff = LevelDifficult.Easy;
    public bool fixColor = false;

    public LevelStats()
    {
        levelIndex = 1;
        coinEarn = 20;
        twoStarTime = 300;
        threeStarTime = 180;
        diff = LevelDifficult.Easy;
    }
}
[System.Serializable]
public enum LevelDifficult
{
    Easy = 0,
    Normal = 1,
    Hard = 2,
    Hell = 3,
}
[System.Serializable]
public class LevelDifficultConfig
{
    public int groupSize;
    public int minSameColorInRow;
    public int maxSameColorInRow;
    public LevelDifficultConfig()
    {
        groupSize = 4;
        minSameColorInRow = GlobalDefine.minSameColorPassengerInRow;
        maxSameColorInRow = GlobalDefine.maxSameColorPassengerInRow;
    }
}
