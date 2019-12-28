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
    //[DisallowMultipleComponent]
	public class UIElement : MonoBehaviour, IInitialize
    {
        public GameObject Target => gameObject;

        public bool Visible
        {
            get
            {
                return Target.activeSelf;
            }
            set
            {
                if (value)
                    Show();
                else
                    Hide();
            }
        }

        public UIElementTransition Transition { get; protected set; }

        public virtual void Configure()
        {
            Transition = GetComponent<UIElementTransition>();

            References.Configure(this);
        }
        public virtual void Init()
        {
            References.Init(this);
        }

        public event Action OnShow;
        public virtual void Show()
        {
            if (Transition == null)
                Target.SetActive(true);

            OnShow?.Invoke();
        }

        public event Action OnHide;
        public virtual void Hide()
        {
            if (Transition == null)
                Target.SetActive(false);

            OnHide?.Invoke();
        }

        public virtual void Toggle()
        {
            Visible = !Visible;
        }
    }
}