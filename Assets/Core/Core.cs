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
        public AdsCore Ads { get; protected set; }
        public PlayFabCore PlayFab { get; protected set; }
        public LeaderboardsCore Leaderboards { get; protected set; }
        public ProceduresCore Procedures { get; protected set; }
        public CountriesCore Countries { get; protected set; }
        public FacebookCore Facebook { get; protected set; }
        public NewsCore News { get; protected set; }

        #region Modules
        public class Module<TModule> : MonoBehaviour, IReference<TModule>
        {
            public Core Core => Core.Instance;

            public TModule Reference { get; protected set; }

            protected virtual void Register<TReference>(TReference reference, IReference<TReference> module)
            {
                References.Configure(reference, module);

                OnInit += InitCallback;

                void InitCallback()
                {
                    OnInit -= Init;

                    References.Init(reference, module);
                }
            }

            public virtual void Configure(TModule reference)
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
        public class Property<TModule> : IReference<TModule>
        {
            public Core Core => Core.Instance;

            public TModule Reference { get; protected set; }

            protected virtual void Register<TReference>(TReference reference, IReference<TReference> module)
            {
                References.Configure(reference, module);

                OnInit += InitCallback;

                void InitCallback()
                {
                    OnInit -= Init;

                    References.Init(reference, module);
                }
            }

            public virtual void Configure(TModule reference)
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
            Ads = this.GetDependancy<AdsCore>();
            Leaderboards = this.GetDependancy<LeaderboardsCore>();
            PlayFab = this.GetDependancy<PlayFabCore>();
            Procedures = this.GetDependancy<ProceduresCore>();
            Countries = this.GetDependancy<CountriesCore>();
            Facebook = this.GetDependancy<FacebookCore>();
            News = this.GetDependancy<NewsCore>();

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