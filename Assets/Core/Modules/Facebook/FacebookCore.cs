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

using Facebook;
using Facebook.Unity;

using UnityEngine.Events;

namespace Game
{
	public class FacebookCore : Core.Module
	{
        public bool Active => FB.IsInitialized;

        public LoginProperty Login { get; protected set; }
        public class LoginProperty : Property
        {
            public bool Active => FB.IsLoggedIn;

            public AccessToken AccessToken => AccessToken.CurrentAccessToken;

            public static readonly List<string> Permissions = new List<string>()
            {
                "public_profile"
            };

            public void Request()
            {
                FB.LogInWithReadPermissions(Permissions, ResultCallback);
            }

            public class ResultEvent : UnityEvent<ILoginResult> { }
            public ResultEvent OnResult { get; protected set; } = new ResultEvent();
            void ResultCallback(ILoginResult result)
            {
                OnResult.Invoke(result);
            }
        }

        [Serializable]
        public class Property : Core.Property<FacebookCore>
        {
            public FacebookCore Facebook => Reference;
        }

        public override void Configure(Core reference)
        {
            base.Configure(reference);

            Login = new LoginProperty();
            Register(this, Login);
        }

        public void Activate()
        {
            FB.Init(InitCallback, HideCallback);
        }

        public class ActivateEvent : UnityEvent { }
        public ActivateEvent OnActivate { get; protected set; } = new ActivateEvent();
        private void InitCallback()
        {
            Debug.Log("Facebook Initiliazed");

            FB.ActivateApp();

            OnActivate.Invoke();
        }

        private void HideCallback(bool isUnityShown)
        {

        }
    }
}