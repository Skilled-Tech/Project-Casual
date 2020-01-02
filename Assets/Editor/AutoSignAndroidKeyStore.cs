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
	public static class AutoSignAndroidKeyStore
	{
        [InitializeOnLoadMethod]
        static void OnLoad() => Execute();

        [MenuItem("Tools/Enter Android Keys")]
        static void MenuItem() => Execute();

        public static void Execute()
        {
            PlayerSettings.Android.keystorePass = "!Fv#79QJ)U";
            PlayerSettings.Android.keyaliasPass = "!Fv#79QJ)U";
        }
	}
}