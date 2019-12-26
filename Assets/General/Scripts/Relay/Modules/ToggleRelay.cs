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
    [RequireComponent(typeof(Toggle))]
	public class ToggleRelay : Relay
	{
        [SerializeField]
        protected bool ignoreOff = true;
        public bool IgnoreOff { get { return ignoreOff; } } 

        Toggle toggle;

        void Start()
        {
            toggle = GetComponent<Toggle>();

            toggle.onValueChanged.AddListener(OnChange);
        }

        void OnChange(bool newValue)
        {
            if (!ignoreOff || newValue)
                Invoke();
        }
    }
}