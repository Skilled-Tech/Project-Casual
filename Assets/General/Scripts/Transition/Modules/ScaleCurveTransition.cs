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
	public class ScaleCurveTransition : Transition.Module
	{
        [SerializeField]
        AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        protected override void UpdateState(float value)
        {
            transform.localScale = Vector3.one * curve.Evaluate(value);

            base.UpdateState(value);
        }
    }
}