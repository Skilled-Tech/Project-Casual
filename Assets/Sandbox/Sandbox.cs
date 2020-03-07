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

using NaughtyAttributes;

using Newtonsoft.Json;

namespace Game
{
	public partial class Sandbox : MonoBehaviour
	{
        public RunCoroutine Run { get; protected set; }
        public class RunCoroutine : MoeCoroutine
        {
            public override IEnumerator Function()
            {
                yield return new WaitForSeconds(2f);

                Debug.Log("Hello");
            }
        }

        [Button("Execute")]
        void Execute()
        {
            StartCoroutine(Procedure());

            IEnumerator Procedure()
            {
                var ping = new Ping("8.8.8.8");
                var time = 0f;

                while(ping.isDone == false)
                {
                    time += Time.deltaTime;

                    yield return new WaitForEndOfFrame();
                }

                Debug.Log("Ping Complete in: " + time + "s");
            }
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            
        }
    }
}