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
	public partial class PlayFabCore : Core.Module
	{
        #region Login
        public bool IsLoggedIn => PlayFabClientAPI.IsClientLoggedIn();

        [SerializeField]
        protected LoginProperty login;
        public LoginProperty Login { get { return login; } }
        
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
        
        [SerializeField]
        protected PlayerProperty player;
        public PlayerProperty Player { get { return player; } }
        
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