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
        public class LoginProperty : Property
        {
            [SerializeField]
            protected bool auto = true;
            public bool Auto { get { return auto; } }

            [SerializeField]
            protected MethodData method;
            public MethodData Method { get { return method; } }
            [Serializable]
            public class MethodData
            {
                [SerializeField]
                protected LoginMethodToggleValue _override;
                public LoginMethodToggleValue Override { get { return _override; } }

                public virtual AuthenticationMethod Evaluate(LoginMethodToggleValue toggle)
                {
                    if (toggle.Enabled)
                        return toggle.Value;

                    return Value;
                }

                public AuthenticationMethod Value
                {
                    get
                    {
#if UNITY_EDITOR
                        if (Override.Enabled) return Override.Value;
#endif

                        return (AuthenticationMethod)Pref;
                    }
                    set
                    {
                        Pref = (int)value;
                    }
                }

                protected virtual int Pref
                {
                    get => PlayerPrefs.GetInt(ID, 0);
                    set => PlayerPrefs.SetInt(ID, value);
                }

                public const string ID = "Login Method";
            }

            public bool IsComplete => Core.PlayFab.IsLoggedIn;

            public bool IsProcessing => Module.IsProcessing;

            [Header("Procedures")]
            [SerializeField]
            protected CustomIDElement customID;
            public CustomIDElement CustomID { get { return customID; } }
            [Serializable]
            public class CustomIDElement : Element
            {
                [SerializeField]
                protected IDData _ID;
                public IDData ID { get { return _ID; } }
                [Serializable]
                public class IDData
                {
                    [SerializeField]
                    protected StringToggleValue _override;
                    public StringToggleValue Override { get { return _override; } }

                    public virtual string Value
                    {
                        get
                        {
#if UNITY_EDITOR
                            if (Override.Enabled) return Override.Value;
#endif

                            if (PlayerPrefs.HasKey(PlayerPrefID))
                                return PlayerPrefs.GetString(PlayerPrefID);
                            else
                            {
                                Generate();

                                return Value;
                            }
                        }
                        set
                        {
                            PlayerPrefs.SetString(PlayerPrefID, value);
                        }
                    }

                    public const string PlayerPrefID = "Custom Login ID";

                    public virtual void Generate()
                    {
                        Debug.Log("Generating New Custom ID");

                        Value = Guid.NewGuid().ToString("N");
                    }
                }

                public override AuthenticationMethod Method => AuthenticationMethod.CustomID;

                public override void Request()
                {
                    base.Request();

                    PlayFabLogin();
                }

                void PlayFabLogin()
                {
                    PlayFab.Login.CustomID.OnResponse.Enque(Callback);
                    PlayFab.Login.CustomID.Request(ID.Value);

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
                public override AuthenticationMethod Method => AuthenticationMethod.Facebook;

                public override void Request()
                {
                    base.Request();

                    if (Core.Facebook.Login.Complete == false)
                        FacebookLogin();
                    else
                        PlayFabLogin();
                }

                void FacebookLogin()
                {
                    RelyOn(Core.Facebook.Login, Callback);

                    void Callback(Response response) => ReplicateResponse(response, PlayFabLogin);
                }

                void PlayFabLogin()
                {
                    PlayFab.Login.Facebook.OnResponse.Enque(Callback);
                    PlayFab.Login.Facebook.Request(Core.Facebook.AccessToken.TokenString);

                    void Callback(LoginResult result, PlayFabError error)
                    {
                        if (error == null)
                            End();
                        else
                            InvokeError(error.ErrorMessage);
                    }
                }
            }

            #region List
            public List<Element> List { get; protected set; }

            public Element this[AuthenticationMethod method] => Find(method);

            public virtual Element Find(AuthenticationMethod method)
            {
                for (int i = 0; i < List.Count; i++)
                    if (List[i].Method == method)
                        return List[i];

                return null;
            }
            #endregion

            public abstract class Element : Procedure
            {
                public PlayFabCore PlayFab => Core.PlayFab;

                public abstract AuthenticationMethod Method { get; }

                public virtual void Require() => Require("Loggin In");
            }

            public class Procedure : Core.Procedure<LoginProperty>
            {
                public LoginProperty Login => Reference;

                public AuthenticationCore Authentication => Login.Authentication;
            }

            public Element Module => Find(Method.Value);

            public override void Configure(AuthenticationCore reference)
            {
                base.Configure(reference);

                List = new List<Element>();

                Register(customID);
                Register(facebook);

                Authentication.Link.OnEnd.Add(LinkResultCallback);
            }

            public virtual void Register(Element element)
            {
                List.Add(element);

                base.Register(this, element);

                element.OnResponse.Add((Core.Procedure.Response response) => ResponseCallback(element, response));
            }

            public override void Init()
            {
                base.Init();

                Debug.Log("Login Method: " + Method.Value);

                if (auto) if (IsProcessing == false) Request();
            }

            public virtual void Require() => Module.Require();
            public virtual void Request() => Module.Request();

            #region Callbacks
            private void LinkResultCallback(LinkProperty.Element result)
            {
                Method.Value = result.Method;
            }
            #endregion

            #region Events
            public MoeEvent<Element, Procedure.Response> OnResponse { get; protected set; }
            void ResponseCallback(Element element, Procedure.Response response)
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
                Method.Value = element.Method;

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

            public LoginProperty()
            {
                OnResponse = new MoeEvent<Element, Core.Procedure.Response>();

                OnEnd = new MoeEvent<Element>();

                OnCancel = new MoeEvent<Element>();

                OnError = new MoeEvent<Element, string>();
            }
        }
    }

    [Serializable]
    public class LoginMethodToggleValue : ToggleValue<AuthenticationMethod> { }
}