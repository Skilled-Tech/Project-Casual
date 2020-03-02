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

using PlayFab;
using PlayFab.ClientModels;
using PlayFab.SharedModels;

using Newtonsoft.Json;

namespace Game
{
    public partial class PlayFabCore
    {
        [Serializable]
        public class PlayerProperty : Property
        {
            [SerializeField]
            protected ProfileProperty profile;
            public ProfileProperty Profile { get { return profile; } }
            [Serializable]
            public class ProfileProperty : Property
            {
                public string ID { get; protected set; }

                [SerializeField]
                protected DisplayNameProperty displayName;
                public DisplayNameProperty DisplayName => displayName;
                [Serializable]
                public class DisplayNameProperty : Element
                {
                    [SerializeField]
                    protected string value;
                    public string Value => value;

                    public bool IsValid => string.IsNullOrEmpty(value) == false;

                    public UpdateRequest Update { get; protected set; }
                    public class UpdateRequest : Request<UpdateUserTitleDisplayNameRequest, UpdateUserTitleDisplayNameResult>
                    {
                        public override MethodDelegate Method => PlayFabClientAPI.UpdateUserTitleDisplayName;

                        public virtual void Request(string desiredName)
                        {
                            var request = GenerateRequest();

                            request.DisplayName = desiredName;

                            Send(request);
                        }
                    }

                    public override void Configure(ProfileProperty reference)
                    {
                        base.Configure(reference);

                        Update = new UpdateRequest();
                        Update.OnResult.Add(UpdateCallback);
                    }

                    protected virtual void UpdateCallback(UpdateUserTitleDisplayNameResult result)
                    {
                        Load(result.DisplayName);

                        InvokeUpdate();
                    }

                    protected override void Load(GetPlayerCombinedInfoResultPayload payload)
                    {
                        Load(payload?.PlayerProfile?.DisplayName);
                    }
                    protected virtual void Load(string value)
                    {
                        this.value = value;
                    }

                    public override void Clear()
                    {
                        value = string.Empty;
                    }
                }

                [SerializeField]
                protected StatisticsProperty statistics;
                public StatisticsProperty Statistics => statistics;
                [Serializable]
                public class StatisticsProperty : Element
                {
                    [SerializeField]
                    protected List<StatisticValue> list;
                    public List<StatisticValue> List => list;

                    public virtual bool Contains(string name)
                    {
                        for (int i = 0; i < list.Count; i++)
                            if (list[i].StatisticName == name)
                                return true;

                        return false;
                    }
                    public virtual StatisticValue Find(string name)
                    {
                        for (int i = 0; i < list.Count; i++)
                            if (list[i].StatisticName == name)
                                return list[i];

                        return null;
                    }

                    public UpdateRequest Update { get; protected set; }
                    public class UpdateRequest : Request<UpdatePlayerStatisticsRequest, UpdatePlayerStatisticsResult>
                    {
                        public override MethodDelegate Method => PlayFabClientAPI.UpdatePlayerStatistics;

                        public virtual void Request(string name, int value)
                        {
                            var request = GenerateRequest();

                            request.Statistics = new List<StatisticUpdate>()
                        {
                            new StatisticUpdate()
                            {
                                StatisticName = name,
                                Value = value
                            }
                        };

                            Send(request);
                        }

                        public override void ResultCallback(UpdatePlayerStatisticsResult result)
                        {
                            base.ResultCallback(result);
                        }
                    }

                    public override void Configure(ProfileProperty reference)
                    {
                        base.Configure(reference);

                        list = new List<StatisticValue>();

                        Update = new UpdateRequest();
                        Update.OnResult.Add(UpdateCallback);
                    }

                    private void UpdateCallback(UpdatePlayerStatisticsResult result)
                    {
                        var request = result.Request as UpdatePlayerStatisticsRequest;

                        if (request.Statistics != null && request.Statistics.Count > 0)
                        {
                            for (int i = 0; i < request.Statistics.Count; i++)
                                Set(request.Statistics[i].StatisticName, request.Statistics[i].Value);
                        }
                    }

