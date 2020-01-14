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
	public partial class ProceduresCore : Core.Module
	{
        [SerializeField]
        protected LoginProperty login;
        public LoginProperty Login { get { return login; } }

        [SerializeField]
        protected LinkProperty link;
        public LinkProperty Link { get { return link; } }

        [SerializeField]
        protected FacebookProperty facebook;
        public FacebookProperty Facebook { get { return facebook; } }
        [Serializable]
        public class FacebookProperty : Property
        {
            [SerializeField]
            protected LoginElement login;
            public LoginElement Login { get { return login; } }
            [Serializable]
            public class LoginElement : Element
            {
                public bool Complete => Core.Facebook.Login.Active;

                public override void Start()
                {
                    base.Start();

                    if (Core.Facebook.Active == false)
                        Activation();
                    else if (Core.Facebook.Login.Active == false)
                        Login();
                    else
                        End();
                }

                void Activation()
                {
                    SingleSubscribe.Execute(Core.Facebook.OnActivate, Login);
                    Core.Facebook.Activate();
                }

                void Login()
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
                            End();
                    }
                }
            }

            public override void Configure(ProceduresCore reference)
            {
                base.Configure(reference);

                Register(Procedures, login);
            }
        }

        [SerializeField]
        protected UpdateDisplayNameProcedure updateDisplayName;
        public UpdateDisplayNameProcedure UpdateDisplayName { get { return updateDisplayName; } }
        [Serializable]
        public class UpdateDisplayNameProcedure : Element
        {
            public PlayFabCore PlayFab => Core.PlayFab;

            public TextInputUI TextInput => Core.UI.TextInput;

            public override void Start()
            {
                base.Start();

                DisplayTextInput();
            }

            void DisplayTextInput()
            {
                TextInput.Show("Enter Display Name", Callback);

                void Callback(string text)
                {
                    if (string.IsNullOrEmpty(text))
                    {
                        InvokeError("Canceled");
                    }
                    else
                    {
                        RequestNameChange(text);
                    }
                }
            }

            void RequestNameChange(string name)
            {
                Popup.Show("Updating Profile Info");

                SingleSubscribe.Execute(PlayFab.Player.Info.UpdateDisplayName.OnResponse, Callback);
                PlayFab.Player.Info.UpdateDisplayName.Request(name);

                void Callback(UpdateUserTitleDisplayNameResult result, PlayFabError error)
                {
                    if (error == null)
                        End();
                    else
                        Popup.Show(error.ErrorMessage, "Okay");
                }
            }

            protected override void End()
            {
                base.End();

                TextInput.Hide();
                Popup.Hide();
            }

            protected override void InvokeError(string error)
            {
                base.InvokeError(error);

                TextInput.Hide();

                Popup.Show(error, "Okay");
            }
        }

        [Serializable]
        public abstract class Element : Property
        {
            public bool IsProcessing { get; protected set; }

            public PopupUI Popup => Core.UI.Popup;

            public ChoiceUI Choice => Core.UI.Choice;

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
            
            public virtual void RelyOn(Element element, Action<string> callback)
            {
                SingleSubscribe.Execute(element.OnResponse, callback);

                element.Request();
            }

            public class ErrorEvent : UnityEvent<string> { }
            public ErrorEvent OnError { get; protected set; }
            protected virtual void InvokeError(string error)
            {
                Stop();

                Debug.LogError(error);

                OnError.Invoke(error);

                Respond(error);
            }

            protected virtual void Stop()
            {
                IsProcessing = false;
            }

            public class CancelEvent : UnityEvent { }
            public CancelEvent OnCancel { get; protected set; }
            protected virtual void Cancel()
            {
                Stop();

                OnCancel.Invoke();

                Respond(null);
            }

            public class EndEvent : UnityEvent { }
            public EndEvent OnEnd { get; protected set; }
            protected virtual void End()
            {
                Stop();

                OnEnd.Invoke();

                Respond(null);
            }

            public class ResponseEvent : UnityEvent<string> { }
            public ResponseEvent OnResponse { get; protected set; }
            protected virtual void Respond(string error)
            {
                OnResponse.Invoke(error);
            }

            public Element()
            {
                OnError = new ErrorEvent();

                OnEnd = new EndEvent();

                OnCancel = new CancelEvent();

                OnResponse = new ResponseEvent();
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
            Register(this, link);
            Register(this, facebook);
            Register(this, updateDisplayName);
        }
    }
}