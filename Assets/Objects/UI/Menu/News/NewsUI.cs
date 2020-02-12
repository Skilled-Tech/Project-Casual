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
    public class NewsUI : MonoBehaviour, IInitialize
	{
        [SerializeField]
        protected UIElement panel;
        public UIElement Panel { get { return panel; } }

        [SerializeField]
        protected Text title;
        public Text Title { get { return title; } }

        [SerializeField]
        protected Text body;
        public Text Body { get { return body; } }

        [SerializeField]
        protected Relay progress;
        public Relay Progress { get { return progress; } }

        public int Index { get; protected set; }

        public UIElement Element { get; protected set; }

        public Core Core => Core.Instance;

        public virtual void Configure()
        {
            Element = GetComponent<UIElement>();
        }
        public virtual void Init()
        {
            progress.OnInvoke += ProgressAction;

            Core.News.OnUpdate.Add(UpdateCallback);
        }

        void UpdateCallback()
        {
            Show();
        }

        void ProgressAction()
        {
            StartCoroutine(Procedure());
            IEnumerator Procedure()
            {
                Index++;

                if (Index >= Core.News.Reports.Count)
                {
                    Panel.Hide();
                    Element.Hide();
                }
                else
                {
                    progress.enabled = false;
                    {
                        panel.Hide();
                        bool PanelIsHidden() => panel.Transition == null ? true : panel.Transition.Value == 0f;
                        yield return new WaitUntil(PanelIsHidden);
                    }
                    progress.enabled = true;

                    Report(Core.News.Reports[Index]);
                }
            }
        }

        public virtual void Show()
        {
            if(Core.News.Reports.Count == 0)
            {
                Debug.LogWarning("Trying to show news UI with zero news reports, ignoring");
                return;
            }

            Index = 0;

            Element.Show();
            panel.SetActive(false);

            Report(Core.News.Reports[0]);
        }

        public virtual void Report(NewsReport report)
        {
            Debug.Log("Reporting: " + report.Title);

            Element.Show();
            panel.Show();

            UpdateState(report);
        }

        protected virtual void UpdateState(NewsReport report)
        {
            title.text = report.Title;

            body.text = report.Body;
        }
    }
}