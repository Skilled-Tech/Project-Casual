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

using PlayFab;
using PlayFab.ClientModels;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.ComponentModel;
using System.Runtime.Serialization;

using NaughtyAttributes;

namespace Game
{
    public class NewsCore : Core.Module
    {
        [SerializeField]
        protected List<NewsReport> reports;
        public List<NewsReport> Reports { get { return reports; } }

        [SerializeField]
        protected OccurrenceProperty occurrence;
        public OccurrenceProperty Occurrence { get { return occurrence; } }
        [Serializable]
        public class OccurrenceProperty : Property
        {
            public HashSet<string> Hash { get; protected set; }

            public const string Key = "News Reports Occurences";

            public string Pref
            {
                get => PlayerPrefs.GetString(Key, string.Empty);
                set => PlayerPrefs.SetString(Key, value);
            }

            public override void Configure(NewsCore reference)
            {
                base.Configure(reference);

                Hash = new HashSet<string>();

                Load();
            }

            public virtual void Load()
            {
                Parse(Pref);
            }
            public virtual void Parse(string json)
            {
                Hash.Clear();

                if (String.IsNullOrEmpty(json))
                {

                }
                else
                {
                    Hash = JsonConvert.DeserializeObject<HashSet<string>>(json);
                }
            }
            public virtual void Save()
            {
                var json = JsonConvert.SerializeObject(Hash);

                Pref = json;
            }

            public virtual bool Contains(NewsReport report) => Contains(report.ID);
            public virtual bool Contains(string ID) => Hash.Contains(ID);

            public virtual void Add(string ID)
            {
                if(Hash.Contains(ID))
                {
                    Debug.Log("News Report ID: " + ID + " Already Added to Occurence Property, Ignoring Add");
                    return;
                }

                Hash.Add(ID);

                Save();
            }
            public virtual void Remove(string ID)
            {
                if(Hash.Contains(ID) == false)
                {
                    Debug.Log("News Report ID: " + ID + " Not Found in Occurence Property, Ignoring Remove");
                    return;
                }

                Hash.Remove(ID);

                Save();
            }

            public virtual void Register(NewsReport report)
            {
                if(Contains(report.ID))
                {

                }
                else
                {
                    Add(report.ID);
                }
            }

            public virtual void Clear()
            {
                Pref = string.Empty;
            }
        }

        [Button("Clear Occurences")]
        void ClearOccurences()
        {
            occurrence.Clear();
        }

        public bool ContainsUnseenReports
        {
            get
            {
                for (int i = 0; i < reports.Count; i++)
                {
                    if (occurrence.Contains(reports[i].ID)) continue;

                    return true;
                }

                return false;
            }
        }

        [SerializeField]
        protected PopupsProperty popups;
        public PopupsProperty Popups { get { return popups; } }
        [Serializable]
        public class PopupsProperty : Property
        {
            public bool ContainsUnseenReports
            {
                get
                {
                    for (int i = 0; i < News.Reports.Count; i++)
                    {
                        if (News.Reports[i].Popup == false) continue;

                        if (News.Occurrence.Contains(News.Reports[i])) continue;

                        return true;
                    }

                    return false;
                }
            }

            public virtual IEnumerable<NewsReport> Iterate()
            {
                for (int i = 0; i < News.Reports.Count; i++)
                {
                    if (News.Reports[i].Popup == false) continue;

                    if (News.Occurrence.Contains(News.Reports[i])) continue;

                    yield return News.Reports[i];
                }
            }

            public override void Init()
            {
                base.Init();

                News.OnUpdate.Add(UpdateCallback);
            }

            private void UpdateCallback()
            {
                if (ContainsUnseenReports) Core.UI.News.Show(Iterate());
            }
        }
        
        [Serializable]
        public class Property : Core.Property<NewsCore>
        {
            public NewsCore News => Reference;
        }

        public PlayFabCore PlayFab => Core.PlayFab;

        public override void Configure(Core reference)
        {
            base.Configure(reference);

            reports = new List<NewsReport>();

            PlayFab.Title.News.OnResponse.Add(PlayFabResponseCallback);
            PlayFab.Login.OnResult.Add(LoginCallback);

            Register(this, occurrence);
            Register(this, popups);
        }

        public void Request()
        {
            PlayFab.Title.News.Request();
        }

        protected virtual void Parse(IList<TitleNewsItem> list)
        {
            reports.Clear();

            for (int i = 0; i < list.Count; i++)
            {
                NewsReport instance;

                try
                {
                    instance = NewsReport.Create(list[i]);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error parsing news item " + list[i].NewsId + ", ignoring news report" + Environment.NewLine + e.Message);
                    continue;
                }

                reports.Add(instance);
            }

            InvokeUpdate();
        }

        public MoeEvent OnUpdate { get; protected set; } = new MoeEvent();
        protected virtual void InvokeUpdate()
        {
            OnUpdate.Invoke();
        }

        void LoginCallback(LoginResult result)
        {
            Request();
        }

        void PlayFabResponseCallback(GetTitleNewsResult result, PlayFabError error)
        {
            if(error == null)
            {
                Parse(result.News);
            }
            else
            {

            }
        }
    }

    [Serializable]
    public class NewsReport
    {
        public string ID { get; protected set; }

        [SerializeField]
        private string title;
        public string Title { get { return title; } }

        public bool HasTitle => String.IsNullOrEmpty(title) == false;

        public string Body { get; protected set; }
        public bool HasBody => String.IsNullOrEmpty(Body) == false;

#pragma warning disable CS0649
        [JsonProperty]
        [TextArea]
        [SerializeField]
        private string text;
        public string Text { get { return text; } }

        public bool HasText => string.IsNullOrEmpty(text) == false;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(false)]
        [SerializeField]
        private bool popup;
        public bool Popup { get { return popup; } }

        [JsonProperty]
        [SerializeField]
        private LinkData[] links;
        public LinkData[] Links { get { return links; } }

        public bool HasLinks => links != null && links.Length > 0;

        [Serializable]
        public class LinkData
        {
            [JsonProperty]
            [SerializeField]
            protected string title;
            public string Title { get { return title; } }

            [JsonProperty(PropertyName = "url")]
            [SerializeField]
            protected string _URL;
            public string URL { get { return _URL; } }
        }
#pragma warning restore CS0649

        public static NewsReport Create(TitleNewsItem item)
        {
            var report = new NewsReport()
            {
                ID = item.NewsId,
                title = item.Title,
                Body = item.Body,
            };

            if(report.HasBody)
                JsonConvert.PopulateObject(report.Body, report);

            return report;
        }
    }

    public enum NewsReportRecurrence
    {
        Once, Constant
    }
}