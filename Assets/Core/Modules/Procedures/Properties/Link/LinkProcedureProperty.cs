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
                    Core.Facebook.ActivateCallback += Callback;
                    Core.Facebook.Activate();

                    void Callback()
                    {
                        FacebookLogin();
                    }
                }

                void FacebookLogin()
                {
                    Core.Facebook.Login.OnResult += Callback;
                    Core.Facebook.Login.Request();

                    void Callback(Facebook.Unity.ILoginResult result)
                    {
                        Core.Facebook.Login.OnResult -= Callback;

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
                    PlayFab.Player.Link.Facebook.OnResponse += Callback;
                    PlayFab.Player.Link.Facebook.Request(token);

                    void Callback(PlayFabResultCommon result, PlayFabError error)
                    {
                        PlayFab.Player.Link.Facebook.OnResponse += Callback;

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

                    OnResponse += Callback;
                    Request();

                    void Callback(string error)
                    {
                        OnResponse -= Callback;

                        if (error == null)
                        {
                            Core.UI.Popup.Hide();
                        }
                        else
                        {
                            Core.UI.Popup.Show(error, "Okay");
                        }
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

                element.OnResponse += (string error) => ResponseCallback(element, error);
                element.OnEnd += () => EndCallback(element);
                element.OnError += ErrorCallback;
            }

            #region Events
            public event RestDelegates.Response<Element, string> OnResponse;
            private void ResponseCallback(Element element, string error)
            {
                OnResponse?.Invoke(element, error);
            }

            public event RestDelegates.Result<Element> OnEnd;
            void EndCallback(Element element)
            {
                OnEnd?.Invoke(element);
            }

            public event RestDelegates.Error<string> OnError;
            void ErrorCallback(string error)
            {
                OnError?.Invoke(error);
            }
            #endregion
        }
    }
}