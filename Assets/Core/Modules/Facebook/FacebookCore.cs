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
        public LoginProperty Login { get; protected set; }
        public class LoginProperty : Property
        {
            public static readonly List<string> Permissions = new List<string>()
            {
                "public_profile"
            };

            public void Request()
            {
                FB.LogInWithReadPermissions(Permissions, ResultCallback);
            }

            public event RestDelegates.ResultCallback<ILoginResult> OnResult;
            void ResultCallback(ILoginResult result)
            {
                Debug.Log("Access Token: " + result.AccessToken.ToString());
                Debug.Log("Access Token String: " + result.AccessToken.TokenString);

                Debug.Log(result.RawResult);

                OnResult?.Invoke(result);
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

        public override void Init()
        {
            base.Init();

            FB.Init(InitCallback, HideCallback);
        }

        private void InitCallback()
        {
            Debug.Log("Facebook Initiliazed");

            FB.ActivateApp();

            Login.Request();
        }

        private void HideCallback(bool isUnityShown)
        {

        }
    }
}