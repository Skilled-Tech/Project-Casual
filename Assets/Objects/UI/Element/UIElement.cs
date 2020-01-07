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
#pragma warning disable CS0108
    public class UIElement : MonoBehaviour, IInitialize
    {
        public RectTransform transform { get; protected set; }

        public GameObject Target => gameObject;

        public bool Visible => gameObject.activeInHierarchy;

        public bool IsOn
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

        public UIElement Parent { get; protected set; }

        public virtual void Configure()
        {
            transform = GetComponent<RectTransform>();

            Transition = GetComponent<UIElementTransition>();

            Parent = this.GetDependancy<UIElement>(Dependancy.Scope.Parents);

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
            IsOn = !IsOn;
        }

        public static class Utility
        {
            public static Coroutine ChainShow<TElement>(IList<TElement> list, MonoBehaviour behaviour)
            where TElement : UIElement
            {
                for (int i = 0; i < list.Count; i++)
                    if (list[i].IsOn)
                        list[i].SetActive(false);

                return ChainAction(list, behaviour, Action, Predicate);

                void Action(UIElement element) => element.Show();
                bool Predicate(int index) => list[index].Transition == null ? true : Mathf.Approximately(list[index].Transition.Value, 1f);
            }

            public static Coroutine ChainHide<TElement>(IList<TElement> list, MonoBehaviour behaviour)
            where TElement : UIElement
            {
                return ChainAction(list, behaviour, Action, Predicate);

                void Action(UIElement element) => element.Hide();
                bool Predicate(int index) => list[index].Transition == null ? true : Mathf.Approximately(list[index].Transition.Value, 0f);
            }

            public static Coroutine ChainAction<TElement>(IList<TElement> list, MonoBehaviour behaviour, Action<UIElement> action, Predicate<int> predicate)
            where TElement : UIElement
            {
                return behaviour.StartCoroutine(Procedure());

                IEnumerator Procedure()
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        action(list[i]);

                        bool IsReady() => predicate(i);

                        yield return new WaitUntil(IsReady);
                    }
                }
            }
        }
    }
#pragma warning restore CS0108
}