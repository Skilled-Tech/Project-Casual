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
	public class QuitLevelOperation : Operation
	{
        public Core Core => Core.Instance;
        public Level Level => Level.Instance;

        public override void Execute()
        {
            Core.UI.Choice.Show("Are You Sure You Wish To Quit ?", ResultCallback);

            void ResultCallback(bool result)
            {
                if (result)
                {
                    Level.Quit();
                }
                else
                {
                    
                }
            }
        }
    }
}