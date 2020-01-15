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

                    void Callback(string error)
                    {
                        if (error == null)
                            PlayFabLink();
                        else
                            InvokeError(error);
                    }
                }

                void PlayFabLink()
                {
                    SingleSubscribe.Execute(PlayFab.Player.Link.Facebook.OnResponse, Callback);
                    PlayFab.Player.Link.Facebook.Request(Core.Facebook.Login.AccessToken.TokenString);

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

            public abstract class Element : ProceduresCore.Element
            {
                public LinkProperty Link => Procedures.Link;

                public PlayFabCore PlayFab => Core.PlayFab;

                public abstract LoginMethod Method { get; }

                public virtual void Require()
                {
                    Popup.Show("Linking");

                    SingleSubscribe.Execute(OnResponse, Callback);
                    Request();

                    void Callback(string error)
                    {
                        if (error == null)
                        {
                            if (Popup.Element.Visible) Popup.Hide();
                        }
                        else
                        {
                            Popup.Show(error, "Okay");
                        }
                    }
                }
                
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

                    void Callback(string error)
                    {
                        if (error == null)
                        {
                            if(Core.PlayFab.Player.Profile.ID != previousData.playfabID)
                            {
                                Debug.Log("Clearing out old account");
                                Core.PlayFab.Player.Clear.Request(previousData.playfabID, previousData.customID);
                            }

                            End();
                        }
                        else
                            InvokeError(error);
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

                element.OnResponse.AddListener((string error) => ResponseCallback(element, error));
                element.OnEnd.AddListener(() => EndCallback(element));
                element.OnError.AddListener(ErrorCallback);
            }

            #region Events
            public class ResponseEvent : UnityEvent<Element, string> { }
            public ResponseEvent OnResponse { get; protected set; }
            private void ResponseCallback(Element element, string error)
            {
                OnResponse?.Invoke(element, error);
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

    public static class SingleSubscribe
    {
        public static void Execute(UnityEvent uEvent, Action callback)
        {
            uEvent.AddListener(Callback);

            void Callback()
            {
                uEvent.RemoveListener(Callback);

                callback();
            }
        }

        public static void Execute<T1>(UnityEvent<T1> uEvent, Action<T1> callback)
        {
            uEvent.AddListener(Callback);

            void Callback(T1 arg1)
            {
                uEvent.RemoveListener(Callback);

                callback(arg1);
            }
        }

        public static void Execute<T1, T2>(UnityEvent<T1, T2> uEvent, Action<T1, T2> callback)
        {
            uEvent.AddListener(Callback);

            void Callback(T1 arg1, T2 arg2)
            {
                uEvent.RemoveListener(Callback);

                callback(arg1, arg2);
            }
        }
    }
}