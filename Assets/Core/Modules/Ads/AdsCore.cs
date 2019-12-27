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

using UnityEngine.Advertisements;

namespace Game
{
	public class AdsCore : Core.Module
    {
        [SerializeField]
        protected GameIDProperty gameID;
        public GameIDProperty GameID { get { return gameID; } }
        [Serializable]
        public class GameIDProperty
        {
            [SerializeField]
            protected string android;
            public string Android { get { return android; } }

            [SerializeField]
            protected string iOS;
            public string IOS { get { return iOS; } }

            public string Current
            {
                get
                {
                    if(Application.isEditor)
                        return android;

                    switch (Application.platform)
                    {
                        case RuntimePlatform.Android:
                            return android;

                        case RuntimePlatform.IPhonePlayer:
                            return iOS;
                    }

                    Debug.LogWarning("No advertisment game id set for platform: " + Application.platform + ", returning empty string");
                    return string.Empty;
                }
            }
        }

        public ListenerProperty Listener { get; protected set; }
        public class ListenerProperty : IUnityAdsListener
        {
            public delegate void ReadyDelegate(string placementId);
            public event ReadyDelegate ReadyEvent;
            public void OnUnityAdsReady(string placementId)
            {
                ReadyEvent?.Invoke(placementId);
            }

            public delegate void StartDelegate(string placementId);
            public event StartDelegate StartEvent;
            public void OnUnityAdsDidStart(string placementId)
            {
                StartEvent?.Invoke(placementId);
            }

            public delegate void FinishDelegate(string placementId, ShowResult result);
            public event FinishDelegate FinishEvent;
            public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
            {
                FinishEvent?.Invoke(placementId, showResult);
            }

            public delegate void ErrorDelegate(string message);
            public event ErrorDelegate ErrorEvent;
            public void OnUnityAdsDidError(string message)
            {
                Debug.LogError("Ads Error: " + message);

                ErrorEvent?.Invoke(message);
            }
        }

        public AdsPlacementsCore Placements { get; protected set; }

        public class Module : Core.Module<AdsCore>
        {
            public AdsCore Ads => Reference;
        }

        public bool TestMode
        {
            get
            {
                if (Application.isEditor)
                    return true;

                return false;
            }
        }

        public override void Configure(Core reference)
        {
            base.Configure(reference);

            Advertisement.debugMode = true;

            Listener = new ListenerProperty();
            Advertisement.AddListener(Listener);

            Placements = this.GetDependancy<AdsPlacementsCore>();

            References.Configure(this);

            Advertisement.Initialize(gameID.Current, TestMode);
        }

        public override void Init()
        {
            base.Init();

            References.Init(this);
        }
    }
}