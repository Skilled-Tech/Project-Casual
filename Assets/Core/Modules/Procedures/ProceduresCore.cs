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
        protected LoginProcedure login;
        public LoginProcedure Login { get { return login; } }
        [Serializable]
        public class LoginProcedure : SingularElement
        {
            [SerializeField]
            protected bool auto;
            public bool Auto { get { return auto; } }

            [SerializeField]
            protected StringToggleValue _IDOverride;
            public StringToggleValue IDOverride { get { return _IDOverride; } }

            public override bool IsComplete => PlayFab.IsLoggedIn;

            public PlayFabCore PlayFab => Core.PlayFab;

            public override void Start()
            {
                base.Start();

                var ID = SystemInfo.deviceUniqueIdentifier;

#if UNITY_EDITOR
                ID = IDOverride.Evaluate(ID);
#endif

                PlayFab.Login.OnResponse += ResponseCallback;
                PlayFab.Login.CustomID.Request(ID);
            }

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

            private void ResponseCallback(LoginResult result, PlayFabError error)
            {
                PlayFab.Login.OnResponse -= ResponseCallback;

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

                    RelyOn(Procedures.Login, LoginResponse);
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
        public abstract class SingularElement : Element
        {
            public abstract bool IsComplete { get; }
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

        public override void Init()
        {
            base.Init();

            if(login.Auto) login.Start();
        }
    }
}