using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

using UnityEngine.Purchasing;

using PlayFab;
using PlayFab.ClientModels;
using PlayFab.SharedModels;

namespace Game
{
	public class UnityIAPCore : UnityCore.Module
	{
        public PlayFabCore PlayFab => Core.PlayFab;

        public AppStore Store { get; protected set; }
        public IStoreController StoreController { get; protected set; }
        public IGooglePlayStoreExtensions PlayStoreExtensions { get; protected set; }

        public bool Active => StoreController != null;

        public ListenerProperty Listener { get; protected set; }
        public class ListenerProperty : Property, IStoreListener
        {
            public delegate void InitializeDelegate(IStoreController controller, IExtensionProvider extensions);
            public event InitializeDelegate InitializeEvent;
            void IStoreListener.OnInitialized(IStoreController controller, IExtensionProvider extensions)
            {
                if (InitializeEvent != null) InitializeEvent(controller, extensions);
            }

            public delegate void InitializeFailDelegate(InitializationFailureReason reason);
            public event InitializeFailDelegate InitializeFailEvent;
            void IStoreListener.OnInitializeFailed(InitializationFailureReason reason)
            {
                if (InitializeFailEvent != null) InitializeFailEvent(reason);
            }

            public delegate void PurchasePrcoessEventDelegate(Product product);
            public event PurchasePrcoessEventDelegate PurchaseProcessEvent;

            public delegate PurchaseProcessingResult PurchasePrcoessHandlerDelegate(Product product);
            public PurchasePrcoessHandlerDelegate PurchaseProcessHandler;

            PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs args)
            {
                if (PurchaseProcessEvent != null) PurchaseProcessEvent(args.purchasedProduct);

                return PurchaseProcessHandler(args.purchasedProduct);
            }

            public delegate void PurchaseFailDelegate(Product product, PurchaseFailureReason reason);
            public event PurchaseFailDelegate PurchaseFailEvent;
            void IStoreListener.OnPurchaseFailed(Product product, PurchaseFailureReason reason)
            {
                if (PurchaseFailEvent != null) PurchaseFailEvent(product, reason);
            }

            public ListenerProperty(PurchasePrcoessHandlerDelegate PurchaseHandler)
            {
                this.PurchaseProcessHandler = PurchaseHandler;
            }
        }

        public ValidateProperty Validate { get; protected set; }
        public class ValidateProperty : Property
        {
            public GoogleProperty Google { get; protected set; }
            public class GoogleProperty : Element<ValidateGooglePlayPurchaseRequest, ValidateGooglePlayPurchaseResult>
            {
                public override MethodDelegate Method => PlayFabClientAPI.ValidateGooglePlayPurchase;

                public class JsonData
                {
                    // Json Fields, ! Case-sensetive

                    public string orderId;
                    public string packageName;
                    public string productId;
                    public long purchaseTime;
                    public int purchaseState;
                    public string purchaseToken;
                }
                public class PayloadData
                {
                    public JsonData JsonData;

                    // Json Fields, ! Case-sensetive
                    public string signature;
                    public string json;

                    public static PayloadData FromJson(string json)
                    {
                        var payload = JsonUtility.FromJson<PayloadData>(json);
                        payload.JsonData = JsonUtility.FromJson<JsonData>(payload.json);
                        return payload;
                    }
                }
                public class Purchase
                {
                    public PayloadData PayloadData;

                    // Json Fields, ! Case-sensetive
                    public string Store;
                    public string TransactionID;
                    public string Payload;

                    public static Purchase FromJson(string json)
                    {
                        var purchase = JsonUtility.FromJson<Purchase>(json);
                        purchase.PayloadData = PayloadData.FromJson(purchase.Payload);
                        return purchase;
                    }
                }
            }

