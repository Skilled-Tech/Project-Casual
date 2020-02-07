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

using System.Threading.Tasks;

using NaughtyAttributes;

namespace Game
{
	public class Sandbox : MonoBehaviour
	{
        [ReadOnly]
        public Vector3 vector;

        [ShowNativeProperty]
        public float A => 5f;

        [ShowNativeProperty]
        public static float B => 5f;

        [Button]
        void Call()
        {
            Debug.Log("Hello World");
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            
        }
    }
}