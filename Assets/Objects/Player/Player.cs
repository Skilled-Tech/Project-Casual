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
#pragma warning disable CS0108
    public class Player : MonoBehaviour, IInitialize
    {
        public PlayerScore Score { get; protected set; }

        public class Module : MonoBehaviour, IReference<Player>
        {
            public Core Core => Core.Instance;
            public Level Level => Level.Instance;

            public Player Player { get; protected set; }
            public virtual void Configure(Player reference)
            {
                this.Player = reference;
            }

            public virtual void Init()
            {
                
            }
        }

        public Core Core => Core.Instance;
        public Level Level => Level.Instance;

        public Rigidbody rigidbody { get; protected set; }

        public virtual void Configure()
        {
            rigidbody = GetComponent<Rigidbody>();

            Score = this.GetDependancy<PlayerScore>();

            References.Configure(this);
        }

        public virtual void Init()
        {
            References.Init(this);

            Level.Menu.Input.OnClick += ClickCallback;
        }

        protected virtual void Update()
        {
            Process();
        }

        public event Action OnProcess;
        protected virtual void Process()
        {
            OnProcess?.Invoke();
        }

        private void ClickCallback()
        {
            Debug.Log("Click");
        }
    }
#pragma warning restore CS0108
}