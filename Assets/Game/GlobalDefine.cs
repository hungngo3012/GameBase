using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GlobalDefine
{
    internal static readonly float carPosY = 0.05f;
    internal static readonly string LevelDataPath = "LevelsData/";
    internal static readonly string LevelProgressDataPath = "/levelProgress.json";
    internal static readonly string rankingDataPath = "rankingData";

    internal static readonly int minSameColorPassengerInRow = 2;
    internal static readonly int maxSameColorPassengerInRow = 8;

    internal static readonly float carMoveDuration = 30f;
    internal static readonly float carScaleWhenStop = 1.2f;

    internal static readonly float passengerScaleWhenSeat = 0.55f;

    internal static readonly float passengerWalkDuration = 9.0f;
    internal static readonly float passengerRunDuration = 0.2f;
    internal static readonly float passengerRunSpeed = 16.0f;

    internal static readonly int continuePlayPrice = 200;
    internal static readonly int numLevel = 10;

    internal static readonly float waitLoadConfigTimeout = 3.0f;

    internal static readonly float carMoveOutDuration = 0.5f;
    internal static readonly float shuffleDuration = 0.2f;

    internal static readonly float emojiWaitCreat = 7.0f;

    internal static readonly int numLevelLoop = 40;

    internal static string confirmChangeSkinMess = "You have switched to a different skin. Are you sure you want to select this skin?";
    internal static string confirmUnlockSkinMess = "Are you sure you want to unlock skin";
    internal static string unlockSkinFailMess = "You don't have enough stars to unlock skin";

    internal static int numDailySpin = 8;

    internal static readonly string[] realNames = {
        "John", "Emma", "Michael", "Olivia", "David", "Sophia",
        "James", "Isabella", "Daniel", "Mia", "Alex", "Hugo",
        "HungNgo", "Hieu", "Minh", "HaiNgo", "Duong", "NgocAnh",
        "Nam", "Tuan", "Huy", "Bale", "Vincent", "HarryKane",
        "Gru", "Isabella", "Quaker", "William", "Lucas", "Ethan",
        "Ella", "Liam", "Charlotte", "Amelia", "Mason", "Ava",
        "Elijah", "Scarlett", "Benjamin", "Sophie", "Noah", "Chloe",
        "Logan", "Zoe", "Henry", "Layla", "Leo", "Victoria",
        "Theo", "Harper", "Grace", "Lily", "Jack", "Riley",
        "Jaxon", "Ellie", "Alexander", "Hannah", "Jackson", "Aurora",
        "Oliver", "Aiden", "Matthew", "Addison", "Ryan", "Audrey",
        "Nathan", "Bella", "Samuel", "Sadie", "Sebastian", "Hazel",
        "Gabriel", "Mila", "Caleb", "Paisley", "Joshua", "Kinsley",
        "Aaron", "Melody", "Carter", "Luna", "Eleanor", "Maya",
        "Isaac", "Willow", "Lincoln", "Piper", "Julian", "Faith",
        "Dominic", "Serenity", "Miles", "Clara", "Ezra", "Ivy",
        "Luke", "Violet", "Hunter", "Brooklyn", "Wyatt", "Jasmine",
        "Jonathan", "Stella", "Evan", "Penelope", "Christian", "Camila"
    };

    internal static readonly float delayWinConfetti = 0.1f;
    internal static readonly float resetShowResumeAfterReward = 5.0f;
    internal static readonly float reloadBannerTime = 10.0f;
    internal static readonly float camOrSizeDefault = 18.0f;
    internal static readonly int showRatePopUpLevel = 6;
}
