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

namespace Game
{
	public class ProceduresCore : Core.Module
	{
        [SerializeField]
        protected LoginProperty login;
        public LoginProperty Login { get { return login; } }
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
                    PlayFab.Login.CustomID.OnResponse += Callback;
                    PlayFab.Login.CustomID.Request(ID);

                    void Callback(LoginResult result, PlayFabError error)
                    {
                        PlayFab.Login.CustomID.OnResponse -= Callback;

                        if (error == null)
                        {
                            End();
                        }
                        else
                        {
                            InvokeError(error.ErrorMessage);
                        }
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

                    if(Core.Facebook.Active == false)
                    {
                        FacebookActivation();
                    }
                    else if(Core.Facebook.Login.Active == false)
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
                            PlayFabLogin(result.AccessToken.TokenString);
                    }
                }

                void PlayFabLogin(string token)
                {
                    PlayFab.Login.Facebook.OnResponse += PlayFabLoginCallback;
                    PlayFab.Login.Facebook.Request(token);

                    void PlayFabLoginCallback(LoginResult result, PlayFabError error)
                    {
                        PlayFab.Login.Facebook.OnResponse += PlayFabLoginCallback;

                        if (error == null)
                            End();
                        else
                            InvokeError(error.ErrorMessage);
                    }
                }
            }

            public Element Procedure => Find(Method);

            public List<Element> List { get; protected set; }

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
                    return LoginMethod.Facebook;
                }
                set
                {
                    //TODO

                    Debug.LogWarning("Setting Login Method: " + value + ", Please Implement");
                }
            }

            public abstract class Element : ProceduresCore.Element
            {
                public LoginProperty Login => Procedures.Login;

                public PlayFabCore PlayFab => Core.PlayFab;

                public abstract LoginMethod Method { get; }

                public virtual void Require()
                {
                    Core.UI.Popup.Show("Loggin In");

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
            }

            public override void Configure(ProceduresCore reference)
            {
                base.Configure(reference);

                List = new List<Element>();

                List.Add(customID);
                List.Add(facebook);

                for (int i = 0; i < List.Count; i++)
                    Register(List[i]);
            }

            public virtual void Register(Element element)
            {
                Register(Procedures, element);

                element.OnResponse += (string error)=> ResponseCallback(element, error);
            }

            public override void Init()
            {
                base.Init();

                if (auto) Procedure.Request();
            }

            #region Events
            public RestDelegates.ErrorCallback<string> OnResponse;
            private void ResponseCallback(Element element, string error)
            {
                if(error == null)
                {
                    Method = element.Method;
                }

                OnResponse?.Invoke(error);
            }
            #endregion
        }

        [SerializeField]
        protected UpdateDisplayNameProcedure updateDisplayName;
        public UpdateDisplayNameProcedure UpdateDisplayName { get { return updateDisplayName; } }
        [Serializable]
        public class UpdateDisplayNameProcedure : Element
        {
            public PlayFabCore PlayFab => Core.PlayFab;

            public PopupUI Popup => Core.UI.Popup;
            public TextInputUI TextInput => Core.UI.TextInput;

            public override void Start()
            {
                base.Start();

                if (Procedures.Login.IsComplete)
                    DisplayTextInput();
                else
                {
                    Popup.Show("Loggin In");

                    RelyOn(Procedures.Login.Procedure, LoginResponse);
                }
            }

            protected virtual void LoginResponse(string error)
            {
                if(error == null)
                {
                    Popup.Hide();

                    DisplayTextInput();
                }
                else
                {
                    InvokeError(error);
                }
            }

            protected virtual void DisplayTextInput()
            {
                TextInput.Show("Enter Display Name", TextInputCallback);
            }

            protected virtual void TextInputCallback(string text)
            {
                if(string.IsNullOrEmpty(text))
                {
                    InvokeError("Canceled");
                }
                else
                {
                    Popup.Show("Updating Profile Info");

                    PlayFab.Player.Info.UpdateDisplayName.OnResponse += UpdateDisplayNameResponseCallback;
                    PlayFab.Player.Info.UpdateDisplayName.Request(text);
                }
            }

            private void UpdateDisplayNameResponseCallback(UpdateUserTitleDisplayNameResult result, PlayFabError error)
            {
                PlayFab.Player.Info.UpdateDisplayName.OnResponse -= UpdateDisplayNameResponseCallback;

                if(error == null)
                {
                    TextInput.Hide();

                    Popup.Hide();

                    End();
                }
                else
                {
                    Popup.Show(error.ErrorMessage, "Okay");
                }
            }

            public override void InvokeError(string error)
            {
                base.InvokeError(error);

                TextInput.Hide();

                Core.UI.Popup.Show(error, "Okay");
            }
        }

        [Serializable]
        public abstract class Element : Property
        {
            public bool IsProcessing { get; protected set; }

            public virtual void Request()
            {
                if(IsProcessing)
                {

                }
                else
                {
                    Start();
                }
            }

            public event Action OnStart;
            public virtual void Start()
            {
                IsProcessing = true;

                OnStart?.Invoke();
            }
            
            public virtual void RelyOn(Element element, ResponseDelegate callback)
            {
                element.OnResponse += RelayCallback;

                if (element.IsProcessing)
                {
                    //Just simply wait for that requirement to finish
                }
                else
                {
                    element.Start();
                }

                void RelayCallback(string error)
                {
                    element.OnResponse -= RelayCallback;

                    callback(error);
                }
            }

            public event RestDelegates.ErrorCallback<string> OnError;
            public virtual void InvokeError(string error)
            {
                IsProcessing = false;

                Debug.LogError(error);

                OnError?.Invoke(error);

                Respond(error);
            }

            public event Action OnEnd;
            protected virtual void End()
            {
                IsProcessing = false;

                OnEnd?.Invoke();

                Respond(null);
            }

            public delegate void ResponseDelegate(string error);
            public event ResponseDelegate OnResponse;
            protected virtual void Respond(string error)
            {
                OnResponse?.Invoke(error);
            }
        }

        [Serializable]
        public class Property : Core.Property<ProceduresCore>
        {
            public ProceduresCore Procedures => Reference;
        }

        public PlayFabCore PlayFab => Core.PlayFab;

        public override void Configure(Core reference)
        {
            base.Configure(reference);

            Register(this, login);
            Register(this, updateDisplayName);
        }
    }

    public enum LoginMethod
    {
        CustomID, Facebook
    }
}