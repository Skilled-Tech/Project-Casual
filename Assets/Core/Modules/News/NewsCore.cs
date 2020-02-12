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

namespace Game
{
    public class NewsCore : Core.Module
    {
        [SerializeField]
        protected List<NewsReport> reports;
        public List<NewsReport> Reports { get { return reports; } }

        public PlayFabCore PlayFab => Core.PlayFab;

        public override void Configure(Core reference)
        {
            base.Configure(reference);

            reports = new List<NewsReport>();

            PlayFab.Title.News.OnResponse.Add(PlayFabResponseCallback);
            PlayFab.Login.OnResult.Add(LoginCallback);
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

        public string Body { get; protected set; }

#pragma warning disable CS0649
        [JsonProperty]
        [TextArea]
        [SerializeField]
        private string text;
        public string Text { get { return text; } }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(NewsReportRecurrence.Once)]
        [SerializeField]
        private NewsReportRecurrence recurrence;
        public NewsReportRecurrence Recurrence { get { return recurrence; } }

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
            var result = new NewsReport()
            {
                ID = item.NewsId,
                title = item.Title,
                Body = item.Body,
            };

            JsonConvert.PopulateObject(result.Body, result);

            return result;
        }
    }

    public enum NewsReportRecurrence
    {
        Once, Constant
    }
}