                    public delegate void SetDelegate(string name, int value);
                    public event SetDelegate OnSet;
                    public virtual void Set(string name, int value)
                    {
                        var element = Find(name);

                        if (element == null)
                        {
                            element = new StatisticValue()
                            {
                                StatisticName = name,
                                Value = value,
                                Version = 0,
                            };

                            list.Add(element);
                        }
                        else
                        {
                            element.Value = value;
                        }

                        OnSet?.Invoke(name, value);

                        InvokeUpdate();
                    }

                    public virtual int Evalute(string name) => Evalute(name, 0);
                    public virtual int Evalute(string name, int defaultValue)
                    {
                        for (int i = 0; i < list.Count; i++)
                            if (list[i].StatisticName == name)
                                return list[i].Value;

                        return defaultValue;
                    }

                    protected override void Load(GetPlayerCombinedInfoResultPayload payload)
                    {
                        Load(payload.PlayerStatistics);
                    }
                    protected virtual void Load(IEnumerable<StatisticValue> collection)
                    {
                        Clear();

                        if (collection == null)
                        {

                        }
                        else
                        {
                            foreach (var element in collection)
                            {
                                var instance = new StatisticValue()
                                {
                                    StatisticName = element.StatisticName,
                                    Value = element.Value,
                                };

                                list.Add(instance);
                            }
                        }
                    }

                    public override void Clear()
                    {
                        list.Clear();
                    }
                }

                [SerializeField]
                protected InventoryProperty inventory;
                public InventoryProperty Inventory => inventory;
                [Serializable]
                public class InventoryProperty : Element
                {
                    [SerializeField]
                    protected List<ItemInstance> items;
                    public List<ItemInstance> Items => items;

                    public Dictionary<string, int> VirtualCurrency { get; protected set; }

                    public RetrieveRequest Retrieve { get; protected set; }
                    public class RetrieveRequest : Request<GetUserInventoryRequest, GetUserInventoryResult>
                    {
                        public override MethodDelegate Method => PlayFabClientAPI.GetUserInventory;

                        public virtual void Request()
                        {
                            var data = GenerateRequest();

                            Send(data);
                        }
                    }

                    public override void Configure(ProfileProperty reference)
                    {
                        base.Configure(reference);

                        items = new List<ItemInstance>();

                        VirtualCurrency = new Dictionary<string, int>();

                        Retrieve = new RetrieveRequest();
                        Retrieve.OnResult.Add(RetrieveCallback);
                    }

                    protected virtual void RetrieveCallback(GetUserInventoryResult result)
                    {
                        Load(result.Inventory, result.VirtualCurrency);

                        InvokeUpdate();
                    }

                    protected override void Load(GetPlayerCombinedInfoResultPayload payload)
                    {
                        Load(payload.UserInventory, payload.UserVirtualCurrency);
                    }
                    protected virtual void Load(IEnumerable<ItemInstance> items, Dictionary<string, int> virtualCurrency)
                    {
                        Clear();

                        this.items.AddRange(items);

                        if (virtualCurrency != null)
                            foreach (var currency in virtualCurrency)
                                VirtualCurrency.Add(currency.Key, currency.Value);
                    }

                    public override void Clear()
                    {
                        items.Clear();

                        VirtualCurrency.Clear();
                    }
                }

                public abstract class Element : Property
                {
                    public override void Init()
                    {
                        base.Init();

                        Profile.OnLoad += Load;
                    }

                    public event Action OnUpdate;
                    protected virtual void InvokeUpdate()
                    {
                        OnUpdate?.Invoke();
                    }

                    protected abstract void Load(GetPlayerCombinedInfoResultPayload payload);

                    public abstract void Clear();
                }

                [Serializable]
                public class Property : Core.Property<ProfileProperty>
                {
                    public ProfileProperty Profile => Reference;
                }

