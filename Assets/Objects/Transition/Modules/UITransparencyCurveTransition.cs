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
	public class UITransparencyCurveTransition : Transition.Module
	{
        [SerializeField]
        AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

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

            Alpha = curve.Evaluate(value);
        }
    }
}