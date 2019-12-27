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

            public TRequest GenerateRequest() => new TRequest();

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

        public class Property : Core.Property<PlayFabCore>
        {
            public PlayFabCore PlayFab => Reference;
        }

        [SerializeField]
        protected LoginProperty login;
        public LoginProperty Login { get { return login; } }
        [Serializable]
        public class LoginProperty : Property
        {
            public CustomIDRequest CustomID { get; protected set; }
            public class CustomIDRequest : Request<LoginWithCustomIDRequest, LoginResult>
            {
                public override MethodDelegate Method => PlayFabClientAPI.LoginWithCustomID;

                public virtual void Request()
                {
                    var request = GenerateRequest();

                    request.CreateAccount = true;

                    Send(request);
                }
            }

            public override void Configure(PlayFabCore reference)
            {
                base.Configure(reference);

                CustomID = new CustomIDRequest();
            }
        }

        public override void Configure(Core reference)
        {
            base.Configure(reference);

            Register(this, login);
        }
    }
}