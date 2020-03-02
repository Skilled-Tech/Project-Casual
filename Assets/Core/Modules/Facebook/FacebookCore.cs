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
        public bool Active => Activation.Complete;

        public ActivationProcedure Activation { get; protected set; }
        [Serializable]
        public class ActivationProcedure : Procedure
        {
            public bool Complete => FB.IsInitialized;

            public override void Request()
            {
                base.Request();

                if (Complete)
                    End();
                else
                    Action();
            }

            protected virtual void Action()
            {
                FB.Init(Callback, Facebook.HideCallback);

                void Callback()
                {
                    Debug.Log("Facebook Initiliazed");

#if UNITY_EDITOR
                    FB.ActivateApp();
#endif

                    End();
                }
            }
        }

        public LoginProcedure Login { get; protected set; }
        public class LoginProcedure : Procedure
        {
            public bool Complete => FB.IsLoggedIn && Facebook.HasAccessToken;

            public static readonly List<string> Permissions = new List<string>()
            {
                "public_profile"
            };

            public override void Request()
            {
                base.Request();

                Facebook.StartCoroutine(Procedure());

                IEnumerator Procedure()
                {
                    while (true)
                    {
                        if (Core.UI.Popup.Element.Visible)
                        {
                            if (Core.UI.Popup.Element.Transition.Value == 1f)
                                break;
                        }
                        else
                            break;

                        yield return new WaitForEndOfFrame();
                    }

                    if (Facebook.Active == false)
                        Activate();
                    else if (Facebook.Login.Complete == false)
                        Action();
                    else
                        End();
                }
            }

            protected virtual void Activate()
            {
                RelyOn(Facebook.Activation, Callback);

                void Callback(Response response) => ReplicateResponse(response, Action);
            }

            protected virtual void Action()
            {
                FB.LogInWithReadPermissions(Permissions, Callback);

                void Callback(ILoginResult result)
                {
                    if (result == null) //No Response
                        InvokeError("No Response Recieved");
                    else if (result.Cancelled) //Canceled
                        Cancel();
                    else if (string.IsNullOrEmpty(result.Error) == false) //Error
                        InvokeError(result.Error);
                    else
                    {
                        Facebook.AccessToken = result.AccessToken;

                        End();
                    }
                }
            }
        }

        public class Procedure : Core.Procedure<FacebookCore>
        {
            public FacebookCore Facebook => Reference;
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

            AccessToken = null;

            Activation = new ActivationProcedure();
            Login = new LoginProcedure();

            Register(this, Activation);
            Register(this, Login);
        }
        
        private void HideCallback(bool isUnityShown)
        {

        }
    }
}