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

        private void Start()
        {
            Show();
        }

        void Show()
        {
            TextInput.Show("Enter Word", ResultCallback);

            void ResultCallback(string result)
            {
                if(result == null)
                {
                    Debug.Log("Canceled");
                }
                else
                {
                    Debug.Log("Accepted: " + result);
                }

                Show();
            }
        }
    }
}