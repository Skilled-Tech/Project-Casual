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

                public override LoginWithCustomIDRequest GenerateRequest()
                {
                    var request = base.GenerateRequest();

                    request.InfoRequestParameters = DefaultInfoRequestParameters;

                    return request;
                }

                public virtual void Request() => Request(SystemInfo.deviceUniqueIdentifier);
                public virtual void Request(string ID)
                {
                    Debug.Log("Custom ID Login: " + ID);

                    var request = GenerateRequest();

                    request.CreateAccount = true;
                    request.CustomId = ID;

                    Send(request);
                }
            }

            public static readonly GetPlayerCombinedInfoRequestParams DefaultInfoRequestParameters = new GetPlayerCombinedInfoRequestParams()
            {
                GetUserAccountInfo = true,
                GetPlayerProfile = true,
                GetPlayerStatistics = true,
            };

            public abstract class Request<TRequest> : Request<TRequest, LoginResult>
                where TRequest : class, new()
            {
                
            }

            public override void Configure(PlayFabCore reference)
            {
                base.Configure(reference);

                CustomID = new CustomIDRequest();
                Register(CustomID);
            }

            protected virtual void Register<TRequest>(Request<TRequest> request)
                where TRequest : class, new()
            {
                request.OnResponse += ResponseCallback;

                request.OnResult += ResultCallback;

                request.OnError += ErrorCallback;
            }

            #region Events
            public event CustomIDRequest.ResponseDelegate OnResponse;
            void ResponseCallback(LoginResult result, PlayFabError error)
            {
                OnResponse?.Invoke(result, error);
            }

            public event CustomIDRequest.ResultDelegate OnResult;
            void ResultCallback(LoginResult result)
            {
                PlayFab.Player.Profile.Update(result);

                OnResult?.Invoke(result);
            }

            public event CustomIDRequest.ErrorDelegate OnError;
            void ErrorCallback(PlayFabError error)
            {
                OnError?.Invoke(error);
            }
            #endregion

            public virtual void ShowRequirementPopup()
            {
                Core.UI.Popup.Show("You need to be logged in to perform this operation");
            }
        }

        public event Action OnLogout;
        public virtual void Logout()
        {
            PlayFabAuthenticationAPI.ForgetAllCredentials();

            Player.Profile.Clear();

            OnLogout?.Invoke();
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

                    public override GetLeaderboardRequest GenerateRequest()
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

                    public override GetLeaderboardAroundPlayerRequest GenerateRequest()
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

                public override void Configure(PlayFabCore reference)
                {
                    base.Configure(reference);

                    Get = new GetRequest();
                    GetAroundPlayer = new GetAroundPlayerRequest();
                }
            }

            public override void Configure(PlayFabCore reference)
            {
                base.Configure(reference);

                Register(PlayFab, leaderboards);
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

                public virtual void Update(UpdateUserTitleDisplayNameResult result)
                {
                    DisplayName = result.DisplayName;

                    Update();
                }
                public virtual void Update(LoginResult result)
                {
                    ID = result.PlayFabId;

                    DisplayName = result?.InfoResultPayload?.PlayerProfile?.DisplayName;

                    Update();
                }

                public event Action OnUpdate;
                protected virtual void Update()
                {
                    OnUpdate?.Invoke();
                }

                public virtual void Clear()
                {
                    ID = null;

                    DisplayName = null;
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

                public override void Configure(PlayFabCore reference)
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

                public override void Configure(PlayFabCore reference)
                {
                    base.Configure(reference);

                    UpdateDisplayName = new UpdateDisplayNameRequest();
                    UpdateDisplayName.OnResult += PlayFab.Player.Profile.Update;
                }
            }

            public override void Configure(PlayFabCore reference)
            {
                base.Configure(reference);

                Register(PlayFab, Profile);
                Register(PlayFab, statistics);
                Register(PlayFab, info);
            }
        }

        [Serializable]
        public class Property : Core.Property<PlayFabCore>
        {
            public PlayFabCore PlayFab => Reference;
        }

        public abstract class Request<TRequest, TResult>
            where TRequest : class, new()
            where TResult : class
        {
            public delegate void MethodDelegate(TRequest request, Action<TResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null);

            public abstract MethodDelegate Method { get; }

            public static void Send(MethodDelegate method, TRequest request, ResponseDelegate callback)
            {
                method.Invoke(request, ResultCallback, ErrorCallback);

                void ResultCallback(TResult result) => callback(result, null);

                void ErrorCallback(PlayFabError error) => callback(null, error);
            }

            public virtual TRequest GenerateRequest() => new TRequest();

            protected virtual void Send(TRequest request)
            {
                Method.Invoke(request, ResultCallback, ErrorCallback);
            }

            #region Events
            public delegate void ResultDelegate(TResult result);
            public event ResultDelegate OnResult;
            public virtual void ResultCallback(TResult result)
            {
                if (OnResult != null) OnResult(result);

                Respond(result, null);
            }

            public delegate void ErrorDelegate(PlayFabError error);
            public event ErrorDelegate OnError;
            public virtual void ErrorCallback(PlayFabError error)
            {
                if (OnError != null) OnError(error);

                Respond(null, error);
            }

            public delegate void ResponseDelegate(TResult result, PlayFabError error);
            public event ResponseDelegate OnResponse;
            public virtual void Respond(TResult result, PlayFabError error)
            {
                if (OnResponse != null) OnResponse(result, error);
            }
            #endregion
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
        public delegate void ResultCallback<TResult>(TResult result);

        public delegate void ErrorCallback<TError>(TError error);

        public delegate void ResponseCallback<TResult, TError>(TResult result, TError error);
    }
}