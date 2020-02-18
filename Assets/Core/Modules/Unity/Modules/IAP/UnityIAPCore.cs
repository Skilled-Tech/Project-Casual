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
                public override AppStore AppStore => AppStore.GooglePlay;

                public override MethodDelegate Method => PlayFabClientAPI.ValidateGooglePlayPurchase;

                protected override void FormatRequest(ref ValidateGooglePlayPurchaseRequest request, Product product)
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

            public abstract class Element<TRequest, TResult> : Element
                where TRequest : PlayFabRequestCommon, new()
                where TResult : PlayFabResultCommon
            {
                public delegate void MethodDelegate(TRequest request, Action<TResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null);

                public abstract MethodDelegate Method { get; }

                public override void Request(Product product)
                {
                    var request = GenerateRequest();

                    FormatRequest(ref request, product);

                    Send(request);
                }
                protected virtual TRequest GenerateRequest()
                {
                    var request = new TRequest();

                    return request;
                }
                protected abstract void FormatRequest(ref TRequest request, Product product);
                protected virtual void Send(TRequest request)
                {
                    Method.Invoke(request, ResultCallback, ErrorCallback);
                }

                public abstract IList<PurchaseReceiptFulfillment> ExtractFulFillments(TResult result);

                public MoeEvent<ResultData> OnResult { get; protected set; } = new MoeEvent<ResultData>();
                protected virtual void ResultCallback(TResult result)
                {
                    var fulfillments = ExtractFulFillments(result);

                    var data = new ResultData(result, fulfillments);

                    OnResult.Invoke(data);

                    Respond(data, null);
                }

                public MoeEvent<PlayFabError> OnError { get; protected set; } = new MoeEvent<PlayFabError>();
                protected virtual  void ErrorCallback(PlayFabError error)
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

                    public IList<PurchaseReceiptFulfillment> Fulfillments { get; protected set; }

                    public ResultData(PlayFabResultCommon result, IList<PurchaseReceiptFulfillment> fulfillments)
                    {
                        this.Source = result;

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

            public MoeEvent<Element, Element.ResultData> OnResult { get; protected set; } = new MoeEvent<Element, Element.ResultData>();
            protected virtual void ResultCallback(Element element, Element.ResultData result)
            {
                OnResult.Invoke(element, result);
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

            Listener = new ListenerProperty(ProcessPurchaseHandler);
            Validate = new ValidateProperty();

            Register(this, Listener);
            Register(this, Validate);

            PlayFab.Title.Catalog.Get.OnResult.Add(CatalogRetriveCallback);
        }

        public override void Init()
        {
            base.Init();

            Listener.InitializeEvent += InitializeCallback;
            Listener.InitializeFailEvent += InitializeFailedCallback;
            Listener.PurchaseFailEvent += PurchaseFailedCallback;

            Validate.OnResponse.Add(ValidateResponse);
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

            if (Validate.Module == null)
            {
                Debug.LogError("No validation module defined for store: " + Store);
                return PurchaseProcessingResult.Complete;
            }

            Debug.Log("Processing purchase for item: " + product.definition.storeSpecificId + " with transaction ID: " + product.transactionID);

            Validate.Module.Request(product);

            return PurchaseProcessingResult.Complete;
        }

        void ValidateResponse(ValidateProperty.Element element, ValidateProperty.Element.ResponseData response)
        {
            if(response.Success)
            {
                Debug.Log("Successfully Validated Purchase");
            }
            else
            {
                Debug.Log("Error Validating Purchase: " + response.Error.GenerateErrorReport());
            }
        }

        void PurchaseFailedCallback(Product product, PurchaseFailureReason reason)
        {
            Debug.LogError("Purchase of prodcut " + product.definition.storeSpecificId + " failed, reason: " + reason);
        }
    }

    public partial class Sandbox
    {
        public static Core Core => Core.Instance;

        [RuntimeInitializeOnLoadMethod()]
        static void IAPLoad()
        {
            Core.Unity.IAP.Listener.InitializeEvent += IAPInitializeCallback;

            Core.PlayFab.Login.OnResult.Add(LoginCallback);
        }

        static void LoginCallback(LoginResult result)
        {
            Core.PlayFab.Title.Catalog.Get.Request();
        }

        static void IAPInitializeCallback(IStoreController controller, IExtensionProvider extensions)
        {
            CoroutineManager.YieldSeconds(() =>
            {
                Core.Unity.IAP.Purchase("premium_pass");
            }, 0.2f);
        }
    }
}