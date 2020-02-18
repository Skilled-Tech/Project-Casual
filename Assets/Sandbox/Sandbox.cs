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

        private void Start()
        {
            Run = new RunCoroutine();
            Run.Configure(this);

            Run.Start();
        }

        private void Update()
        {
            if (Run.IsProcessing)
                Debug.Log("Is Running");
        }
    }
}