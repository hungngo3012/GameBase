using AppsFlyerSDK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

namespace NinthArt
{
    internal class IAP : Singleton<IAP>, IDetailedStoreListener
    {
        private IStoreController m_Controller;

        private IAppleExtensions m_AppleExtensions;
        //private ISamsungAppsExtensions m_SamsungExtensions;
        private IMicrosoftExtensions m_MicrosoftExtensions;
        private ITransactionHistoryExtensions m_TransactionHistoryExtensions;
        private IGooglePlayStoreExtensions m_GooglePlayStoreExtensions;
        [SerializeField] private string pubKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAqgScc5wpciI3zt3SWIMmKqO+16E8GpDsQaQSf0iSermGb9gxyTqaYBSoDe0IH7cpDCjQhTisIJUWflk8n8OhdQNsxAmHAButPjZLeWhImgI7Jjpq2fyvszXhhb85Sn1gp4EVhRo+UB5UHnbOTLhTjreSGstMlVijN14v5kYPlnsPregCDh1Xi3/RPmYiiu+YBLYyfep+D+mSWMGRCJK0IRZwxvWdNsCldYB29G9cAvhtIru+A06zChc/yXjqFBrRcfo9xhD46qIn4bU+kRKxPzCk1FEGgjUK4gIRl7sZzNJF3nPerVAhi89tchqO1q+5YYhPhtegyJjAHcSYVjpOHQIDAQAB";
        List<int> purchasedNonConsumables = new List<int>();
        public static string kProductID;
        [Serializable]
        public class Item
        {
            public Item(string id, ProductType type, bool enabled = true)
            {
                this.id = id;
                this.type = type;
                this.enabled = enabled;
            }

            public string id;
            public ProductType type;
            public bool enabled;
            public bool available = false;
        }

        Dictionary<string, Item> itemMap = new Dictionary<string, Item>(12);

        internal static List<Pack> availableItems = new List<Pack>(GameManager.Instance.iapItems.items);

        static IStoreController storeController = null;
        IExtensionProvider storeExtensionProvider = null;

        public static bool Initialized
        {
            get { return storeController != null; }
        }

        public static bool Purchasing { get; private set; }

