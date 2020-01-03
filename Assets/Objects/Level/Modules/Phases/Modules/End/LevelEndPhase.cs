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
	public class LevelEndPhase : LevelPhases.Element
    {
        public LevelEndPhaseScore Score { get; protected set; }

        public class Module : MonoBehaviour, IReference<LevelEndPhase>
        {
            public LevelEndPhase Phase { get; protected set; }
            public virtual void Configure(LevelEndPhase reference)
            {
                Phase = reference;
            }

            public Level Level => Phase.Level;

            public Core Core => Core.Instance;

            public virtual void Init()
            {
                
            }
        }

        public override void Configure(LevelPhases reference)
        {
            base.Configure(reference);

            Score = this.GetDependancy<LevelEndPhaseScore>();

            References.Configure(this);
        }

        public override void Init()
        {
            base.Init();

            References.Init(this);
        }

        public override void Begin()
        {
            base.Begin();

            Level.Menu.End.Show();

            Level.OnEnd += End;
        }

        protected override void End()
        {
            base.End();

            Level.OnEnd -= End;
        }
    }
}