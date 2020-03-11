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
	public class LevelPlayPhase : LevelPhases.Element
	{
        public override void Begin()
        {
            base.Begin();

            Level.Player.Instance.OnFail += PlayerFailCallback;
        }

        private void PlayerFailCallback()
        {
            Level.Player.Instance.OnFail -= PlayerFailCallback;

            End();
        }
    }
}