        public void Init()
        {
            UnityServices.InitializeAsync();
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            EventManager.Subscribe(EventType.IapPurchased, OnPurChased);

#if UNITY_IOS
			var storeName = AppleAppStore.Name;
#elif UNITY_ANDROID
            var storeName = GooglePlay.Name;
#else
			var storeName = GooglePlay.Name;
#endif
            foreach (var item in availableItems)
            {
                itemMap.Add(item.iapItem.id, item.iapItem);
                builder.AddProduct(
                    item.iapItem.id,
                    item.iapItem.type,
                    new IDs() { { item.iapItem.id, storeName } });
            }

            Debug.Log("IAP call init");
            UnityPurchasing.Initialize(this, builder);
        }
        public static void Purchase(int index)
        {
            Debug.Log("IAP on click purchase: " + index);
            if (availableItems.Count > 0)
            {
                Debug.Log("IAP availableItems.Count > 0");
                Purchase(availableItems[index].iapItem.id);
                kProductID = availableItems[index].iapItem.id;
                //Profile.CurPurchaseProduct = index;
                //SceneManager.OpenScene(SceneID.PurchaseSuccessUi);
            }
            else
            {
                Debug.Log("IAP availableItems.Count <= 0");
            }
        }
        public static string GetProductPrice(int index)
        {
            Debug.Log("IAP call get product price: " + index);
            if (availableItems.Count > 0)
            {
                Debug.Log("IAP get product availableItems.Count > 0");
                return GetProductPrice(availableItems[index].iapItem.id);
            }
            else
            {
                Debug.Log("IAP get product availableItems.Count <= 0");
                return null;
            }
        }
        public static void OnPurChased(object message)
        {
            if (!(message is string id)) return;

            int i = 0;
            foreach (var product in storeController.products.all)
            {
                //if (!Instance.itemMap.TryGetValue(product.definition.id, out var item)) continue;
                var hasPurchasedBefore =
                    product.hasReceipt ||
                    !string.IsNullOrEmpty(product.receipt) ||
                    !string.IsNullOrEmpty(product.transactionID);
                if (hasPurchasedBefore)
                {
                    if (product.definition.type == ProductType.NonConsumable)
                    {
                        if(!Instance.purchasedNonConsumables.Contains(i))
                            Instance.purchasedNonConsumables.Add(i);
                    }
                }
                i++;
            }
            int index = 0;
            foreach (Pack item in availableItems)
            {
                if(id.Equals(item.iapItem.id))
                {
                    EventManager.Annouce(EventType.CheckIAPSoldOut);
                    Profile.CurPurchaseProduct = index;
                    SceneManager.OpenPopup(SceneID.PurchaseSuccessUi);
                    break;
                }
                index++;
            }              
            Purchased();
        }   
        private static void Purchase(string productId)
        {
            if (!Initialized)
            {
                Debug.Log("IAP not initialized");
                return;
            }
            Purchasing = true;
            Debug.Log("IAP purchase: " + productId);
            var product = storeController.products.WithID(productId);
            if (product != null)
            {
                Debug.Log("IAP purchased: " + productId);
                storeController.InitiatePurchase(product);
            }
        }
        private static string GetProductPrice(string productId)
        {
            if (Initialized)
            {
                Product product = storeController.products.WithID(productId);
                if (product != null)
                {
                    return product.metadata.localizedPriceString;
                }
            }

            return null; // Giá không có sẵn
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            m_Controller = controller;
            m_AppleExtensions = extensions.GetExtension<IAppleExtensions>();
            //m_SamsungExtensions = extensions.GetExtension<ISamsungAppsExtensions>();
            m_MicrosoftExtensions = extensions.GetExtension<IMicrosoftExtensions>();
            m_TransactionHistoryExtensions = extensions.GetExtension<ITransactionHistoryExtensions>();
            m_GooglePlayStoreExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();
            // Purchasing has succeeded initializing. Collect our Purchasing references.
            // Overall Purchasing system, configured with products for this application.
            storeController = controller;
            // Store specific subsystem, for accessing device-specific store features.
            storeExtensionProvider = extensions;

            if (storeController != null)
                EventManager.Annouce(EventType.IapInitialized);

            Dictionary<string, string> introductory_info_dict = m_AppleExtensions.GetIntroductoryPriceDictionary();
            int i = 0;
            foreach (var product in controller.products.all)
            {
                var hasPurchasedBefore =
                    product.hasReceipt ||
                    !string.IsNullOrEmpty(product.receipt) ||
                    !string.IsNullOrEmpty(product.transactionID);
                if (hasPurchasedBefore)
                {                    
                    if (product.definition.type == ProductType.NonConsumable)
                    {
                        Debug.Log("Non consumable");
                        purchasedNonConsumables.Add(i);
                    }
                    if (availableItems[i].isRemoveAds && IsNonConsumPurchased(i))
                    {
                        Debug.Log("vip purchased: " + i);
                        Profile.Vip = true;
                    }
                    if (product.receipt!=null)
                    {
                        if (product.definition.type == ProductType.Subscription)
                        {
                            /*if (CheckIfProductIsAvailableForSubscriptionManager(product.receipt))
                            {*/
                                string intro_json =
                                    (introductory_info_dict == null ||
                                     !introductory_info_dict.ContainsKey(product.definition.storeSpecificId))
                                        ? null
                                        : introductory_info_dict[product.definition.storeSpecificId];
                                SubscriptionManager p = new SubscriptionManager(product, intro_json);
                                SubscriptionInfo info = p.getSubscriptionInfo();
                                if (info.isSubscribed() == Result.True)
                                {
                                    Debug.Log("Subscribed: " + i);
                                    //Profile.Vip = true; //tmp
                                }
                            /*}
                            else
                            {
                                Debug.Log(
                                    "This product is not available for SubscriptionManager class, only products that are purchase by 1.19+ SDK can use this class.");
                            }*/
                        }
                        else
                        {
                            Debug.LogError("the product is not a subscription product: " + product.definition.type);
                        }
                    }
                    else
                    {
                        Debug.LogError("the product should have a valid receipt: " + product.ToString());
                    }
                }
                /*
                if (!item.enabled) continue;
                item.available = true;

                Pack newPack = new Pack();
                newPack.iapItem = item;
                availableItems.Add(newPack);*/
                i++;
            }
            Debug.Log("IAP Initialized: " + storeController.ToString());
        }
        public void OnInitializeFailed(InitializationFailureReason reason)
        {
            Debug.LogError($"IAP initialization failed: {reason}");

            // You can add more specific logging depending on the reason
            switch (reason)
            {
                case InitializationFailureReason.AppNotKnown:
                    Debug.LogError("App not recognized by the store.");
                    break;
                case InitializationFailureReason.PurchasingUnavailable:
                    Debug.LogError("Purchasing is unavailable.");
                    break;
                case InitializationFailureReason.NoProductsAvailable:
                    Debug.LogError("No products available for purchase.");
                    break;
            }
        }
        private static bool CheckIfProductIsAvailableForSubscriptionManager(string receipt)
        {
            var receipt_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
            if (!receipt_wrapper.ContainsKey("Store") || !receipt_wrapper.ContainsKey("Payload"))
            {
                Debug.Log("The product receipt does not contain enough information");
                return false;
            }

            var store = (string)receipt_wrapper["Store"];
            var payload = (string)receipt_wrapper["Payload"];

            if (payload == null) return false;
            switch (store)
            {
                case GooglePlay.Name:
                {
                    var payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
                    if (!payload_wrapper.ContainsKey("json"))
                    {
                        Debug.Log(
                            "The product receipt does not contain enough information, the 'json' field is missing");
                        return false;
                    }

                    var original_json_payload_wrapper =
                        (Dictionary<string, object>)MiniJson.JsonDecode((string)payload_wrapper["json"]);
                    if (original_json_payload_wrapper == null ||
                        !original_json_payload_wrapper.ContainsKey("developerPayload"))
                    {
                        Debug.Log(
                            "The product receipt does not contain enough information, the 'developerPayload' field is missing");
                        return false;
                    }

                    var developerPayloadJSON = (string)original_json_payload_wrapper["developerPayload"];
                    var developerPayload_wrapper =
                        (Dictionary<string, object>)MiniJson.JsonDecode(developerPayloadJSON);
                    if (developerPayload_wrapper == null ||
                        !developerPayload_wrapper.ContainsKey("is_free_trial") ||
                        !developerPayload_wrapper.ContainsKey("has_introductory_price_trial"))
                    {
                        Debug.Log(
                            "The product receipt does not contain enough information, the product is not purchased using 1.19 or later");
                        return false;
                    }

                    return true;
                }
                case AppleAppStore.Name:
                case AmazonApps.Name:
                case MacAppStore.Name:
                {
                    return true;
                }
                default:
                {
                    return false;
                }
            }

            return false;
        }
        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError("IAP error: " + error.ToString());
            throw new System.NotImplementedException();
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            if(Purchasing)
                EventManager.Annouce(EventType.IapPurchased, e.purchasedProduct.definition.id);

