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
	public class Sandbox : MonoBehaviour
	{
        public FaderUI fader1;
        public FaderUI fader2;

        [Range(0f, 1f)]
        public float rate = 0.5f;

        private void Update()
        {
            fader1.Value = Mathf.Lerp(0f, 0.8f, rate);
            fader2.Value = Mathf.Lerp(0.8f, 0f, rate);
        }
    }
}