                public override void Configure(PlayerProperty reference)
                {
                    base.Configure(reference);

                    Register(statistics);
                    Register(displayName);
                    Register(inventory);
                }

                protected virtual void Register(Element element)
                {
                    element.OnUpdate += InvokeUpdate;

                    base.Register(this, element);
                }

                public virtual void Load(LoginResult result) => Load(result.InfoResultPayload);
                public delegate void LoadDelegate(GetPlayerCombinedInfoResultPayload payload);
                public event LoadDelegate OnLoad;
                protected virtual void Load(GetPlayerCombinedInfoResultPayload payload)
                {
                    ID = payload.AccountInfo.PlayFabId;

                    OnLoad?.Invoke(payload);

                    InvokeUpdate();
                }

                public event Action OnUpdate;
                protected virtual void InvokeUpdate()
                {
                    OnUpdate?.Invoke();
                }

                public virtual void Clear()
                {
                    ID = null;

                    displayName = null;

                    statistics.Clear();
                }
            }

            [SerializeField]
            protected LinkProperty link;
            public LinkProperty Link { get { return link; } }
            [Serializable]
            public class LinkProperty : Property
            {
                public FacebookRequest Facebook { get; protected set; }
                public class FacebookRequest : Request<LinkFacebookAccountRequest, LinkFacebookAccountResult>
                {
                    public override MethodDelegate Method => PlayFabClientAPI.LinkFacebookAccount;

                    public virtual void Request(string token)
                    {
                        var request = GenerateRequest();

                        request.AccessToken = token;

                        Send(request);
                    }
                }
                public override void Configure(PlayerProperty reference)
                {
                    base.Configure(reference);

                    Facebook = new FacebookRequest();
                    Register(Facebook);
                }

                protected virtual void Register<TRequest, TResult>(Request<TRequest, TResult> element)
                    where TRequest : PlayFabRequestCommon, new()
                    where TResult : PlayFabResultCommon
                {
                    element.OnResponse.Add(ResponseCallback);

                    element.OnResult.Add(ResultCallback);

                    element.OnError.Add(ErrorCallback);
                }

                #region Events
                public MoeEvent<PlayFabResultCommon, PlayFabError> OnResponse { get; protected set; }
                void ResponseCallback(PlayFabResultCommon result, PlayFabError error)
                {
                    OnResponse?.Invoke(result, error);
                }

                public MoeEvent<PlayFabResultCommon> OnResult { get; protected set; }
                void ResultCallback(PlayFabResultCommon result)
                {
                    OnResult.Invoke(result);
                }

                public MoeEvent<PlayFabError> OnError { get; protected set; }
                void ErrorCallback(PlayFabError error)
                {
                    OnError.Invoke(error);
                }
                #endregion

                public LinkProperty()
                {
                    OnResponse = new MoeEvent<PlayFabResultCommon, PlayFabError>();

                    OnResult = new MoeEvent<PlayFabResultCommon>();

                    OnError = new MoeEvent<PlayFabError>();
                }
            }

            public class Property : Core.Property<PlayerProperty>
            {
                public PlayerProperty Player => Reference;

                public PlayFabCore PlayFab => Player.PlayFab;
            }

            public ClearRequest Clear { get; protected set; }
            public class ClearRequest : CloudScriptRequest
            {
                public override string FunctionName => "ClearPlayer";

                public virtual void Request(string playfabID, string customID)
                {
                    var request = GenerateRequest();

                    request.FunctionParameter = new ParametersData(playfabID, customID);

                    Send(request);
                }

                struct ParametersData
                {
                    public string playfabID;
                    public string customID;

                    public ParametersData(string playfabID, string customID)
                    {
                        this.playfabID = playfabID;
                        this.customID = customID;
                    }
                }
            }

            public override void Configure(PlayFabCore reference)
            {
                base.Configure(reference);

                Register(this, profile);
                Register(this, link);

                Clear = new ClearRequest();
            }
        }
    }
}