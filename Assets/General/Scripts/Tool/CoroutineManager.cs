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
	public class CoroutineManager : MonoBehaviour
	{
        public static CoroutineManager Instance { get; protected set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnLoad()
        {
            var gameObject = new GameObject("Coroutine Manager");

            Instance = gameObject.AddComponent<CoroutineManager>();

            DontDestroyOnLoad(gameObject);
        }

        public static Coroutine YieldFrame(Action action)
        {
            return Yield(action, new WaitForEndOfFrame());
        }

        public static Coroutine YieldSeconds(Action action, float time) => YieldSeconds(action, time, true);
        public static Coroutine YieldSeconds(Action action, float time, bool realtime)
        {
            if (realtime)
                return Yield(action, new WaitForSecondsRealtime(time));
            else
                return Yield(action, new WaitForSeconds(time));
        }

        public static Coroutine Yield(Action action, object instruction)
        {
            return Start(Procedure());

            IEnumerator Procedure()
            {
                yield return instruction;

                action();
            }
        }

        public static Coroutine Start(IEnumerator routine) => Instance.StartCoroutine(routine);
    }
}