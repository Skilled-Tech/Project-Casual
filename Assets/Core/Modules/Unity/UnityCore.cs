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
    public class UnityCore : Core.Module
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
                    if (Application.isEditor)
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

        public UnityAdsCore Ads { get; protected set; }

        public UnityIAPCore IAP { get; protected set; }

        public class Module : Core.Module<UnityCore>
        {
            public UnityCore Unity => Reference;
        }

        public override void Configure(Core reference)
        {
            base.Configure(reference);

            Ads = this.GetDependancy<UnityAdsCore>();
            IAP = this.GetDependancy<UnityIAPCore>();

            References.Configure(this);
        }

        public override void Init()
        {
            base.Init();

            References.Init(this);
        }
    }
}