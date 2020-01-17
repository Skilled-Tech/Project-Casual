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

using UnityEngine.Events;

namespace Game
{
    public partial class ProceduresCore
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
                    else if (Procedures.Facebook.Login.Complete)
                        PlayFabLink();
                    else
                        FacebookLogin();
                }

                void FacebookLogin()
                {
                    RelyOn(Procedures.Facebook.Login, Callback);

                    void Callback(Response response)
                    {
                        if (response.Success)
                            PlayFabLink();
                        else
                            ApplyResponse(response);
                    }
                }

                void PlayFabLink()
                {
                    SingleSubscribe.Execute(PlayFab.Player.Link.Facebook.OnResponse, Callback);
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
                public LinkProperty Link => Procedures.Link;

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
                        customID = Procedures.Login.CustomID.ID.Value
                    };

                    PlayFab.Logout();

                    RelyOn(Procedures.Login[Method], Callback);

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
                            ApplyResponse(response);
                    }
                }
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

            public override void Configure(ProceduresCore reference)
            {
                base.Configure(reference);

                List = new List<Element>();

                Register(facebook);
            }

            public virtual void Register(Element element)
            {
                List.Add(element);

                Register(Procedures, element);

                element.OnResponse.AddListener((Procedure.Response response) => ResponseCallback(element, response));
                element.OnEnd.AddListener(() => EndCallback(element));
                element.OnError.AddListener(ErrorCallback);
            }

            #region Events
            public class ResponseEvent : UnityEvent<Element, Procedure.Response> { }
            public ResponseEvent OnResponse { get; protected set; }
            private void ResponseCallback(Element element, Procedure.Response response)
            {
                OnResponse?.Invoke(element, response);
            }

            public class EndEvent : UnityEvent<Element> { }
            public EndEvent OnEnd { get; protected set; }
            void EndCallback(Element element)
            {
                OnEnd?.Invoke(element);
            }

            public class ErrorEvent : UnityEvent<string> { }
            public ErrorEvent OnError { get; protected set; }
            void ErrorCallback(string error)
            {
                OnError?.Invoke(error);
            }
            #endregion

            public LinkProperty()
            {
                OnResponse = new ResponseEvent();

                OnEnd = new EndEvent();

                OnError = new ErrorEvent();
            }
        }
    }
}