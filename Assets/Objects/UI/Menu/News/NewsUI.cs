﻿using System;
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

                if(report.HasLinks)
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

        [SerializeField]
        protected Relay progress;
        public Relay Progress { get { return progress; } }

        public int Index { get; protected set; }

        public UIElement Element { get; protected set; }

        public Core Core => Core.Instance;

        public virtual void Configure()
        {
            Element = GetComponent<UIElement>();

            References.Configure(this, links);
        }
        public virtual void Init()
        {
            progress.OnInvoke += ProgressAction;

            Core.News.OnUpdate.Add(UpdateCallback);

            References.Init(this, links);
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
            Element.Show();
            panel.Show();

            UpdateState(report);
        }

        protected virtual void UpdateState(NewsReport report)
        {
            title.text = report.Title;

            text.text = report.Text;

            links.UpdateState(report);
        }
    }
}