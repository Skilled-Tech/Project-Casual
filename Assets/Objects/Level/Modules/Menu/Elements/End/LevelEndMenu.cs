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
	public class LevelEndMenu : UIElement
	{
        [SerializeField]
        protected Text score;
        public Text Score { get { return score; } }

        public virtual void Show(int points)
        {
            score.text = points.ToString("N0");

            base.Show();
        }
    }
}