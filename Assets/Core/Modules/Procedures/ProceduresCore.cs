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
        protected FacebookProperty facebook;
        public FacebookProperty Facebook { get { return facebook; } }
        [Serializable]
        public class FacebookProperty : Property
        {
            [SerializeField]
            protected LoginElement login;
            public LoginElement Login { get { return login; } }
            [Serializable]
            public class LoginElement : Procedure
            {
                public bool Complete => Core.Facebook.Login.Active;

                public override void Start()
                {
                    base.Start();

                    Procedures.StartCoroutine(Procedure());

                    IEnumerator Procedure()
                    {
                        while(true)
                        {
                            if (Core.UI.Popup.Element.Visible)
                            {
                                if (Core.UI.Popup.Element.Transition.Value == 1f)
                                    break;
                            }
                            else
                                break;

                            yield return new WaitForEndOfFrame();
                        }

                        if (Core.Facebook.Active == false)
                            Activation();
                        else if (Core.Facebook.Login.Active == false)
                            Login();
                        else
                            End();
                    }
                }

                void Activation()
                {
                    Core.Facebook.OnActivate.Enque(Callback);
                    Core.Facebook.Activate();

                    void Callback() => Login();
                }

                void Login()
                {
                    Core.Facebook.Login.OnResult.Enque(Callback);
                    Core.Facebook.Login.Request();

                    void Callback(Facebook.Unity.ILoginResult result)
                    {
                        if (result == null) //No Response
                            InvokeError("No Response Recieved");
                        else if (result.Cancelled) //Canceled
                            Cancel();
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
        public class UpdateDisplayNameProcedure : Procedure
        {
            public PlayFabCore PlayFab => Core.PlayFab;

            public TextInputUI TextInput => Core.UI.TextInput;

            public virtual void Require() => Require("Updating Display Name");

            public override void Start()
            {
                base.Start();

                DisplayInput();
            }

            void DisplayInput()
            {
                Popup.Hide();
                TextInput.Show("Enter Display Name", Callback);

                void Callback(TextInputUI.Response response)
                {
                    if (response.Success)
                        PlayFabRequest(response.Text);
                    else if (response.Canceled)
                        Cancel();
                    else
                    {
                        Debug.LogError("Unknown Condition Met");
                        InvokeError("Unknown Error");
                    }
                }
            }

            void PlayFabRequest(string name)
            {
                Popup.Lock("Updating Profile Info");

                PlayFab.Player.Info.UpdateDisplayName.OnResponse.Enque(Callback);
                PlayFab.Player.Info.UpdateDisplayName.Request(name);

                void Callback(UpdateUserTitleDisplayNameResult result, PlayFabError error)
                {
                    if (error == null)
                        End();
                    else
                    {
                        if(error.Error == PlayFabErrorCode.NameNotAvailable)
                            Popup.Show("Name Not Available, Please Try A Different Name", "Okay");
                        else
                            InvokeError(error.ErrorMessage);
                    }
                }
            }

            protected override void Stop()
            {
                base.Stop();

                Popup.Hide();

                TextInput.Hide();
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

    [Serializable]
    public abstract class Procedure : ProceduresCore.Property
    {
        public bool IsProcessing { get; protected set; }

        public PopupUI Popup => Core.UI.Popup;

        public ChoiceUI Choice => Core.UI.Choice;

        public virtual void Request()
        {
            if (IsProcessing)
            {

            }
            else
            {
                Start();
            }
        }

        public virtual void Require(string message)
        {
            Popup.Lock(message);

            OnResponse.Enque(Callback);
            Request();

            void Callback(Response response)
            {
                if (response.Success || response.Canceled)
                    Popup.Hide();
                else
                    Popup.Show(response.Error, "Okay");
            }
        }

        public virtual void RelyOn(Procedure element, Action<Response> action)
        {
            element.OnResponse.Enque(Callback);

            void Callback(Response response) => action(response);

            element.Request();
        }

        public event Action OnStart;
        public virtual void Start()
        {
            IsProcessing = true;

            OnStart?.Invoke();
        }
        
        protected virtual void InvokeError(string error)
        {
            Stop();

            Debug.LogError(error);

            OnError.Invoke(error);

            Respond(error, false);
        }
        public MoeEvent<string> OnError { get; protected set; }

        protected virtual void Stop()
        {
            IsProcessing = false;
        }
        
        protected virtual void Cancel()
        {
            Stop();

            OnCancel.Invoke();

            Respond(null, true);
        }
        public MoeEvent OnCancel { get; protected set; }
        
        protected virtual void End()
        {
            Stop();

            OnEnd.Invoke();

            Respond(null, false);
        }
        public MoeEvent OnEnd { get; protected set; }

        #region Response
        protected virtual void Respond(string error, bool canceled)
        {
            var response = new Response(error, canceled);

            Respond(response);
        }
        protected virtual void Respond(Response response)
        {
            OnResponse.Invoke(response);
        }

        public MoeEvent<Response> OnResponse { get; protected set; }

        public class Response
        {
            public bool Canceled { get; protected set; }

            public string Error { get; protected set; }
            public bool HasError => Error != null;

            public bool Success => Canceled == false && HasError == false;

            public Response(string error, bool canceled)
            {
                this.Canceled = canceled;

                this.Error = error;
            }
        }

        protected virtual void ApplyResponse(Response response)
        {
            if (response.HasError)
                InvokeError(response.Error);
            else if (response.Canceled)
                Cancel();
            else
                End();
        }
        #endregion

        public Procedure()
        {
            OnError = new MoeEvent<string>();

            OnEnd = new MoeEvent();

            OnCancel = new MoeEvent();

            OnResponse = new MoeEvent<Response>();
        }
    }
}