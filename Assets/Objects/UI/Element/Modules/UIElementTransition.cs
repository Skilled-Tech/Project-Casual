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

        public override float Value
        {
            set
            {
                base.Value = value;

                if (Mathf.Approximately(Value, 0f)) Element.Target.SetActive(false);
            }
        }

        public override void Configure()
        {
            Element = GetComponent<UIElement>();

            Element.OnShow += ShowCallback;
            Element.OnHide += HideCallback;

            Value = Element.IsOn ? 1f : 0f;

            base.Configure();
        }

        protected virtual void ShowCallback() => Perform(1f);
        protected virtual void HideCallback() => Perform(0f);

        public override void Perform(float target)
        {
            if (target > 0f) Element.Target.SetActive(true);

            base.Perform(target);
        }

        protected virtual void OnDestroy()
        {
            Element.OnShow -= ShowCallback;
            Element.OnHide -= HideCallback;
        }
    }
}