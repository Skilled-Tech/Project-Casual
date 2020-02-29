﻿using System;
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
	public class PlayFabCore : Core.Module
	{
        #region Login
        public bool IsLoggedIn => PlayFabClientAPI.IsClientLoggedIn();

        [SerializeField]
        protected LoginProperty login;
        public LoginProperty Login { get { return login; } }
        [Serializable]
        public class LoginProperty : Property
        {
            public CustomIDRequest CustomID { get; protected set; }
            public class CustomIDRequest : Request<LoginWithCustomIDRequest>
            {
                public override MethodDelegate Method => PlayFabClientAPI.LoginWithCustomID;

                protected override void ApplyDefaults(ref LoginWithCustomIDRequest request)
                {
                    request.CreateAccount = true;
                    request.InfoRequestParameters = DefaultInfoRequestParameters;
                }

                public virtual void Request() => Request(SystemInfo.deviceUniqueIdentifier);
                public virtual void Request(string ID)
                {
                    var request = GenerateRequest();

                    request.CustomId = ID;

                    Send(request);
                }
            }

            public FacebookRequest Facebook { get; protected set; }
            public class FacebookRequest : Request<LoginWithFacebookRequest>
            {
                public override MethodDelegate Method => PlayFabClientAPI.LoginWithFacebook;

                protected override void ApplyDefaults(ref LoginWithFacebookRequest request)
                {
                    request.CreateAccount = true;
                    request.InfoRequestParameters = DefaultInfoRequestParameters;
                }

                public virtual void Request(string token)
                {
                    var request = GenerateRequest();

                    request.AccessToken = token;

                    Send(request);
                }
            }

            public GoogleRequest Google { get; protected set; }
            public class GoogleRequest : Request<LoginWithGoogleAccountRequest>
            {
                public override MethodDelegate Method => PlayFabClientAPI.LoginWithGoogleAccount;

                protected override void ApplyDefaults(ref LoginWithGoogleAccountRequest request)
                {
                    request.CreateAccount = true;
                    request.InfoRequestParameters = DefaultInfoRequestParameters;
                }

                public virtual void Request(string authCode)
                {
                    var request = GenerateRequest();

                    request.ServerAuthCode = authCode;

                    Send(request);
                }
            }

            public abstract class Request<TRequest> : Request<TRequest, LoginResult>
                where TRequest : PlayFabRequestCommon, new()
            {
                protected override TRequest GenerateRequest()
                {
                    var request = base.GenerateRequest();

                    ApplyDefaults(ref request);

                    return request;
                }

                protected abstract void ApplyDefaults(ref TRequest request);
            }

            public static readonly GetPlayerCombinedInfoRequestParams DefaultInfoRequestParameters = new GetPlayerCombinedInfoRequestParams()
            {
                GetUserAccountInfo = true,
                GetPlayerProfile = true,
                GetPlayerStatistics = true,
            };

            public override void Configure(PlayFabCore reference)
            {
                base.Configure(reference);

                CustomID = new CustomIDRequest();
                Register(CustomID);

                Facebook = new FacebookRequest();
                Register(Facebook);

                Google = new GoogleRequest();
                Register(Google);
            }

            protected virtual void Register<TRequest>(Request<TRequest> element)
                where TRequest : PlayFabRequestCommon, new()
            {
                element.OnResponse.Add(ResponseCallback);

                element.OnResult.Add(ResultCallback);

                element.OnError.Add(ErrorCallback);
            }

            public virtual void ShowRequirementPopup()
            {
                Core.UI.Popup.Show("You need to be logged in to perform this operation", "Okay");
            }

            #region Events
            public MoeEvent<LoginResult, PlayFabError> OnResponse { get; protected set; }
            void ResponseCallback(LoginResult result, PlayFabError error)
            {
                OnResponse.Invoke(result, error);
            }

            public MoeEvent<LoginResult> OnResult { get; protected set; }
            void ResultCallback(LoginResult result)
            {
                PlayFab.Player.Profile.Update(result);

                OnResult.Invoke(result);
            }

            public MoeEvent<PlayFabError> OnError { get; protected set; }
            void ErrorCallback(PlayFabError error)
            {
                OnError?.Invoke(error);
            }
            #endregion

            public LoginProperty()
            {
                OnResponse = new MoeEvent<LoginResult, PlayFabError>();

                OnResult = new MoeEvent<LoginResult>();

                OnError = new MoeEvent<PlayFabError>();
            }
        }

        public MoeEvent OnLogout { get; protected set; } = new MoeEvent();
        public virtual void Logout()
        {
            PlayFabAuthenticationAPI.ForgetAllCredentials();

            Player.Profile.Clear();

            OnLogout.Invoke();
        }
        #endregion

        [SerializeField]
        protected TitleProperty title;
        public TitleProperty Title { get { return title; } }
        [Serializable]
        public class TitleProperty : Property
        {
            [SerializeField]
            protected LeaderboardsProperty leaderboards;
            public LeaderboardsProperty Leaderboards { get { return leaderboards; } }
            [Serializable]
            public class LeaderboardsProperty : Property
            {
                public GetRequest Get { get; protected set; }
                public class GetRequest : Request<GetLeaderboardRequest, GetLeaderboardResult>
                {
                    public override MethodDelegate Method => PlayFabClientAPI.GetLeaderboard;

                    protected override GetLeaderboardRequest GenerateRequest()
                    {
                        var request = base.GenerateRequest();

                        request.ProfileConstraints = DefaultPlayerProfileConstraints;

                        return request;
                    }

                    public virtual void Request(string statistic) => Request(statistic, 5);
                    public virtual void Request(string statistic, int count)
                    {
                        var request = GenerateRequest();

                        request.StatisticName = statistic;
                        request.MaxResultsCount = count;

                        Send(request);
                    }
                }

                public GetAroundPlayerRequest GetAroundPlayer { get; protected set; }
                public class GetAroundPlayerRequest : Request<GetLeaderboardAroundPlayerRequest, GetLeaderboardAroundPlayerResult>
                {
                    public override MethodDelegate Method => PlayFabClientAPI.GetLeaderboardAroundPlayer;

                    protected override GetLeaderboardAroundPlayerRequest GenerateRequest()
                    {
                        var request = base.GenerateRequest();

                        request.ProfileConstraints = DefaultPlayerProfileConstraints;

                        return request;
                    }

                    public virtual void Request(string statistic, int count)
                    {
                        var request = GenerateRequest();

                        request.StatisticName = statistic;
                        request.MaxResultsCount = count;

                        Send(request);
                    }
                }

                public static readonly PlayerProfileViewConstraints DefaultPlayerProfileConstraints = new PlayerProfileViewConstraints()
                {
                    ShowDisplayName = true,
                    ShowLocations = true,
                };

                public override void Configure(TitleProperty reference)
                {
                    base.Configure(reference);

                    Get = new GetRequest();
                    GetAroundPlayer = new GetAroundPlayerRequest();
                }
            }

            [SerializeField]
            protected CatalogProperty catalog;
            public CatalogProperty Catalog { get { return catalog; } }
            [Serializable]
            public class CatalogProperty : Property
            {
                [SerializeField]
                protected string name = "Default";
                public string Name { get { return name; } }

                public GetRequest Get { get; protected set; }
                public class GetRequest : Request<GetCatalogItemsRequest, GetCatalogItemsResult>
                {
                    public override MethodDelegate Method => PlayFabClientAPI.GetCatalogItems;

                    public CatalogProperty Catalog { get; protected set; }

                    public virtual void Request()
                    {
                        var request = GenerateRequest();

                        request.CatalogVersion = Catalog.Name;

                        Send(request);
                    }

                    public GetRequest(CatalogProperty catalog)
                    {
                        this.Catalog = catalog;
                    }
                }

                public override void Configure(TitleProperty reference)
                {
                    base.Configure(reference);

                    Get = new GetRequest(this);
                }
            }

            public DataRequest Data { get; protected set; }
            public class DataRequest : Request<GetTitleDataRequest, GetTitleDataResult>
            {
                public override MethodDelegate Method => PlayFabClientAPI.GetTitleData;

                public virtual void Request() => Query(null);

                public virtual void Request(params string[] keys)
                {
                    var list = keys.ToList();

                    Query(list);
                }

                public virtual void Request(IList<string> keys)
                {
                    var list = keys.ToList();

                    Query(list);
                }

                protected virtual void Query(List<string> keys)
                {
                    var request = GenerateRequest();

                    request.Keys = keys;

                    Send(request);
                }
            }

            public NewsRequest News { get; protected set; }
            public class NewsRequest : Request<GetTitleNewsRequest, GetTitleNewsResult>
            {
                public override MethodDelegate Method => PlayFabClientAPI.GetTitleNews;

                public virtual void Request() => Request(10);
                public virtual void Request(int count)
                {
                    var request = GenerateRequest();

                    request.Count = count;

                    Send(request);
                }
            }

            public class Property : Core.Property<TitleProperty>
            {
                public TitleProperty Title => Reference;

                public PlayFabCore PlayFab => Title.PlayFab;
            }

            public override void Configure(PlayFabCore reference)
            {
                base.Configure(reference);

                Register(this, leaderboards);
                Register(this, catalog);

                Data = new DataRequest();
                News = new NewsRequest();
            }
        }

        [SerializeField]
        protected PlayerProperty player;
        public PlayerProperty Player { get { return player; } }
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

                public string DisplayName { get; protected set; }
                public bool HasDisplayName => String.IsNullOrEmpty(DisplayName) == false;

                public StatisticsProperty Statistics { get; protected set; }
                [Serializable]
                public class StatisticsProperty : Property
                {
                    public List<StatisticValue> List { get; protected set; }

                    public virtual bool Contains(string name)
                    {
                        for (int i = 0; i < List.Count; i++)
                            if (List[i].StatisticName == name)
                                return true;

                        return false;
                    }
                    public virtual StatisticValue Find(string name)
                    {
                        for (int i = 0; i < List.Count; i++)
                            if (List[i].StatisticName == name)
                                return List[i];

                        return null;
                    }

                    public override void Configure(ProfileProperty reference)
                    {
                        base.Configure(reference);

                        List = new List<StatisticValue>();
                    }

                    public override void Init()
                    {
                        base.Init();

                        Profile.Player.Statistics.Update.OnResult.Add(StatisticUpdateCallback);
                    }

                    private void StatisticUpdateCallback(UpdatePlayerStatisticsResult result)
                    {
                        var request = result.Request as UpdatePlayerStatisticsRequest;

                        if (request.Statistics != null && request.Statistics.Count > 0)
                        {
                            for (int i = 0; i < request.Statistics.Count; i++)
                                Set(request.Statistics[i].StatisticName, request.Statistics[i].Value);

                            InvokeUpdate();
                        }
                    }

                    public virtual int Evalute(string name) => Evalute(name, 0);
                    public virtual int Evalute(string name, int defaultValue)
                    {
                        for (int i = 0; i < List.Count; i++)
                            if (List[i].StatisticName == name)
                                return List[i].Value;

                        return defaultValue;
                    }

                    public virtual void Update(IEnumerable<StatisticValue> source)
                    {
                        foreach (var element in source)
                            Set(element.StatisticName, element.Value);

                        InvokeUpdate();
                    }

                    public delegate void SetDelegate(string name, int value);
                    public event SetDelegate OnSet;
                    protected virtual void Set(string name, int value)
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

                            List.Add(element);
                        }
                        else
                        {
                            element.Value = value;
                        }

                        OnSet?.Invoke(name, value);
                    }

                    public event Action OnUpdate;
                    protected virtual void InvokeUpdate()
                    {
                        OnUpdate?.Invoke();
                    }

                    public virtual void Clear()
                    {
                        List.Clear();
                    }
                }

                public class Property : Core.Property<ProfileProperty>
                {
                    public ProfileProperty Profile => Reference;
                }

                public override void Configure(PlayerProperty reference)
                {
                    base.Configure(reference);

                    Statistics = new StatisticsProperty();
                    Register(this, Statistics);
                    Statistics.OnUpdate += InvokeUpdate;
                }

                public virtual void Update(UpdateUserTitleDisplayNameResult result)
                {
                    DisplayName = result.DisplayName;

                    InvokeUpdate();
                }
                public virtual void Update(LoginResult result) => Update(result.InfoResultPayload);
                public virtual void Update(GetPlayerCombinedInfoResultPayload payload)
                {
                    ID = payload.AccountInfo.PlayFabId;

                    DisplayName = payload?.PlayerProfile?.DisplayName;

                    Statistics.Update(payload.PlayerStatistics);

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

                    DisplayName = null;

                    Statistics.Clear();
                }
            }

            [SerializeField]
            protected StatisticsProperty statistics;
            public StatisticsProperty Statistics { get { return statistics; } }
            [Serializable]
            public class StatisticsProperty : Property
            {
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

                public override void Configure(PlayerProperty reference)
                {
                    base.Configure(reference);

                    Update = new UpdateRequest();
                }
            }

            [SerializeField]
            protected InfoProperty info;
            public InfoProperty Info { get { return info; } }
            [Serializable]
            public class InfoProperty : Property
            {
                public UpdateDisplayNameRequest UpdateDisplayName { get; protected set; }
                public class UpdateDisplayNameRequest : Request<UpdateUserTitleDisplayNameRequest, UpdateUserTitleDisplayNameResult>
                {
                    public override MethodDelegate Method => PlayFabClientAPI.UpdateUserTitleDisplayName;

                    public virtual void Request(string desiredName)
                    {
                        var request = GenerateRequest();

                        request.DisplayName = desiredName;

                        Send(request);
                    }
                }

                public override void Configure(PlayerProperty reference)
                {
                    base.Configure(reference);

                    UpdateDisplayName = new UpdateDisplayNameRequest();
                    UpdateDisplayName.OnResult.Add(PlayFab.Player.Profile.Update);
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
                Register(this, statistics);
                Register(this, info);
                Register(this, link);

                Clear = new ClearRequest();
            }
        }

        [Serializable]
        public class Property : Core.Property<PlayFabCore>
        {
            public PlayFabCore PlayFab => Reference;
        }

        public abstract class CloudScriptRequest : Request<ExecuteCloudScriptRequest, ExecuteCloudScriptResult>
        {
            public override MethodDelegate Method => PlayFabClientAPI.ExecuteCloudScript;

            public abstract string FunctionName { get; }

            protected override ExecuteCloudScriptRequest GenerateRequest()
            {
                var request = base.GenerateRequest();

                request.FunctionName = FunctionName;

                return request;
            }
        }
        public abstract class Request<TRequest, TResult>
            where TRequest : PlayFabRequestCommon, new()
            where TResult : PlayFabResultCommon
        {
            public delegate void MethodDelegate(TRequest request, Action<TResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null);

            public abstract MethodDelegate Method { get; }

            public static void Send(MethodDelegate method, TRequest request, RestDelegates.Response<TResult, PlayFabError> callback)
            {
                method.Invoke(request, ResultCallback, ErrorCallback);

                void ResultCallback(TResult result) => callback(result, null);

                void ErrorCallback(PlayFabError error) => callback(null, error);
            }

            protected virtual TRequest GenerateRequest() => new TRequest();

            protected virtual void Send(TRequest request)
            {
                Method.Invoke(request, ResultCallback, ErrorCallback);
            }

            #region Events
            public MoeEvent<TResult> OnResult { get; protected set; }
            public virtual void ResultCallback(TResult result)
            {
                OnResult.Invoke(result);

                Respond(result, null);
            }

            public MoeEvent<PlayFabError> OnError { get; protected set; }
            public virtual void ErrorCallback(PlayFabError error)
            {
                Debug.LogError(error.GenerateErrorReport());

                OnError.Invoke(error);

                Respond(null, error);
            }

            public MoeEvent<TResult, PlayFabError> OnResponse { get; protected set; }
            public virtual void Respond(TResult result, PlayFabError error)
            {
                OnResponse.Invoke(result, error);
            }
            #endregion

            public Request()
            {
                OnResult = new MoeEvent<TResult>();

                OnError = new MoeEvent<PlayFabError>();

                OnResponse = new MoeEvent<TResult, PlayFabError>();
            }
        }

        public override void Configure(Core reference)
        {
            base.Configure(reference);

            Register(this, login);
            Register(this, title);
            Register(this, player);
        }
    }

    public class RestDelegates
    {
        public delegate void Result<TResult>(TResult result);

        public delegate void Error<TError>(TError error);

        public delegate void Response<TResult, TError>(TResult result, TError error);
    }
}