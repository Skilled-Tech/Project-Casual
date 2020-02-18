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

using UnityEngine.Purchasing;

namespace Game
{
	public class UnityIAPCore : UnityCore.Module
	{
        public override void Configure(UnityCore reference)
        {
            base.Configure(reference);
        }
    }
}