// This file is auto-generated. Modifications are not saved.

namespace NinthArt
{
	internal enum SceneID
	{
		GameManager,
		Gameplay,
		SettingUI,
		WinUI,
		LoseUI,
		Sort,
		Shuffle,
		VipSlot,
		ReplayUi,
		LoadingScene,
		ShopUi,
		PurchaseSuccessUi,
		StageUi,
		NotiUi,
		LuckyWheelUi,
		RankingUi,
		UnlockShuffle,
		UnlockSort,
		UnlockVipSlot,
		Home,
		HomeTab,
		InfoUi,
		ChangeName,
		RateUi,
		LoadResumeAds,
		END
	}

	internal static class Layers
	{
		internal const int Default = 0;
		internal const int TransparentFX = 1;
		internal const int Ignore_Raycast = 2;
		internal const int Water = 4;
		internal const int UI = 5;
		internal const int Road = 6;
		internal const int Car = 7;
		internal const int HideInCam = 8;
		internal const int Belt = 9;
		internal const int VipStop = 10;

		internal const int DefaultMask = 1 << 0;
		internal const int TransparentFXMask = 1 << 1;
		internal const int Ignore_RaycastMask = 1 << 2;
		internal const int WaterMask = 1 << 4;
		internal const int UIMask = 1 << 5;
		internal const int RoadMask = 1 << 6;
		internal const int CarMask = 1 << 7;
		internal const int HideInCamMask = 1 << 8;
		internal const int BeltMask = 1 << 9;
		internal const int VipStopMask = 1 << 10;
	}

	internal static class SceneNames
	{
		internal const string INVALID_SCENE = "InvalidScene";
		internal static readonly string[] ScenesNameArray = {
			"GameManager",
			"Gameplay",
			"SettingUI",
			"WinUI",
			"LoseUI",
			"Sort",
			"Shuffle",
			"VipSlot",
			"ReplayUi",
			"LoadingScene",
			"ShopUi",
			"PurchaseSuccessUi",
			"StageUi",
			"NotiUi",
			"LuckyWheelUi",
			"RankingUi",
			"UnlockShuffle",
			"UnlockSort",
			"UnlockVipSlot",
			"Home",
			"HomeTab",
			"InfoUi",
			"ChangeName",
			"RateUi",
			"LoadResumeAds"
		};
		internal static string GetSceneName(SceneID scene) {
			var index = (int)scene;
			if(index > 0 && index < ScenesNameArray.Length) {
				return ScenesNameArray[index];
			} else {
				return INVALID_SCENE;
			}
		}
	}

	internal static class ExtentionHelpers {
		internal static string GetName(this SceneID scene) {
			  return SceneNames.GetSceneName(scene);
		}
	}
}

