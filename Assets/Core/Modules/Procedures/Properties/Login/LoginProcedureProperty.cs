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
        public class LoginProperty : Property
        {
            [SerializeField]
            protected bool auto;
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

                public virtual LoginMethod Evaluate(LoginMethodToggleValue toggle)
                {
                    if (toggle.Enabled)
                        return toggle.Value;

                    return Value;
                }

                public LoginMethod Value
                {
                    get
                    {
#if UNITY_EDITOR
                        if (Override.Enabled) return Override.Value;
#endif

                        return (LoginMethod)Pref;
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

            public bool IsProcessing => Procedure.IsProcessing;

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

                public override LoginMethod Method => LoginMethod.CustomID;

                public override void Start()
                {
                    base.Start();

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
                public override LoginMethod Method => LoginMethod.Facebook;

                public override void Start()
                {
                    base.Start();

                    if (Procedures.Facebook.Login.Complete == false)
                        FacebookLogin();
                    else
                        PlayFabLogin();
                }

                void FacebookLogin()
                {
                    RelyOn(Procedures.Facebook.Login, Callback);

                    void Callback(Response response)
                    {
                        if (response.Success)
                            PlayFabLogin();
                        else
                            ApplyResponse(response);
                    }
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

            public Element this[LoginMethod method] => Find(method);

            public virtual Element Find(LoginMethod method)
            {
                for (int i = 0; i < List.Count; i++)
                    if (List[i].Method == method)
                        return List[i];

                return null;
            }
            #endregion

            public abstract class Element : Procedure
            {
                public LoginProperty Login => Procedures.Login;

                public PlayFabCore PlayFab => Core.PlayFab;

                public abstract LoginMethod Method { get; }

                public virtual void Require() => Require("Loggin In");
            }
            
            public Element Procedure => Find(Method.Value);

            public override void Configure(ProceduresCore reference)
            {
                base.Configure(reference);

                List = new List<Element>();

                Register(customID);
                Register(facebook);

                Procedures.Link.OnEnd.Add(LinkResultCallback);
            }

            public virtual void Register(Element element)
            {
                List.Add(element);

                Register(Procedures, element);

                element.OnResponse.Add((Procedure.Response response) => ResponseCallback(element, response));
                element.OnEnd.Add(() => EndCallback(element));
                element.OnError.Add(ErrorCallback);
            }

            public override void Init()
            {
                base.Init();

                Debug.Log("Login Method: " + Method.Value);

                if (auto) Request();
            }

            public virtual void Request() => Procedure.Request();
            public virtual void Require() => Procedure.Require();
            public virtual void Start() => Procedure.Start();

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
                OnResponse.Invoke(element, response);
            }

            public MoeEvent<Element> OnEnd { get; protected set; }
            void EndCallback(Element element)
            {
                Method.Value = element.Method;

                OnEnd.Invoke(element);
            }

            public MoeEvent<string> OnError { get; protected set; }
            void ErrorCallback(string error)
            {
                OnError.Invoke(error);
            }
#endregion

            public LoginProperty()
            {
                OnResponse = new MoeEvent<Element, Procedure.Response>();

                OnEnd = new MoeEvent<Element>();

                OnError = new MoeEvent<string>();
            }
        }
    }

    public enum LoginMethod
    {
        CustomID, Facebook
    }

    [Serializable]
    public class LoginMethodToggleValue : ToggleValue<LoginMethod> { }
}