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
    public class UIElementTransition : Transition
    {
        public UIElement Element { get; protected set; }

        public override void Configure()
        {
            Element = GetComponent<UIElement>();

            Element.OnShow += ShowCallback;
            Element.OnHide += HideCallback;

            Value = Element.IsOn ? 1f : 0f;

            base.Configure();
        }

        protected virtual void ShowCallback() => To(1f);
        protected virtual void HideCallback() => To(0f);

        public override void To(float target)
        {
            if (target > 0f) Element.Target.SetActive(true);

            base.To(target);
        }

        protected override IEnumerator Procedure(float target)
        {
            yield return base.Procedure(target);

            if (Mathf.Approximately(Value, 0f)) Element.Target.SetActive(false);
        }

        protected virtual void OnDestroy()
        {
            Element.OnShow -= ShowCallback;
            Element.OnHide -= HideCallback;
        }
    }
}