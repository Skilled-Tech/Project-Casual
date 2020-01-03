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
	public class LevelPhases : Level.Module
	{
        #region List
        public List<Element> List { get; protected set; }

        public int Count => List.Count;
        public Element this[int index] => List[index];
        #endregion

        [SerializeField]
        protected CommonData common;
        public CommonData Common { get { return common; } }
        [Serializable]
        public class CommonData
        {
            [SerializeField]
            protected LevelStartPhase start;
            public LevelStartPhase Start { get { return start; } }

            [SerializeField]
            protected LevelEndPhase end;
            public LevelEndPhase End { get { return end; } }
        }

        public Element Current { get; protected set; }

        public class Element : Module
        {
            #region Query
            public int Index { get; protected set; }

            public bool IsFirst => Index == 0;
            public bool IsLast => Index + 1 >= Phases.Count;

            public virtual Element Previous => IsFirst ? null : Phases[Index - 1];
            public virtual Element Next => IsLast ? null : Phases[Index + 1];
            #endregion

            public bool IsProcessing => Phases.Current == this;

            public override void Configure(LevelPhases reference)
            {
                base.Configure(reference);

                Index = reference.List.IndexOf(this);
            }

            public event Action OnBegin;
            public virtual void Begin()
            {
                OnBegin?.Invoke();
            }

            public virtual void Stop()
            {
                End();
            }

            public event Action OnEnd;
            protected virtual void End()
            {
                OnEnd?.Invoke();
            }
        }

        public delegate void ElementDelegate(Element element);

        public class Module : MonoBehaviour, IReference<LevelPhases>
        {
            public LevelPhases Phases { get; protected set; }
            public virtual void Configure(LevelPhases reference)
            {
                Phases = reference;
            }

            public Level Level => Phases.Level;

            public Core Core => Core.Instance;

            public virtual void Init()
            {
                
            }
        }

        public override void Configure(Level reference)
        {
            base.Configure(reference);

            List = this.GetAllDependancies<Element>();

            References.Configure(this);

            for (int i = 0; i < Count; i++)
            {
                var element = this[i];

                this[i].OnBegin += () => ElementBeginCallback(element);
                this[i].OnEnd += () => ElementEndCallback(element);
            }
        }

        public override void Init()
        {
            base.Init();

            References.Init(this);

            if (List.Count > 0) Initiate(this[0]);
        }

        protected virtual void Initiate(Element element)
        {
            Current = element;

            element.Begin();
        }

        public event ElementDelegate OnBegin;
        private void ElementBeginCallback(Element element)
        {
            OnBegin?.Invoke(element);
        }

        public event ElementDelegate OnEnd;
        private void ElementEndCallback(Element element)
        {
            if (element.IsLast)
            {

            }
            else
            {
                Initiate(element.Next);
            }

            OnEnd?.Invoke(element);
        }
    }
}