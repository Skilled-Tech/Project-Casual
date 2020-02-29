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
    public partial class PlayFabCore
    {
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
                GetUserInventory = true,
                GetUserVirtualCurrency = true,
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
    }
}