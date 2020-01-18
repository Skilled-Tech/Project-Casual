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

namespace Game
{
	public class Sandbox : MonoBehaviour
	{
        public TextInputUI TextInput => Core.Instance.UI.TextInput;

        public MoeEvent MyEvent = new MoeEvent();

        private void Start()
        {
            MyEvent.Add(() => Debug.Log("Add"));
            MyEvent.Enque(() => Debug.Log("Subscribe"));
        }

        private void Update()
        {
            if (Input.anyKeyDown)
                MyEvent.Invoke();
        }
    }
}