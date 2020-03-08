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

using NaughtyAttributes;

namespace Game
{
	public class Core : MonoBehaviour
	{
        public static Core Instance { get; protected set; }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnLoad()
        {
            var prefab = Resources.Load(nameof(Core)) as GameObject;

            if(prefab == null)
            {
                Debug.LogWarning("No core prefab setup, need a prefab named: " + nameof(Core) + " in a resources folder");
            }
            else
            {
                var gameObject = Instantiate(prefab);

                gameObject.name = nameof(Core);
                DontDestroyOnLoad(gameObject);

                Instance = gameObject.GetComponent<Core>();
                Instance.Configure();
            }
        }
        
        public AudioCore Audio { get; protected set; }
        public ScenesCore Scenes { get; protected set; }
        public UICore UI { get; protected set; }
        public PlayerCore Player { get; protected set; }
        public PlayFabCore PlayFab { get; protected set; }
        public LeaderboardsCore Leaderboards { get; protected set; }
        public AuthenticationCore Authentication { get; protected set; }
        public CountriesCore Countries { get; protected set; }
        public FacebookCore Facebook { get; protected set; }
        public NewsCore News { get; protected set; }
        public UnityCore Unity { get; protected set; }
        public InternetCore Internet { get; protected set; }

        #region Modules
        public class Module<TReference> : MonoBehaviour, IReference<TReference>
        {
            public Core Core => Core.Instance;

            public TReference Reference { get; protected set; }

            protected virtual void Register<TThis>(TThis reference, IReference<TThis> module)
            {
                References.Configure(reference, module);

                OnInit += InitCallback;

                void InitCallback()
                {
                    OnInit -= Init;

                    References.Init(reference, module);
                }
            }

            public virtual void Configure(TReference reference)
            {
                this.Reference = reference;
            }

            protected event Action OnInit;
            public virtual void Init()
            {
                OnInit?.Invoke();
            }
        }
        public class Module : Module<Core>
        {

        }
        #endregion

        #region Property
        [Serializable]
        public class Property<TReference> : IReference<TReference>
        {
            public Core Core => Core.Instance;

            [NonSerialized]
            protected TReference Reference;

            protected virtual void Register<TThis>(TThis reference, IReference<TThis> module)
            {
                References.Configure(reference, module);

                OnInit += InitCallback;

                void InitCallback()
                {
                    OnInit -= Init;

                    References.Init(reference, module);
                }
            }

            public virtual void Configure(TReference reference)
            {
                this.Reference = reference;
            }

            protected event Action OnInit;
            public virtual void Init()
            {
                OnInit?.Invoke();
            }
        }
        [Serializable]
        public class Property : Module<Core>
        {

        }
        #endregion

        #region Procedure
        [Serializable]
        public abstract class Procedure
        {
            public bool IsProcessing { get; protected set; }

            public Core Core => Core.Instance;
            public PopupUI Popup => Core.UI.Popup;
            public ChoiceUI Choice => Core.UI.Choice;

            public virtual void Require(string message)
            {
                Popup.Lock(message);

                OnResponse.Enque(Callback);

                if(IsProcessing == false) Request();

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

                if (element.IsProcessing == false) element.Request();
            }

            public event Action OnRequest;
            public virtual void Request()
            {
                if (IsProcessing)
                    throw new InvalidOperationException("Cannot Start Procedure " + GetType().Name + " When it's Already Processing");

                IsProcessing = true;

                OnRequest?.Invoke();
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

            protected virtual void ReplicateResponse(Response response)
            {
                if (response.HasError)
                    InvokeError(response.Error);
                else if (response.Canceled)
                    Cancel();
                else
                    End();
            }
            protected virtual void ReplicateResponse(Response response, Action callback)
            {
                if (response.Success)
                    callback.Invoke();
                else
                    ReplicateResponse(response);
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

        [Serializable]
        public abstract class Procedure<TReference> : Procedure, IReference<TReference>
        {
            [NonSerialized]
            protected TReference Reference;

            public virtual void Configure(TReference reference)
            {
                this.Reference = reference;
            }

            public virtual void Init()
            {

            }
        }
        #endregion

        public virtual void Register(IReference<Core> module)
        {
            References.Configure(this, module);

            OnInit += InitCallback;

            void InitCallback()
            {
                OnInit -= Init;

                References.Init(this, module);
            }
        }

        protected virtual void Configure()
        {
            Audio = this.GetDependancy<AudioCore>();
            Scenes = this.GetDependancy<ScenesCore>();
            UI = this.GetDependancy<UICore>();
            Leaderboards = this.GetDependancy<LeaderboardsCore>();
            Player = this.GetDependancy<PlayerCore>();
            PlayFab = this.GetDependancy<PlayFabCore>();
            Authentication = this.GetDependancy<AuthenticationCore>();
            Countries = this.GetDependancy<CountriesCore>();
            Facebook = this.GetDependancy<FacebookCore>();
            News = this.GetDependancy<NewsCore>();
            Unity = this.GetDependancy<UnityCore>();
            Internet = this.GetDependancy<InternetCore>();

            Application.targetFrameRate = 60;

            References.Configure(this);
            Initializer.Configure(gameObject);

            Init();
        }

        public event Action OnInit;
        protected virtual void Init()
        {
            References.Init(this);
            Initializer.Init(gameObject);

            OnInit?.Invoke();
        }

        [Button("Clear Player Prefs")]
        public virtual void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }
	}
}