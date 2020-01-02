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
	public class BackgroundFaderUI : FaderUI
	{
        public const float Duration = 0.1f;

        public static List<BackgroundFaderUI> list = new List<BackgroundFaderUI>();
        static void Add(BackgroundFaderUI element)
        {
            if(list.Contains(element))
            {
                Debug.LogWarning(nameof(BackgroundFaderUI) + " already contains element: " + element.name + ", ignoring add request");
                return;
            }

            element.To(element.InitialValue, Duration);
            if(Last != null) element.Value = Last.Value;

            if (Last != null) Last.To(0f, Duration);

            list.Add(element);
        }
        static void Remove(BackgroundFaderUI element)
        {
            if (list.Contains(element) == false)
            {
                Debug.LogWarning(nameof(BackgroundFaderUI) + " doesn't contain element: " + element.transform.parent.name + ", ignoring remove request");
                return;
            }

            list.Remove(element);

            if (Last != null) Last.To(Last.InitialValue, Duration / 2f);
        }
        static BackgroundFaderUI Last
        {
            get
            {
                if (list.Count == 0) return null;

                return list[list.Count - 1];
            }
        }

        public UIElement UIElement { get; protected set; }

        public float InitialValue { get; protected set; }

        public override void Configure()
        {
            base.Configure();

            UIElement = this.GetDependancy<UIElement>(Dependancy.Scope.CurrentToParents);

            InitialValue = Value;
        }

        public override void Init()
        {
            base.Init();

            UIElement.OnShow += ShowCallback;
            UIElement.OnHide += HideCallback;
        }

        private void ShowCallback() => Add(this);
        private void HideCallback() => Remove(this);
    }
}