            public abstract class Element<TRequest, TResult> : Property
                where TRequest : PlayFabRequestCommon
                where TResult : PlayFabResultCommon
            {
                public delegate void MethodDelegate(TRequest request, Action<TResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null);

                public abstract MethodDelegate Method { get; }

                protected virtual void Request(TRequest request)
                {
                    Method.Invoke(request, RetrieveCallback, ErrorCallback);
                }

                public void Request(Product product)
                {
                    var reciept = Purchase.FromJson(product.receipt);

                    var currencyCode = product.metadata.isoCurrencyCode;
                    var purchaseprice = (uint)product.metadata.localizedPrice * 100;
                    var recieptJson = reciept.PayloadData.json;
                    var signature = reciept.PayloadData.signature;

                    Request(currencyCode, purchaseprice, recieptJson, signature);
                }
                public void Request(string currencyCode, uint purchasePrice, string reciept, string signature)
                {
                    Debug.Log("Validating " + Environment.NewLine + reciept + Environment.NewLine + signature);

                    var request = new ValidateGooglePlayPurchaseRequest
                    {
                        CatalogVersion = PlayFab.Catalog.Version,
                        CurrencyCode = currencyCode,
                        PurchasePrice = purchasePrice,
                        ReceiptJson = reciept,
                        Signature = signature,
                    };

                    PlayFabClientAPI.ValidateGooglePlayPurchase(request, RetrieveCallback, ErrorCallback);
                }

                public MoeEvent<TResult> OnResult { get; protected set; }
                void RetrieveCallback(TResult result)
                {
                    OnResult.Invoke(result);

                    Respond(result, null);
                }

                public MoeEvent<PlayFabError> OnError { get; protected set; }
                protected virtual  void ErrorCallback(PlayFabError error)
                {
                    OnError.Invoke(error);

                    Respond(null, error);
                }

                public MoeEvent<ResponseData> OnResponse { get; protected set; }
                protected virtual void Respond(TResult result, PlayFabError error)
                {
                    var data = new ResponseData(result, error);

                    Respond(data);
                }
                protected virtual void Respond(ResponseData data)
                {
                    OnResponse.Invoke(data);
                }

                public class ResponseData
                {
                    public TResult Result { get; protected set; }

                    public PlayFabError Error { get; protected set; }
                    public bool HasError => Error != null;

                    public bool Success => Error == null;

                    public ResponseData(TResult result, PlayFabError error)
                    {
                        this.Result = result;

                        this.Error = error;
                    }
                }
            }

            public class Property : Core.Property<ValidateProperty>
            {
                public ValidateProperty Validate => Reference;
            }

            public override void Configure(UnityIAPCore reference)
            {
                base.Configure(reference);

                Register(this, Google);
            }
        }

        public class Property : Core.Property<UnityIAPCore>
        {
            public UnityIAPCore IAP => Reference;
        }

        public PopupUI Popup => Core.UI.Popup;

        public override void Configure(UnityCore reference)
        {
            base.Configure(reference);

            Listener = new ListenerProperty(ProcessPurchaseHandler);
            Listener.InitializeEvent += InitializeCallback;
            Listener.InitializeFailEvent += InitializeFailedCallback;
            Listener.PurchaseFailEvent += PurchaseFailedCallback;

            Register(this, Listener);

            PlayFab.Title.Catalog.Get.OnResult.Add(CatalogRetriveCallback);
        }

        void CatalogRetriveCallback(GetCatalogItemsResult result) => Initialize(result.Catalog);

        void Initialize(IList<CatalogItem> items)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    Store = AppStore.GooglePlay;
                    break;

                case RuntimePlatform.IPhonePlayer:
                    Store = AppStore.AppleAppStore;
                    break;

                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    Store = AppStore.fake;
                    break;

                default:
                    Store = AppStore.NotSpecified;
                    break;
            }

            var module = StandardPurchasingModule.Instance(Store);

            var builder = ConfigurationBuilder.Instance(module);

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].VirtualCurrencyPrices == null) continue;

                if (items[i].VirtualCurrencyPrices.ContainsKey("RM") == false) continue;

                builder.AddProduct(items[i].ItemId, ProductType.Consumable);
            }

            UnityPurchasing.Initialize(Listener, builder);
        }

        void InitializeCallback(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("IAP Initialzed Correctly");

            StoreController = controller;

            PlayStoreExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();
        }
        void InitializeFailedCallback(InitializationFailureReason error)
        {
            Debug.LogError("Store initialization failed, reason: " + error);
        }

        public void Purchase(string productID)
        {
            if (Active == false) throw new Exception("IAP Core is not initialized");

            StoreController.InitiatePurchase(productID);
        }

        PurchaseProcessingResult ProcessPurchaseHandler(Product product)
        {
            if (product == null)
            {
                Debug.LogError("Attempted to process purchase with unknown product, ignoring");
                return PurchaseProcessingResult.Complete;
            }

            if (string.IsNullOrEmpty(product.receipt))
            {
                Debug.LogError("Attempted to process purchase with no receipt, ignoring");
                return PurchaseProcessingResult.Complete;
            }

            Debug.Log("Processing purchase for item: " + product.definition.storeSpecificId + " with transaction ID: " + product.transactionID);

            switch (Store)
            {
                case AppStore.GooglePlay://TODO
                    break;

                case AppStore.AppleAppStore: //TODO implement Apple App Store Purchase Process
                    break;

                default:
                    Debug.LogError("IAP Purchase Process not implemented for " + Application.platform);
                    break;
            }

            return PurchaseProcessingResult.Complete;
        }

        void PurchaseFailedCallback(Product product, PurchaseFailureReason reason)
        {
            Debug.LogError("Purchase of prodcut " + product.definition.storeSpecificId + " failed, reason: " + reason);
        }
    }
}