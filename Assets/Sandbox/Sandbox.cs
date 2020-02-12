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
        [Button("Execute")]
        void Execute()
        {
            
        }
    }
}