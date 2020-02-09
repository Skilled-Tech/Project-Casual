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

using Google;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

namespace Game
{
	public class GoogleCore : Core.Module
    {
        public LoginProperty Login { get; protected set; }
        public class LoginProperty : Property
        {
            public bool Active => Google.Platform.IsAuthenticated();

            public void Request()
            {
                var builder = new PlayGamesClientConfiguration.Builder();

                builder.AddOauthScope("profile");
                builder.RequestServerAuthCode(false);

                var config = builder.Build();

                PlayGamesPlatform.InitializeInstance(config);

                PlayGamesPlatform.DebugLogEnabled = true;

                PlayGamesPlatform.Activate();

                Social.localUser.Authenticate(AuthenticateCallback);
            }

            public MoeEvent<bool, string> OnResult { get; protected set; } = new MoeEvent<bool, string>();
            void AuthenticateCallback(bool success, string error)
            {
                if (success)
                {
                    Debug.Log("Google Login Success: " + Google.AuthCode);
                }
                else
                {
                    Debug.LogError("Google Login Error: " + error);
                }

                OnResult.Invoke(success, error);
            }
        }

        [Serializable]
        public class Property : Core.Property<GoogleCore>
        {
            public GoogleCore Google => Reference;
        }

        public PlayGamesPlatform Platform => PlayGamesPlatform.Instance;

        public string AuthCode => Platform?.GetServerAuthCode();

        public override void Configure(Core reference)
        {
            base.Configure(reference);

            Login = new LoginProperty();
            Register(this, Login);
        }
    }
}