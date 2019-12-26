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
                var gameObject = GameObject.Instantiate(prefab);

                gameObject.name = nameof(Core);

                GameObject.DontDestroyOnLoad(gameObject);

                Instance = gameObject.GetComponent<Core>();
                Instance.Configure();
            }
        }

        public AudioCore Audio { get; protected set; }

        public class Module<TModule> : MonoBehaviour, IReference<TModule>
        {
            public TModule Reference { get; protected set; }

            public virtual void Configure(TModule reference)
            {
                this.Reference = reference;
            }

            public virtual void Init()
            {

            }
        }
        public class Module : Module<Core>
        {
            public Core Core => Reference;
        }

        protected virtual void Configure()
        {
            Audio = this.GetDependancy<AudioCore>();

            References.Configure(this);

            Init();
        }

        protected virtual void Init()
        {
            References.Init(this);
        }
	}
}