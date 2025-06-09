
namespace NinthArt
{
	enum EventType
	{
		// Event an iap item is purchased
		IapPurchased,

		// Event the user profile is reloaded
		ProfileReloaded,

		// Ball skin changed
		CharacterSkinChanged,

		// Theme changed
		ThemeChanged,

		// Coint amount changed
		CoinAmountChanged,

		// Key amount changed
		KeyAmountChanged,

		// Level complete
		LevelComplete,

		// Level restart
		RestartLevel,

		// VIP activated
		VipChanged,

		// Configurations loaded
		ConfigsLoaded,

		// Avaible item
		AvaibleItem,

		// Avaible item
		NewItem,

		// Level Pregress Chnaged
		LevelPregressChanged,

		// Show background gameplay
		ShowBackgroundGameplay,

		BuyCharacter,

		IapInitialized,

		NoAdsPurchased,

		StarAmountChanged,

		UpdateSkinEquip,

		UpdateSkinUi,

		CarGoToStop,

		NumItemChange,

		ShowBannerAds,

		UpdateInfo,

		GoToShop,

		CheckIAPSoldOut,
	}
}