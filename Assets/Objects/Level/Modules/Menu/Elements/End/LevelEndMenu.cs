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
    [RequireComponent(typeof(UIElement))]
	public class LevelEndMenu : MonoBehaviour, IInitialize
	{
        [SerializeField]
        protected Text score;
        public Text Score { get { return score; } }

        public UIElement Element { get; protected set; }

        public virtual void Configure()
        {
            Element = GetComponent<UIElement>();
        }
        public virtual void Init()
        {
            
        }

        public virtual void Show(int points)
        {
            score.text = points.ToString("N0");

            Element.Show();
        }
    }
}