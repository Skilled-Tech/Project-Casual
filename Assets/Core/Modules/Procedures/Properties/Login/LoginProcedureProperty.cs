﻿using System;
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
        public class LoginProperty : Property
        {
            [SerializeField]
            protected bool auto;
            public bool Auto { get { return auto; } }

            public bool IsComplete => Core.PlayFab.IsLoggedIn;

            [SerializeField]
            protected CustomIDElement customID;
            public CustomIDElement CustomID { get { return customID; } }
            [Serializable]
            public class CustomIDElement : Element
            {
                [SerializeField]
                protected StringToggleValue _IDOverride;
                public StringToggleValue IDOverride { get { return _IDOverride; } }

                public override LoginMethod Method => LoginMethod.CustomID;

                public override void Start()
                {
                    base.Start();

                    var ID = SystemInfo.deviceUniqueIdentifier;

#if UNITY_EDITOR
                    ID = IDOverride.Evaluate(ID);
#endif

                    PlayFabLogin(ID);
                }

                void PlayFabLogin(string ID)
                {
                    SingleSubscribe.Execute(PlayFab.Login.CustomID.OnResponse, Callback);
                    PlayFab.Login.CustomID.Request(ID);

                    void Callback(LoginResult result, PlayFabError error)
                    {
                        if (error == null)
                            End();
                        else
                            InvokeError(error.ErrorMessage);
                    }
                }
            }

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
                        PlayFabLogin(Core.Facebook.Login.AccessToken.TokenString);
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
                            PlayFabLogin(result.AccessToken.TokenString);
                    }
                }

                void PlayFabLogin(string token)
                {
                    SingleSubscribe.Execute(PlayFab.Login.Facebook.OnResponse, Callback);
                    PlayFab.Login.Facebook.Request(token);

                    void Callback(LoginResult result, PlayFabError error)
                    {
                        if (error == null)
                            End();
                        else
                            InvokeError(error.ErrorMessage);
                    }
                }
            }

            public Element Procedure => Find(Method);

            public List<Element> List { get; protected set; }

            public Element this[LoginMethod method] => Find(method);

            public virtual Element Find(LoginMethod method)
            {
                for (int i = 0; i < List.Count; i++)
                    if (List[i].Method == method)
                        return List[i];

                return null;
            }

            public LoginMethod Method
            {
                get
                {
                    return (LoginMethod)PlayerPrefs.GetInt(MethodID, 0);
                }
                set
                {
                    PlayerPrefs.SetInt(MethodID, (int)value);
                }
            }
            public const string MethodID = "Login Method";

            public abstract class Element : ProceduresCore.Element
            {
                public LoginProperty Login => Procedures.Login;

                public PlayFabCore PlayFab => Core.PlayFab;

                public abstract LoginMethod Method { get; }

                public virtual void Require()
                {
                    Core.UI.Popup.Show("Loggin In");

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
            }

            public override void Configure(ProceduresCore reference)
            {
                base.Configure(reference);

                List = new List<Element>();

                Register(customID);
                Register(facebook);

                Procedures.Link.OnEnd.AddListener(LinkResultCallback);

                #if UNITY_EDITOR
                Method = LoginMethod.CustomID; //TODO Remove
                #endif
            }

            public virtual void Register(Element element)
            {
                List.Add(element);

                Register(Procedures, element);

                element.OnResponse.AddListener((string error) => ResponseCallback(element, error));
                element.OnEnd.AddListener(() => EndCallback(element));
                element.OnError.AddListener(ErrorCallback);
            }

            public override void Init()
            {
                base.Init();

                Debug.Log("Login Method: " + Method);

                if (auto) Request();
            }

            public virtual void Request() => Procedure.Request();
            public virtual void Require() => Procedure.Require();
            public virtual void Start() => Procedure.Start();

            #region Callbacks
            private void LinkResultCallback(LinkProperty.Element result)
            {
                Method = result.Method;
            }
            #endregion

            #region Events
            public class ResponseEvent : UnityEvent<Element, string> { }
            public ResponseEvent OnResponse { get; protected set; }
            void ResponseCallback(Element element, string error)
            {
                OnResponse.Invoke(element, error);
            }

            public class EndEvent : UnityEvent<Element> { }
            public EndEvent OnEnd { get; protected set; }
            void EndCallback(Element element)
            {
                Method = element.Method;

                OnEnd.Invoke(element);
            }

            public class ErrorEvent : UnityEvent<string> { }
            public ErrorEvent OnError { get; protected set; }
            void ErrorCallback(string error)
            {
                OnError.Invoke(error);
            }
            #endregion

            public LoginProperty()
            {
                OnResponse = new ResponseEvent();

                OnEnd = new EndEvent();

                OnError = new ErrorEvent();
            }
        }
    }

    public enum LoginMethod
    {
        CustomID, Facebook
    }
}