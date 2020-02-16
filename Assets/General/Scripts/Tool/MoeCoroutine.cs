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
	public abstract class MoeCoroutine
	{
        public MonoBehaviour MonoBehaviour { get; protected set; }
        public virtual void Configure(MonoBehaviour reference)
        {
            this.MonoBehaviour = reference;
        }

        public Coroutine Coroutine { get; protected set; }
        public virtual bool IsProcessing => Coroutine != null;

        public virtual void Start()
        {
            if (IsProcessing) Stop();

            Coroutine = MonoBehaviour.StartCoroutine(Procedure());
        }

        public virtual void Stop()
        {
            if(IsProcessing)
            {
                MonoBehaviour.StopCoroutine(Coroutine);
                Coroutine = null;
            }
        }

        public IEnumerator Procedure()
        {
            yield return Function();

            Coroutine = null;
        }

        public abstract IEnumerator Function();
	}
}