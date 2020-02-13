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
        protected Text text;
        public Text Text { get { return text; } }

        [SerializeField]
        protected LinksProperty links;
        public LinksProperty Links { get { return links; } }
        [Serializable]
        public class LinksProperty : Property
        {
            [SerializeField]
            protected RectTransform panel;
            public RectTransform Panel { get { return panel; } }

            [SerializeField]
            protected GameObject template;
            public GameObject Template { get { return template; } }

            public List<NewsReportLinkUITemplate> Elements { get; protected set; }

            public override void Configure(NewsUI reference)
            {
                base.Configure(reference);

                Elements = new List<NewsReportLinkUITemplate>();
            }

            public virtual void Clear()
            {
                for (int i = 0; i < Elements.Count; i++)
                    Destroy(Elements[i].gameObject);

                Elements.Clear();
            }

            public virtual void UpdateState(NewsReport report)
            {
                Clear();

                panel.gameObject.SetActive(report.HasLinks);

                if (report.HasLinks)
                {
                    for (int i = 0; i < report.Links.Length; i++)
                    {
                        var instance = Create(report.Links[i]);

                        Elements.Add(instance);
                    }
                }
            }

            protected virtual NewsReportLinkUITemplate Create(NewsReport.LinkData link)
            {
                var instance = NewsReportLinkUITemplate.Create(template, panel);

                instance.Set(link);

                return instance;
            }
        }

        [SerializeField]
        protected ProgressProperty progress;
        public ProgressProperty Progress { get { return progress; } }
        [Serializable]
        public class ProgressProperty : Property
        {
            [SerializeField]
            protected Text label;
            public Text Label { get { return label; } }

            [SerializeField]
            protected Selectable selectable;
            public Selectable Selectable { get { return selectable; } }

            public string Text
            {
                get => label.text;
                set => label.text = value;
            }

            [SerializeField]
            protected Relay relay;
            public Relay Relay { get { return relay; } }

            public override void Configure(NewsUI reference)
            {
                base.Configure(reference);

                relay.OnInvoke += RelayCallback;
            }

            private void RelayCallback()
            {
                News.StartCoroutine(Procedure());
                IEnumerator Procedure()
                {
                    if (News.Queue.Count == 0)
                    {
                        News.Hide();
                    }
                    else
                    {
                        selectable.interactable = false;
                        {
                            News.Panel.Hide();
                            bool PanelIsHidden() => News.Panel.Transition == null ? true : News.Panel.Transition.Value == 0f;
                            yield return new WaitUntil(PanelIsHidden);
                        }
                        selectable.interactable = true;

                        Action();
                    }
                }
            }

            public virtual NewsReport Action()
            {
                if (News.Queue.Count == 0)
                {
                    Debug.Log("trying to progress empty news queue, ignoring");
                    return null;
                }

                var report = News.Queue.Dequeue();

                News.Report(report);

                return report;
            }
        }

        public class Property : IReference<NewsUI>
        {
            public NewsUI News { get; protected set; }

            public virtual void Configure(NewsUI reference)
            {
                News = reference;
            }

            public virtual void Init()
            {

            }
        }

        public Queue<NewsReport> Queue { get; protected set; }

        public UIElement Element { get; protected set; }

        public Core Core => Core.Instance;

        public virtual void Configure()
        {
            Element = GetComponent<UIElement>();

            Queue = new Queue<NewsReport>();

            References.Configure(this, links);
            References.Configure(this, progress);
        }
        public virtual void Init()
        {
            Core.News.OnUpdate.Add(CoreUpdateCallback);

            References.Init(this, links);
            References.Init(this, progress);
        }

        void CoreUpdateCallback()
        {
            Show(Core.News.Reports);
        }

        public virtual void Show(IList<NewsReport> list)
        {
            if (list.Count == 0)
            {
                Debug.LogWarning("Trying to show news UI with zero news reports, ignoring");
                return;
            }

            Queue.Clear();

            for (int i = 0; i < list.Count; i++)
            {
                if (Core.News.Occurrence.CanDisplay(list[i]) == false) continue;

                Queue.Enqueue(list[i]);
            }

            if(Queue.Count == 0)
            {
                return;
            }

            Element.Show();
            panel.SetActive(false);

            Progress.Action();
        }

        public virtual void Report(NewsReport report)
        {
            Element.Show();
            panel.Show();

            UpdateState(report);

            Core.News.Occurrence.Register(report);
        }

        protected virtual void UpdateState(NewsReport report)
        {
            title.text = report.Title;
            text.text = report.Text;

            links.UpdateState(report);

            progress.Text = Queue.Count == 0 ? "Close" : "Next";
        }

        public virtual void Hide()
        {
            Panel.Hide();
            Element.Hide();
        }
    }
}