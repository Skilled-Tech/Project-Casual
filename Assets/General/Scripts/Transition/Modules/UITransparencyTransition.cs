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
    [RequireComponent(typeof(CanvasGroup))]
	public class UITransparencyTransition : Transition.Module
	{
        public CanvasGroup CanvasGroup { get; protected set; }

        public float Alpha
        {
            get => CanvasGroup.alpha;
            set => CanvasGroup.alpha = value;
        }

        public override void Configure(Transition reference)
        {
            base.Configure(reference);

            CanvasGroup = GetComponent<CanvasGroup>();
        }

        protected override void UpdateState(float value)
        {
            base.UpdateState(value);

            Alpha = value;
        }
    }
}