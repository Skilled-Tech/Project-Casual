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
        AxisConstraintsProperty axisConstraints = new AxisConstraintsProperty();
        public AxisConstraintsProperty AxisConstrains => axisConstraints;

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

    [Serializable]
    public class AxisConstraintsProperty
    {
        [SerializeField]
        public bool X;

        [SerializeField]
        public bool Y;

        [SerializeField]
        public bool Z;

        public AxisConstraintsProperty() : this(true)
        {

        }
        public AxisConstraintsProperty(bool value) : this(value, value, value)
        {

        }
        public AxisConstraintsProperty(bool x, bool y, bool z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
}