            var product = e.purchasedProduct;
            string prodID = e.purchasedProduct.definition.id;
            string price = (e.purchasedProduct.metadata.localizedPrice * (decimal)Config.Instance.af_net_IAP_android).ToString(CultureInfo.InvariantCulture);
            string currency = e.purchasedProduct.metadata.isoCurrencyCode;

            string receipt = e.purchasedProduct.receipt;
            var recptToJSON = (Dictionary<string, object>)AFMiniJSON.Json.Deserialize(product.receipt);
            var receiptPayload = (Dictionary<string, object>)AFMiniJSON.Json.Deserialize((string)recptToJSON["Payload"]);
            var transactionID = product.transactionID;

            if (String.Equals(e.purchasedProduct.definition.id, kProductID, StringComparison.Ordinal))
            {
#if UNITY_IOS

            if(isSandbox)
            {
                AppsFlyeriOS.setUseReceiptValidationSandbox(true);
            }

            AppsFlyeriOS.validateAndSendInAppPurchase(prodID, price, currency, transactionID, null, this);
#elif UNITY_ANDROID
                var purchaseData = (string)receiptPayload["json"];
                var signature = (string)receiptPayload["signature"];
                AppsFlyer.validateAndSendInAppPurchase(
                    pubKey, signature,
                    purchaseData,
                    price,
                    currency,
                    null,
                    this);
#endif
            }
            print("???" + product.definition.id);

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
        {
            Purchasing = false;
        }

        /// <summary>
        /// Restore purchases previously made by this customer. 
        /// Some platforms automatically restore purchases. 
        /// Apple currently requires explicit purchase restoration for IAP.
        /// </summary>
        public void RestorePurchases(bool autoRestore = false)
        {
            // If Purchasing has not yet been set up ...
            if (!Initialized)
            {
                return;
            }

            // TODO: Show loading

            // Begin the asynchronous process of restoring purchases. Expect a confirmation response in the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            storeExtensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions((result) =>
            {
                if (result)
                {
                    // This does not mean anything was restored,
                    // merely that the restoration process succeeded.
                }
                else
                {
                    // Restoration failed.
                }

                // TODO: Hide loading
            });
        }
        internal static void Purchased()
        {
            Purchasing = false;
        }
        private void OnDestroy()
        {
            EventManager.Unsubscribe(EventType.IapPurchased, OnPurChased);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Purchasing = false;
        }
        public static bool IsNonConsumablePurchased(int index)
        {
            foreach(int i in Instance.purchasedNonConsumables)
            {
                if (i != index)
                    continue;
                return true;
            }
            return false;
        }
        public static bool IsNonConsumPurchased(int index)
        {
            if (availableItems.Count > index && (availableItems[index].iapItem.type == ProductType.NonConsumable))
            {
                if (Instance.purchasedNonConsumables.Contains(index))
                {
                    return IsNonConsumablePurchased(availableItems[index].iapItem.id);
                }    
                else
                {
                    return false;
                }    
            }

            return false;
            //return IsNonConsumablePurchased(availableItems[index].iapItem.id);
        }
        public static bool IsNonConsumablePurchased(string productId)
        {
            if (Instance.m_Controller == null)
            {
                Debug.LogError("StoreController is not initialized.");
                return false; // Nếu chưa khởi tạo, coi như chưa mua
            }

            // Lấy sản phẩm từ StoreController
            var product = Instance.m_Controller.products.WithID(productId);
            if (product == null)
            {
                Debug.LogError($"Product {productId} not found in StoreController.");
                return false; // Nếu không tìm thấy sản phẩm, coi như chưa mua
            }

            var purchaseState = Instance.m_GooglePlayStoreExtensions.GetPurchaseState(product);
            if (purchaseState == GooglePurchaseState.Purchased)
            {
                return true;
            }
            else if (purchaseState == GooglePurchaseState.Refunded)
            {
                print("The product has been refunded: " + productId);
            }
            else
            {
                print("The product is in an unknown state: " + purchaseState);
            }

            Debug.Log($"Product {productId} has not been purchased.");
            return false; // Chưa mua, có thể mua
        }
    }
}