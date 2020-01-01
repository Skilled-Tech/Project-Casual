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

namespace Game
{
	public class PlayFabCore : Core.Module
	{
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
                OnResult?.Invoke(result);
            }

            public event CustomIDRequest.ErrorDelegate OnError;
            void ErrorCallback(PlayFabError error)
            {
                OnError?.Invoke(error);
            }
            #endregion
        }
        
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

                    public virtual void Request(string name)
                    {
                        var request = GenerateRequest();

                        request.ProfileConstraints = new PlayerProfileViewConstraints()
                        {
                            ShowDisplayName = true,
                            ShowLocations = true,
                        };

                        request.StatisticName = name;

                        Send(request);
                    }

                    public override void ResultCallback(GetLeaderboardResult result)
                    {
                        base.ResultCallback(result);
                    }
                }

                public override void Configure(PlayFabCore reference)
                {
                    base.Configure(reference);

                    Get = new GetRequest();
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
                }
            }

            public override void Configure(PlayFabCore reference)
            {
                base.Configure(reference);

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
                Response(result, null);

                if (OnResult != null) OnResult(result);
            }

            public delegate void ErrorDelegate(PlayFabError error);
            public event ErrorDelegate OnError;
            public virtual void ErrorCallback(PlayFabError error)
            {
                Response(null, error);

                if (OnError != null) OnError(error);
            }

            public delegate void ResponseDelegate(TResult result, PlayFabError error);
            public event ResponseDelegate OnResponse;
            public virtual void Response(TResult result, PlayFabError error)
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