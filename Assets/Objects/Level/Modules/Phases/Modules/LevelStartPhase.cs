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
	public class LevelStartPhase : LevelPhases.Element
    {
        private void Update()
        {
            if(IsProcessing)
            {
                if (Input.GetKeyDown(KeyCode.E) || Input.touchCount > 1 || (Input.GetMouseButton(0) && Input.GetMouseButton(1)))
                    End();
            }
        }

        protected override void End()
        {
            base.End();
        }
    }
}