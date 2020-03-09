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
        public GameObject marker;

        public float ammount;

        public float spacing;

        [Button("Execute")]
        void Execute()
        {
            var transform = new GameObject("A").transform;

            transform.SetParent(this.transform);

            transform.localPosition = Vector3.zero;

            for (int i = 0; i < ammount; i++)
            {
                var instance = Instantiate(marker);

                instance.transform.SetParent(transform);

                instance.transform.localPosition = Vector3.forward * i * spacing;
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