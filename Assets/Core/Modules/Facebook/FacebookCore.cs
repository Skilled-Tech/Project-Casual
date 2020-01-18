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

namespace Game
{
	public class FacebookCore : Core.Module
	{
        public bool Active => FB.IsInitialized;

        public LoginProperty Login { get; protected set; }
        public class LoginProperty : Property
        {
            public bool Active => FB.IsLoggedIn && Facebook.HasAccessToken;

            public static readonly List<string> Permissions = new List<string>()
            {
                "public_profile"
            };

            public void Request()
            {
                FB.LogInWithReadPermissions(Permissions, ResultCallback);
            }

            public MoeEvent<ILoginResult> OnResult { get; protected set; } = new MoeEvent<ILoginResult>();
            void ResultCallback(ILoginResult result)
            {
                Facebook.AccessToken = result?.AccessToken;

                OnResult.Invoke(result);
            }
        }

        public AccessToken AccessToken { get; protected set; }
        public bool HasAccessToken => string.IsNullOrEmpty(AccessToken?.TokenString) == false;

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

        public MoeEvent OnActivate { get; protected set; } = new MoeEvent();
        private void InitCallback()
        {
            Debug.Log("Facebook Initiliazed");

#if UNITY_EDITOR
            FB.ActivateApp();
#endif

            OnActivate.Invoke();
        }

        private void HideCallback(bool isUnityShown)
        {

        }
    }
}