using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace NinthArt
{
	internal class Profile : Singleton<Profile>
	{
		private const string Passphase = "Nama1234";
		private const string SaveFile = "/save.dat";

		[Serializable]
		private class UserData
		{
			// Soft currency
			[SerializeField] internal int nCoins;
			[SerializeField] internal int nStars;

			[SerializeField] internal int totalCoinsCollect;
			[SerializeField] internal int totalStarsCollect;
			// Level
			[SerializeField] internal int level = 1;
			[SerializeField] internal int playCount;

			// First open time
			[SerializeField] internal string firstTime;
			[SerializeField] internal bool firstAoa = true;
			[SerializeField] internal bool isPlayed;

			[SerializeField] internal List<string> skins = new List<string>() { "Default" };
			[SerializeField] internal int currentSkin;

			[SerializeField] internal List<Items> items = new List<Items>();

			[SerializeField] internal int curPurchaseProduct;

			[SerializeField] internal string lastCheckedDate;

			[SerializeField] internal int luckyWheelProgress = 0;

			[SerializeField] internal bool canCollectRent = false;

			[SerializeField] internal int unlockToolProgress = 0;

			[SerializeField] internal int curAvatar = 0;

			// AppFlyer
			[SerializeField] internal int interCount = 0;

			// User
			[SerializeField] internal string userName = "You";
			[SerializeField] internal string userTag = GeneralCalculate.GetRandomChars();

			//Rating
			[SerializeField] internal bool ratingPopupShown = false;
		}

		private UserData _data;
		private bool _vip;

		internal static bool Vip
		{
			get
            {
				if (!Config.Instance.hien_qc)//Neu hien_qc = false thi luon skip qc
					return true;

				return Instance._vip;
            }
			set
			{
				if (Instance._vip == value)
				{
					return;
				}

				Instance._vip = value;
				EventManager.Annouce(EventType.VipChanged);
			}
		}
		internal static bool CanCollectRent
		{
			get => Instance._data.canCollectRent;
			set
			{
				if (Instance._data.canCollectRent == value)
				{
					return;
				}

				Instance._data.canCollectRent = value;
			}
		}
		internal static bool RatingPopupShown
		{
			get => Instance._data.ratingPopupShown;
			set
			{
				if (Instance._data.ratingPopupShown == value)
				{
					return;
				}

				Instance._data.ratingPopupShown = value;
			}
		}

		private void Awake()
		{
			Initialize();
		}

		private void Initialize()
		{
			LoadLocal();
		}

		internal static int CurrentSkinIndex
		{
			get => Instance._data?.currentSkin ?? 0;
			set
			{
				if (Instance._data == null || value < 0 || value >= Instance._data.skins.Count) return;
				Instance._data.currentSkin = value;
				RequestSave();
			}
		}
		internal static int CurPurchaseProduct
        {
			get => Instance._data?.curPurchaseProduct ?? 0;
			set
            {
				if (Instance._data == null || value < 0) return;
				Instance._data.curPurchaseProduct = value;
				RequestSave();
			}
		}			
		internal static string CurrentSkin
		{
			get
			{
				if (Instance._data?.skins == null ||
					Instance._data.currentSkin < 0 ||
					Instance._data.currentSkin >= Instance._data.skins.Count)
					return string.Empty;

				return Instance._data.skins[Instance._data.currentSkin];
			}
			
			set
			{
				if (Instance._data?.skins == null)
					return;

				var index = Instance._data.skins.IndexOf(value);
				if (index < 0 || index >= Instance._data.skins.Count)
					return;

				Instance._data.currentSkin = index;
				RequestSave();
			}
		}

		internal static List<string> Skins => Instance._data?.skins;

		internal static void UnlockSkin(string skin)
		{
			if (Instance._data == null) return;
			if (string.IsNullOrEmpty(skin)) return;
			if (Instance._data.skins.Contains(skin)) return;
			Instance._data.currentSkin = Instance._data.skins.Count;
			Instance._data.skins.Add(skin);
			RequestSave();
		}
		internal static void SelectSkin(string skin)
        {
			int i = 0;
			foreach(string ownedSkin in Instance._data.skins)
            {
				if(ownedSkin == skin)
                {
					Instance._data.currentSkin = i;
					break;
				}
				i++;
            }

			RequestSave();
		}			
		internal static int CoinAmount
		{
			get => Instance._data?.nCoins ?? 0;
			set
			{
				if (Instance._data == null)
				{
					return;
				}

				Instance._data.nCoins = value;
				EventManager.Annouce(EventType.CoinAmountChanged);
				RequestSave();
			}
		}
		internal static int CoinCollected
		{
			get => Instance._data?.totalCoinsCollect ?? 0;
			set
			{
				if (Instance._data == null)
				{
					return;
				}

				Instance._data.totalCoinsCollect = value;
				RequestSave();
			}
		}
		internal static int StarAmount
		{
			get => Instance._data?.nStars ?? 0;
			set
			{
				if (Instance._data == null)
				{
					return;
				}

				Instance._data.nStars = value;
				EventManager.Annouce(EventType.StarAmountChanged);
				RequestSave();
			}
		}
		internal static int StarCollected
		{
			get => Instance._data?.totalStarsCollect ?? 0;
			set
			{
				if (Instance._data == null)
				{
					return;
				}

				Instance._data.totalStarsCollect = value;
				RequestSave();
			}
		}

		internal static int Level
		{
			get => Instance._data?.level ?? 1;
			set
			{
				if (Instance._data == null) return;
				Instance._data.level = value < 1 ? 1 : value;
				RequestSave();
			}
		}

		internal static int PlayCount
		{
			get => Instance._data?.playCount ?? 0;
			set
			{
				if (Instance._data == null) return;
				Instance._data.playCount = value;
				RequestSave();
			}
		}

		internal static DateTime FirstOpenTime
		{
			get
			{
				if (Instance._data == null || string.IsNullOrEmpty(Instance._data.firstTime)) return DateTime.Now;
				try
				{
					return DateTime.Parse(Instance._data.firstTime);
				}
				catch
				{
					// ignored
				}

				return DateTime.Now;
			}
		}
		internal static List<Items> Items
		{
			get => Instance._data?.items;
			set
			{
				if (Instance._data == null) return;
				Instance._data.items = value;
				RequestSave();
			}
		}
		internal static void AddItem(Items newItem)
		{
			if (Instance._data == null || Instance._data.items == null) return;

			Instance._data.items.Add(newItem);  // Th�m item v�o danh s�ch
			RequestSave();
		}
		internal static void RemoveItem(Items removeItems)
		{
			if (Instance._data == null || Instance._data.items == null) return;

			Instance._data.items.Remove(removeItems);  // Th�m item v�o danh s�ch
			RequestSave();
		}
		internal static DateTime LastCheckedTime
		{
			get
			{
				if (Instance._data == null || string.IsNullOrEmpty(Instance._data.lastCheckedDate))
				{
					Instance._data.lastCheckedDate = DateTime.Now.ToString(CultureInfo.CurrentCulture);
					RequestSave();
					return DateTime.Now;
				}

				try
				{
					return DateTime.Parse(Instance._data.lastCheckedDate);
				}
				catch
				{
					// ignored
				}

				Instance._data.lastCheckedDate = DateTime.Now.ToString(CultureInfo.CurrentCulture);
				RequestSave();
				return DateTime.Now;
			}
			set
			{
				if (Instance._data == null) return;
				Instance._data.lastCheckedDate = value.ToString(CultureInfo.CurrentCulture);
				RequestSave();
			}
		}
		internal static int LuckyWheelProgress
		{
			get => Instance._data?.luckyWheelProgress ?? 0;
			set
			{
				if (Instance._data == null)
				{
					return;
				}

				Instance._data.luckyWheelProgress = value;
				RequestSave();
			}
		}
		internal static int UnlockToolProgress
		{
			get => Instance._data?.unlockToolProgress ?? 0;
			set
			{
				if (Instance._data == null)
				{
					return;
				}

				Instance._data.unlockToolProgress = value;
				RequestSave();
			}
		}
		internal static int CurAvt
		{
			get => Instance._data?.curAvatar ?? 0;
			set
			{
				if (Instance._data == null)
				{
					return;
				}

				Instance._data.curAvatar = value;
				RequestSave();
			}
		}
		internal static int InterCount
		{
			get => Instance._data?.interCount ?? 0;
			set
			{
				if (Instance._data == null)
				{
					return;
				}

				Instance._data.interCount = value;
				RequestSave();
			}
		}
		internal static bool FirstAoa
		{
			get => Instance._data.firstAoa;
			set
			{
				if (Instance._data.firstAoa == value)
				{
					return;
				}

				Instance._data.firstAoa = value;
				RequestSave();
			}
		}
		internal static bool IsPlayed
		{
			get => Instance._data.isPlayed;
			set
			{
				if (Instance._data.isPlayed == value)
				{
					return;
				}

				Instance._data.isPlayed = value;
				RequestSave();
			}
		}
		internal static string UserName
		{
			get
			{
				if (Instance._data == null || string.IsNullOrEmpty(Instance._data.userName)) 
					return "";
				return Instance._data.userName;
			}
			set
            {
				if (Instance._data.userName == value)
				{
					return;
				}

				Instance._data.userName = value;
				RequestSave();
			}
		}
		internal static string UserTag
		{
			get
			{
				if (Instance._data == null || string.IsNullOrEmpty(Instance._data.userTag))
					return "";
				return Instance._data.userTag;
			}
		}
		private void LoadLocal()
		{
			try
			{
				TextReader tr = new StreamReader(Application.persistentDataPath + SaveFile);
				var encryptedJson = tr.ReadToEnd();
				tr.Close();

				var json = Security.Decrypt(encryptedJson, Passphase);
				_data = JsonUtility.FromJson<UserData>(json);
			}
			catch
			{
				// ignored
			}

			if (_data == null)
			{
				_data = new UserData {firstTime = DateTime.Now.ToString(CultureInfo.InvariantCulture)};
				RequestSave();
			}

			_data.skins ??= new List<string>();
			if (_data.skins.Count <= 0)
			{
				_data.skins.Add("Boy");
				_data.currentSkin = 0;
				RequestSave();
			}

			if (_data.level < 1)
			{
				_data.level = 1;
				RequestSave();
			}
		}

		private bool _modifed;

		private static void RequestSave()
		{
			Instance._modifed = true;
		}

		private void Update()
		{
			if (!_modifed) return;
			_modifed = false;
			SaveLocal();
		}

		private void SaveLocal()
		{
			try
			{
				var json = JsonUtility.ToJson(_data);
				var encryptedJson = Security.Encrypt(json, Passphase);

				TextWriter tw = new StreamWriter(Application.persistentDataPath + SaveFile);
				tw.Write(encryptedJson);
				tw.Close();
			}
			catch
			{
				// ignored
			}
		}
	}
	[Serializable]
	public class Items
	{
		public Item item;
		public int num;
	}
}