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
	public partial class ProceduresCore : Core.Module
	{
        [SerializeField]
        protected LoginProperty login;
        public LoginProperty Login { get { return login; } }

        [SerializeField]
        protected LinkProperty link;
        public LinkProperty Link { get { return link; } }

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
                element.OnResponse += Callback;
                element.Request();

                void Callback(string error)
                {
                    element.OnResponse -= Callback;

                    callback(error);
                }
            }

            public event RestDelegates.Error<string> OnError;
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
            Register(this, link);
            Register(this, updateDisplayName);
        }
    }
}