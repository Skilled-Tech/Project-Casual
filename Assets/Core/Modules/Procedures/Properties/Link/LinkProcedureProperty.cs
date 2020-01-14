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

                    if (Core.Facebook.Active == false)
                    {
                        FacebookActivation();
                    }
                    else if (Core.Facebook.Login.Active == false)
                    {
                        FacebookLogin();
                    }
                    else
                    {
                        PlayFabLink(Core.Facebook.Login.AccessToken.TokenString);
                    }
                }

                void FacebookActivation()
                {
                    SingleSubscribe.Execute(Core.Facebook.OnActivate, Callback);
                    Core.Facebook.Activate();

                    void Callback()
                    {
                        FacebookLogin();
                    }
                }

                void FacebookLogin()
                {
                    SingleSubscribe.Execute(Core.Facebook.Login.OnResult, Callback);
                    Core.Facebook.Login.Request();

                    void Callback(Facebook.Unity.ILoginResult result)
                    {
                        if (result == null) //No Response
                            InvokeError("No Response Recieved");
                        else if (result.Cancelled) //Canceled
                            InvokeError("Login Canceled");
                        else if (string.IsNullOrEmpty(result.Error) == false) //Error
                            InvokeError(result.Error);
                        else
                            PlayFabLink(result.AccessToken.TokenString);
                    }
                }

                void PlayFabLink(string token)
                {
                    SingleSubscribe.Execute(PlayFab.Player.Link.Facebook.OnResponse, Callback);
                    PlayFab.Player.Link.Facebook.Request(token);

                    void Callback(PlayFabResultCommon result, PlayFabError error)
                    {
                        if (error == null)
                            End();
                        else
                        {
                            if(error.Error == PlayFabErrorCode.LinkedAccountAlreadyClaimed) //Login instead if the account is already claimed
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
                    Core.UI.Popup.Show("Linking");

                    SingleSubscribe.Execute(OnResponse, Callback);
                    Request();

                    void Callback(string error)
                    {
                        if (error == null)
                            Core.UI.Popup.Hide();
                        else
                            Core.UI.Popup.Show(error, "Okay");
                    }
                }

                protected virtual void LoginToExistantAccount()
                {
                    PlayFab.Logout();

                    RelyOn(Procedures.Login[Method], Callback);

                    void Callback(string error)
                    {
                        if (error == null)
                            End();
                        else
                            InvokeError(error);
                    }
                }
            }

            public List<Element> List { get; protected set; }

            public Element this[LoginMethod method] => Find(method);

            public virtual Element Find(LoginMethod method)
            {
                for (int i = 0; i < List.Count; i++)
                    if (List[i].Method == method)
                        return List[i];

                return null;
            }

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