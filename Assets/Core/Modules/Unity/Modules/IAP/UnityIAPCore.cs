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

        public bool Active => StoreController != null;

        public ListenerProperty Listener { get; protected set; }
        [Serializable]
        public class ListenerProperty : Property, IStoreListener
        {
            void IStoreListener.OnInitialized(IStoreController controller, IExtensionProvider extensions)
                => IAP.InitializeCompleteCallback(controller, extensions);

            void IStoreListener.OnInitializeFailed(InitializationFailureReason reason)
                => IAP.InitializeFailedCallback(reason);

            PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs args)
                => IAP.ProcessPurchaseHandler(args.purchasedProduct);

            void IStoreListener.OnPurchaseFailed(Product product, PurchaseFailureReason reason)
                => IAP.PurchaseFailedCallback(product, reason);
        }

        public ValidateProperty Validate { get; protected set; }
        public class ValidateProperty : Property
        {
            public GoogleProperty Google { get; protected set; }
            public class GoogleProperty : Element<ValidateGooglePlayPurchaseRequest, ValidateGooglePlayPurchaseResult>
            {
                public override AppStore AppStore => AppStore.GooglePlay;

                public override MethodDelegate Method => PlayFabClientAPI.ValidateGooglePlayPurchase;

                protected override void FillRequest(ref ValidateGooglePlayPurchaseRequest request, Product product)
                {
                    var reciept = Purchase.FromJson(product.receipt);

                    request.CatalogVersion = Core.PlayFab.Title.Catalog.Name;

                    request.CurrencyCode = product.metadata.isoCurrencyCode;
                    request.PurchasePrice = (uint)product.metadata.localizedPrice * 100;
                    request.ReceiptJson = reciept.PayloadData.json;
                    request.Signature = reciept.PayloadData.signature;
                }

                public override IList<PurchaseReceiptFulfillment> ExtractFulFillments(ValidateGooglePlayPurchaseResult result)
                {
                    return result.Fulfillments;
                }

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

            public abstract class Element<TRequest, TResult> : Element
                where TRequest : PlayFabRequestCommon, new()
                where TResult : PlayFabResultCommon
            {
                public delegate void MethodDelegate(TRequest request, Action<TResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null);

                public abstract MethodDelegate Method { get; }

                public override void Request(Product product)
                {
                    var request = GenerateRequest();

                    FillRequest(ref request, product);

                    Send(request, product);
                }
                protected virtual TRequest GenerateRequest()
                {
                    var request = new TRequest();

                    return request;
                }
                protected abstract void FillRequest(ref TRequest request, Product product);
                protected virtual void Send(TRequest request, Product product)
                {
                    Method.Invoke(request, ResultCallback, ErrorCallback);

                    void ResultCallback(TResult result) => this.ResultCallback(result, product);
                }

                public abstract IList<PurchaseReceiptFulfillment> ExtractFulFillments(TResult result);

                public MoeEvent<ResultData> OnResult { get; protected set; } = new MoeEvent<ResultData>();
                protected virtual void ResultCallback(TResult result, Product product)
                {
                    var fulfillments = ExtractFulFillments(result);

                    var data = new ResultData(result, product, fulfillments);

                    OnResult.Invoke(data);

                    Respond(data, null);
                }

                public MoeEvent<PlayFabError> OnError { get; protected set; } = new MoeEvent<PlayFabError>();
                protected virtual void ErrorCallback(PlayFabError error)
                {
                    OnError.Invoke(error);

                    Respond(null, error);
                }

                public MoeEvent<ResponseData> OnResponse { get; protected set; } = new MoeEvent<ResponseData>();
                protected virtual void Respond(ResultData result, PlayFabError error)
                {
                    var data = new ResponseData(result, error);

                    OnResponse.Invoke(data);
                }
            }
            public abstract class Element : Property
            {
                public abstract AppStore AppStore { get; }

                public abstract void Request(Product product);

                public class ResultData
                {
                    public PlayFabResultCommon Source { get; protected set; }

                    public Product Product { get; protected set; }

                    public IList<PurchaseReceiptFulfillment> Fulfillments { get; protected set; }

                    public ResultData(PlayFabResultCommon result, Product product, IList<PurchaseReceiptFulfillment> fulfillments)
                    {
                        this.Source = result;

                        this.Product = product;

                        this.Fulfillments = fulfillments;
                    }
                }

                public class ResponseData
                {
                    public ResultData Result { get; protected set; }

                    public PlayFabError Error { get; protected set; }
                    public bool HasError => Error != null;

                    public bool Success => Error == null;

                    public ResponseData(ResultData result, PlayFabError error)
                    {
                        this.Result = result;

                        this.Error = error;
                    }
                }
            }

            public List<Element> Elements { get; protected set; }

            public Element this[AppStore store] => Get(store);

            public virtual Element Get(AppStore store)
            {
                for (int i = 0; i < Elements.Count; i++)
                    if (Elements[i].AppStore == store)
                        return Elements[i];

                return null;
            }

            public Element Module => this[IAP.Store];

            public class Property : Core.Property<ValidateProperty>
            {
                public ValidateProperty Validate => Reference;
            }

            public override void Configure(UnityIAPCore reference)
            {
                base.Configure(reference);

                Elements = new List<Element>();

                Google = new GoogleProperty();

                Register(Google);
            }

            public virtual void Register<TRequest, TResult>(Element<TRequest, TResult> element)
                where TRequest : PlayFabRequestCommon, new()
                where TResult : PlayFabResultCommon
            {
                base.Register(this, element);

                Elements.Add(element);

                element.OnResponse.Add((Element.ResponseData data) => ResponseCallback(element, data));
            }

            public virtual void Request(Product product) => Module.Request(product);

            public MoeEvent<Element, Element.ResultData> OnResult { get; protected set; } = new MoeEvent<Element, Element.ResultData>();
            protected virtual void ResultCallback(Element element, Element.ResultData result)
            {
                OnResult.Invoke(element, result);

                IAP.ConfirmPendingPurchase(result.Product);
            }

            public MoeEvent<Element, PlayFabError> OnError { get; protected set; } = new MoeEvent<Element, PlayFabError>();
            protected virtual void ErrorCallback(Element element, PlayFabError error)
            {
                OnError.Invoke(element, error);
            }

            public MoeEvent<Element, Element.ResponseData> OnResponse { get; protected set; } = new MoeEvent<Element, Element.ResponseData>();
            protected virtual void ResponseCallback(Element element, Element.ResponseData response)
            {
                if (response.HasError)
                    ErrorCallback(element, response.Error);
                else
                    ResultCallback(element, response.Result);

                OnResponse.Invoke(element, response);
            }
        }

        public class Property : Core.Property<UnityIAPCore>
        {
            public UnityIAPCore IAP => Reference;
        }

        public override void Configure(UnityCore reference)
        {
            base.Configure(reference);

            Listener = new ListenerProperty();
            Validate = new ValidateProperty();

            Register(this, Listener);
            Register(this, Validate);

            PlayFab.Title.Catalog.Get.OnResult.Add(CatalogRetriveCallback);
        }

        #region Initialize
        void CatalogRetriveCallback(GetCatalogItemsResult result) => Initialize(result.Catalog);

        void Initialize(IList<CatalogItem> items)
        {
            Store = RunTimePlatformToAppStore(Application.platform);

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

        public delegate void InitializeDelegate(IStoreController controller, IExtensionProvider extensions);
        public event InitializeDelegate OnInitialize;
        void InitializeCompleteCallback(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("IAP Initialzed Correctly");

            StoreController = controller;

            OnInitialize?.Invoke(controller, extensions);
        }

        public delegate void InitializeFailedDelegate(InitializationFailureReason reason);
        public event InitializeFailedDelegate OnInitializeFailed;
        void InitializeFailedCallback(InitializationFailureReason reason)
        {
            Debug.LogError("Store initialization failed, reason: " + reason);

            OnInitializeFailed.Invoke(reason);
        }
        #endregion

        #region Purchase
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

            if (Validate.Module == null)
            {
                if(Application.isEditor)
                {
                    Debug.LogWarning("IAP purchases won't be validated in editor");
                    return PurchaseProcessingResult.Complete;
                }
                else
                {
                    Debug.LogError("No validation module defined for store: " + Store);
                    return PurchaseProcessingResult.Complete;
                }
            }

            Debug.Log("Processing purchase validation for item: " + product.definition.storeSpecificId + " with transaction ID: " + product.transactionID);

            Validate.Module.Request(product);

            return PurchaseProcessingResult.Pending;
        }

        void ConfirmPendingPurchase(Product product)
        {
            if(StoreController == null)
            {
                Debug.LogError("No Store Controller set for Pending Purchase Confirmation");
                return;
            }

            StoreController.ConfirmPendingPurchase(product);
            Debug.Log("Confirming Purchase for: " + product.definition.id);
        }
        #endregion

        public delegate void PurchaseFailedDelegate(Product product, PurchaseFailureReason reason);
        public event PurchaseFailedDelegate OnPurchaseFailed;
        void PurchaseFailedCallback(Product product, PurchaseFailureReason reason)
        {
            Debug.LogError("Purchase of prodcut " + product.definition.storeSpecificId + " failed, reason: " + reason);

            OnPurchaseFailed?.Invoke(product, reason);
        }

        //Static Utility
        public static AppStore RunTimePlatformToAppStore(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return AppStore.GooglePlay;

                case RuntimePlatform.IPhonePlayer:
                    return AppStore.AppleAppStore;

                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return AppStore.fake;

                default:
                    return AppStore.NotSpecified;
            }
        }
    }

    public partial class Sandbox
    {
        public static Core Core => Core.Instance;

        [RuntimeInitializeOnLoadMethod()]
        static void IAPLoad()
        {
            Core.PlayFab.Login.OnResult.Add(LoginCallback);
        }

        static void LoginCallback(LoginResult result)
        {
            Core.PlayFab.Title.Catalog.Get.Request();
        }
    }
}