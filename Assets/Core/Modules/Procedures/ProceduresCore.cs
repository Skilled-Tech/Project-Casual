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
            
            public virtual void RelyOn(Element element, Action<string> callback)
            {
                SingleSubscribe.Execute(element.OnResponse, callback);

                element.Request();
            }

            public class ErrorEvent : UnityEvent<string> { }
            public ErrorEvent OnError { get; protected set; }
            public virtual void InvokeError(string error)
            {
                IsProcessing = false;

                Debug.LogError(error);

                OnError.Invoke(error);

                Respond(error);
            }

            public class EndEvent : UnityEvent { }
            public EndEvent OnEnd { get; protected set; }
            protected virtual void End()
            {
                IsProcessing = false;

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
            Register(this, updateDisplayName);
        }
    }
}