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
	public class ToggleRelay : SelectableRelay<Toggle>
	{
        [SerializeField]
        protected bool ignoreOff = true;
        public bool IgnoreOff { get { return ignoreOff; } }

        public override void Configure()
        {
            base.Configure();

            Component.onValueChanged.AddListener(OnChange);
        }

        void OnChange(bool newValue)
        {
            if (!ignoreOff || newValue)
                Invoke();
        }
    }
}