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
        protected string _ID;
        public string ID { get { return _ID; } }

        [SerializeField]
        protected GameObject template;
        public GameObject Template { get { return template; } }

        [SerializeField]
        protected RectTransform panel;
        public RectTransform Panel { get { return panel; } }

        public List<LeaderboardUITemplate> Entries { get; protected set; }

        public LeaderboardModule Leaderboard { get; protected set; }

        public UIElement Element { get; protected set; }

        public Core Core => Core.Instance;

        public virtual void Configure()
        {
            Element = GetComponent<UIElement>();

            Leaderboard = Core.Leaderboards.Find(ID);
            if(Leaderboard == null)
            {
                Debug.LogError("No Leaderboard defined with ID: " + ID, gameObject);
                enabled = false;
                return;
            }

            Entries = new List<LeaderboardUITemplate>();

            Leaderboard.OnUpdate += UpdateCallback;
        }
        public virtual void Init()
        {
            
        }

        protected virtual void OnEnable()
        {
            StartCoroutine(Procedure());

            IEnumerator Procedure()
            {
                yield return new WaitForEndOfFrame();

                for (int i = 0; i < Entries.Count; i++) Entries[i].Element.SetActive(false);

                yield return new WaitForSeconds(0.2f);

                yield return ChainShow();
            }
        }

        Coroutine ChainShowCoroutine;
        Coroutine ChainShow()
        {
            if (ChainShowCoroutine != null) StopCoroutine(ChainShowCoroutine);

            ChainShowCoroutine = UITemplate.Utility.ChainShow(Entries, this);

            return ChainShowCoroutine;
        }

        private void UpdateCallback(LeaderboardModule result) => UpdateState();
        protected virtual void UpdateState()
        {
            Clear();

            Create();
        }

        protected virtual void Create()
        {
            Entries = LeaderboardUITemplate.Create(Leaderboard.List, template, panel);
            Entries.AddRange(LeaderboardUITemplate.Create(Leaderboard.List, template, panel));
            Entries.AddRange(LeaderboardUITemplate.Create(Leaderboard.List, template, panel));
            Entries.AddRange(LeaderboardUITemplate.Create(Leaderboard.List, template, panel));
            Entries.AddRange(LeaderboardUITemplate.Create(Leaderboard.List, template, panel));
            Entries.AddRange(LeaderboardUITemplate.Create(Leaderboard.List, template, panel));
            Entries.AddRange(LeaderboardUITemplate.Create(Leaderboard.List, template, panel));
            Entries.AddRange(LeaderboardUITemplate.Create(Leaderboard.List, template, panel));

            if (Element.Visible)
            {
                ChainShow();
            }
        }

        protected virtual void Clear()
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                Destroy(Entries[i].gameObject);
            }

            Entries.Clear();
        }

        protected virtual void OnDestroy()
        {
            Leaderboard.OnUpdate -= UpdateCallback;
        }
    }
}