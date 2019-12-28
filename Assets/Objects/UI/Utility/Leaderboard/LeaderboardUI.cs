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

using PlayFab.ClientModels;

namespace Game
{
    [RequireComponent(typeof(UIElement))]
    public class LeaderboardUI : MonoBehaviour, IInitialize
    {
        [SerializeField]
        protected GameObject template;
        public GameObject Template { get { return template; } }

        [SerializeField]
        protected RectTransform panel;
        public RectTransform Panel { get { return panel; } }

        public UIElement Element { get; protected set; }

        public virtual void Configure()
        {
            Element = GetComponent<UIElement>();
        }
        public virtual void Init()
        {

        }

        private void Start()
        {
            var entries = new PlayerLeaderboardEntry[20];

            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new PlayerLeaderboardEntry()
                {
                    DisplayName = System.Guid.NewGuid().ToString().Substring(0, 6),
                    Position = i,
                    StatValue = Mathf.RoundToInt(entries.Length / 1f / (i + 1f) * 1000),
                };
            }

            var elements = LeaderboardUITemplate.Create(entries, template, panel);
        }
    }
}