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
	public class ScenesCore : Core.Module
	{
		[SerializeField]
        protected GameScene mainMenu;
        public GameScene MainMenu { get { return mainMenu; } }

        [SerializeField]
        protected GameScene level;
        public GameScene Level { get { return level; } }

        public virtual void Load(GameScene scene) => Load(scene.name);
        public virtual void Load(string name)
        {
            StartCoroutine(Procedure());

            IEnumerator Procedure()
            {
                yield return Core.UI.Fader.To(1f, 0.2f);

                yield return new WaitForSeconds(0.2f);

                SceneManager.LoadScene(name);

                yield return new WaitForSeconds(0.2f);

                yield return Core.UI.Fader.To(0f, 0.2f);
            }
        }
    }
}