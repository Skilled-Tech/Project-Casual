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
	public class Sandbox : MonoBehaviour
	{
        public BellUI bell;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
                bell.Show();

            if (Input.GetKeyDown(KeyCode.S))
                bell.Chime();

            if (Input.GetKeyDown(KeyCode.D))
                bell.Hide();
        }
    }
}