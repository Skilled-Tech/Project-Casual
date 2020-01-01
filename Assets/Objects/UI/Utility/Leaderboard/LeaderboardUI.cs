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
        protected ScrollRect scoll;
        public ScrollRect Scroll { get { return scoll; } }

        [SerializeField]
        protected UIElement emptyIndicator;
        public UIElement EmptyIndicator { get { return emptyIndicator; } }

        [SerializeField]
        protected RelayPanelUI submit;
        public RelayPanelUI Submit { get { return submit; } }

        #region Entries
        public List<LeaderboardUITemplate> Entries { get; protected set; }

        public bool HasEntries => Entries.Count > 0;

        protected virtual void ForAllEntries(Action<LeaderboardUITemplate, int> action)
        {
            for (int i = 0; i < Entries.Count; i++)
                action(Entries[i], i);
        }

        protected virtual void HideAllEntries()
        {
            ForAllEntries(Action);

            void Action(LeaderboardUITemplate template, int index) => template.Element.SetActive(false);
        }
        #endregion

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

        public virtual void Show()
        {
            Element.Show();

            UpdateState();

            StartCoroutine(Procedure());

            IEnumerator Procedure()
            {
                HideAllEntries();

                bool IsTransitionComplete() => Element.Transition == null ? true : Element.Transition.Value == 1f;

                yield return new WaitUntil(IsTransitionComplete);

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

        protected virtual void UpdateState()
        {
            emptyIndicator.IsOn = !HasEntries;

            Scroll.gameObject.SetActive(HasEntries);
        }

        private void UpdateCallback(LeaderboardModule result) => Create();

        protected virtual void Create()
        {
            Clear();

            Entries = LeaderboardUITemplate.Create(Leaderboard.List, template, scoll.content);

            /*
            Entries.AddRange(LeaderboardUITemplate.Create(Leaderboard.List, template, scoll.content));
            Entries.AddRange(LeaderboardUITemplate.Create(Leaderboard.List, template, scoll.content));
            Entries.AddRange(LeaderboardUITemplate.Create(Leaderboard.List, template, scoll.content));
            Entries.AddRange(LeaderboardUITemplate.Create(Leaderboard.List, template, scoll.content));
            Entries.AddRange(LeaderboardUITemplate.Create(Leaderboard.List, template, scoll.content));
            Entries.AddRange(LeaderboardUITemplate.Create(Leaderboard.List, template, scoll.content));
            Entries.AddRange(LeaderboardUITemplate.Create(Leaderboard.List, template, scoll.content));
            */

            UpdateState();

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