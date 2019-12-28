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

        [SerializeField]
        AxisConstraintsData axisConstraints = new AxisConstraintsData();
        [Serializable]
        public class AxisConstraintsData
        {
            [SerializeField]
            protected bool x;
            public bool X { get { return x; } }

            [SerializeField]
            protected bool y;
            public bool Y { get { return y; } }

            [SerializeField]
            protected bool z;
            public bool Z { get { return z; } }

            public AxisConstraintsData()
            {
                x = y = z = true;
            }
        }

        protected override void UpdateState(float value)
        {
            base.UpdateState(value);

            var scale = transform.localScale;

            var eval = curve.Evaluate(value);

            if (axisConstraints.X) scale.x = eval;
            if (axisConstraints.Y) scale.y = eval;
            if (axisConstraints.Z) scale.z = eval;

            transform.localScale = scale;
        }
    }
}