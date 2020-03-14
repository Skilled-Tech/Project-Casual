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
        public PlayerControls Controls { get; protected set; }

        public PlayerInput Input { get; protected set; }

        public PlayerMovement Movement { get; protected set; }

        public PlayerJump Jump { get; protected set; }

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

            protected virtual void OnEnable()
            {

            }
        }

        public Core Core => Core.Instance;
        public Level Level => Level.Instance;

        public Rigidbody rigidbody { get; protected set; }

        public Collider collider { get; protected set; }

        public virtual void Configure()
        {
            rigidbody = GetComponent<Rigidbody>();

            collider = GetComponent<Collider>();

            Controls = this.GetDependancy<PlayerControls>();

            Input = this.GetDependancy<PlayerInput>();

            Movement = this.GetDependancy<PlayerMovement>();

            Jump = this.GetDependancy<PlayerJump>();

            Score = this.GetDependancy<PlayerScore>();

            References.Configure(this);
        }

        public virtual void Init()
        {
            References.Init(this);
        }

        protected virtual void Update()
        {
            Process();

            if (transform.position.y < -5) Fail();
        }

        public event Action OnProcess;
        protected virtual void Process()
        {
            OnProcess?.Invoke();
        }

        public event Action OnFail;
        public virtual void Fail()
        {
            OnFail?.Invoke();
        }
    }
#pragma warning restore CS0108
}