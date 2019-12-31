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
    [DisallowMultipleComponent]
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

        public virtual void SetActive(bool value)
        {
            Target.SetActive(value);

            if(Transition != null)
            {
                Transition.Value = value ? 1f : 0f;
            }
        }

        public event Action OnShow;
        public virtual void Show()
        {
            if (Transition == null)
                SetActive(true);

            OnShow?.Invoke();
        }

        public event Action OnHide;
        public virtual void Hide()
        {
            if (Transition == null)
                SetActive(false);

            OnHide?.Invoke();
        }

        public virtual void Toggle()
        {
            Visible = !Visible;
        }

        public static class Utility
        {
            public static Coroutine ChainDisplay<TElement>(IList<TElement> list, MonoBehaviour behaviour)
            where TElement : UIElement
            {
                return behaviour.StartCoroutine(Procedure());

                IEnumerator Procedure()
                {
                    for (int i = 0; i < list.Count; i++)
                        if (list[i].Visible)
                            list[i].SetActive(false);

                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].Show();

                        bool IsReady() => list[i].Transition == null ? true : Mathf.Approximately(list[i].Transition.Value, 1f);

                        yield return new WaitUntil(IsReady);
                    }
                }
            }
        }
    }
}