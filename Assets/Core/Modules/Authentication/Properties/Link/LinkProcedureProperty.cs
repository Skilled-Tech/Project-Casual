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

namespace Game
{
    public partial class AuthenticationCore
    {
        [Serializable]
        public class LinkProperty : Property
        {
            [SerializeField]
            protected FacebookElement facebook;
            public FacebookElement Facebook { get { return facebook; } }
            [Serializable]
            public class FacebookElement : Element
            {
                public override LoginMethod Method => LoginMethod.Facebook;

                public override void Start()
                {
                    base.Start();

                    if (PlayFab.IsLoggedIn == false)
                        InvokeError("Must Be Logged In To Link Account");
                    else if (Core.Facebook.Login.Complete)
                        PlayFabLink();
                    else
                        FacebookLogin();
                }

                void FacebookLogin()
                {
                    RelyOn(Core.Facebook.Login, Callback);

                    void Callback(Response response)
                    {
                        if (response.Success)
                            PlayFabLink();
                        else
                            ReplicateResponse(response);
                    }
                }

                void PlayFabLink()
                {
                    PlayFab.Player.Link.Facebook.OnResponse.Enque(Callback);
                    PlayFab.Player.Link.Facebook.Request(Core.Facebook.AccessToken.TokenString);

                    void Callback(PlayFabResultCommon result, PlayFabError error)
                    {
                        if (error == null)
                            End();
                        else
                        {
                            if (error.Error == PlayFabErrorCode.LinkedAccountAlreadyClaimed) //Login instead if the account is already claimed
                                LoginToExistantAccount();
                            else
                                InvokeError(error.ErrorMessage);
                        }
                    }
                }
            }

            public abstract class Element : Procedure
            {
                public PlayFabCore PlayFab => Core.PlayFab;

                public abstract LoginMethod Method { get; }

                public virtual void Require() => Require("Linking " + Method);

                protected virtual void PromtToExistantLogin()
                {
                    Popup.Hide();

                    Choice.Show("This Link was used for an Existing Account, Would You Like To Login to That Account ?", Callback);

                    void Callback(bool agree)
                    {
                        if (agree)
                            LoginToExistantAccount();
                        else
                            InvokeError("Link Canceled");
                    }
                }

                protected virtual void LoginToExistantAccount()
                {
                    Popup.Text = "Logging In";

                    var previousData = new
                    {
                        playfabID = Core.PlayFab.Player.Profile.ID,
                        customID = Authentication.Login.CustomID.ID.Value
                    };

                    PlayFab.Logout();

                    RelyOn(Authentication.Login[Method], Callback);

                    void Callback(Response response)
                    {
                        if (response.Success)
                        {
                            if (Core.PlayFab.Player.Profile.ID != previousData.playfabID)
                            {
                                Debug.Log("Clearing out old account");
                                Core.PlayFab.Player.Clear.Request(previousData.playfabID, previousData.customID);
                            }

                            End();
                        }
                        else
                            ReplicateResponse(response);
                    }
                }
            }

            public class Procedure : Core.Procedure<LinkProperty>
            {
                public LinkProperty Link => Reference;

                public AuthenticationCore Authentication => Link.Authentication;
            }

            #region List
            public List<Element> List { get; protected set; }

            public Element this[LoginMethod method] => Find(method);

            public virtual Element Find(LoginMethod method)
            {
                for (int i = 0; i < List.Count; i++)
                    if (List[i].Method == method)
                        return List[i];

                return null;
            }
            #endregion

            public override void Configure(AuthenticationCore reference)
            {
                base.Configure(reference);

                List = new List<Element>();

                Register(facebook);
            }

            public virtual void Register(Element element)
            {
                List.Add(element);

                base.Register(this, element);

                element.OnResponse.Add((Core.Procedure.Response response) => ResponseCallback(element, response));
            }

            #region Events
            public MoeEvent<Element, Core.Procedure.Response> OnResponse { get; protected set; }
            void ResponseCallback(Element element, Core.Procedure.Response response)
            {
                if (response.HasError)
                    ErrorCallback(element, response.Error);
                else if (response.Canceled)
                    CancelCallback(element);
                else if (response.Success)
                    EndCallback(element);

                OnResponse.Invoke(element, response);
            }

            public MoeEvent<Element> OnEnd { get; protected set; }
            void EndCallback(Element element)
            {
                OnEnd.Invoke(element);
            }

            public MoeEvent<Element> OnCancel { get; protected set; }
            void CancelCallback(Element element)
            {
                OnCancel.Invoke(element);
            }

            public MoeEvent<Element, string> OnError { get; protected set; }
            void ErrorCallback(Element element, string error)
            {
                OnError.Invoke(element, error);
            }
            #endregion

            public LinkProperty()
            {
                OnResponse = new MoeEvent<Element, Core.Procedure.Response>();

                OnEnd = new MoeEvent<Element>();

                OnCancel = new MoeEvent<Element>();

                OnError = new MoeEvent<Element, string>();
            }
        }
    }
}