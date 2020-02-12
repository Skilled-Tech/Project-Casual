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

using System.ComponentModel;

namespace Game
{
    public class NewsCore : Core.Module
    {
        public const string Key = "News";

        public string JSON { get; protected set; }

        [SerializeField]
        protected List<NewsReport> reports;
        public List<NewsReport> Reports { get { return reports; } }

        public PlayFabCore PlayFab => Core.PlayFab;

        public override void Configure(Core reference)
        {
            base.Configure(reference);

            PlayFab.Title.Data.OnResponse.Add(TitleDataCallback);

            PlayFab.Login.OnResult.Add(LoginCallback);
        }

        public void Request()
        {
            PlayFab.Title.Data.Request(Key);
        }

        protected virtual void Parse(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                reports.Clear();
            }
            else
            {
                try
                {
                    reports = JsonConvert.DeserializeObject<List<NewsReport>>(json);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error parsing news: " + Environment.NewLine + e.Message);

                    reports.Clear();
                }
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

        void TitleDataCallback(GetTitleDataResult result, PlayFabError error)
        {
            var request = result.Request as GetTitleDataRequest;

            if (request.Keys.Contains(Key))
            {
                if (error == null)
                {
                    if (result.Data.ContainsKey(Key))
                        JSON = result.Data[Key];
                    else
                        JSON = string.Empty;

                    Parse(JSON);
                }
                else
                {

                }
            }
        }
    }

    [Serializable]
    public struct NewsReport
    {
        [JsonProperty(Required = Required.Always)]
        [SerializeField]
        private string id;
        public string ID { get { return id; } }

        [JsonProperty(Required = Required.Always)]
        [SerializeField]
        private string title;
        public string Title { get { return title; } }

        [JsonProperty(Required = Required.Always)]
        [SerializeField]
        private string body;
        public string Body { get { return body; } }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(NewsReportRecurrence.Once)]
        [SerializeField]
        private NewsReportRecurrence recurrence;
        public NewsReportRecurrence Recurrence { get { return recurrence; } }

        public NewsReport(string id, string title, string body, NewsReportRecurrence recurrence)
        {
            this.id = id;
            this.title = title;
            this.body = body;
            this.recurrence = recurrence;
        }
    }

    public enum NewsReportRecurrence
    {
        Once, Constant
    }
}