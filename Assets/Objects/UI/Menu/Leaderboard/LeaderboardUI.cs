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
        protected PrefabsData prefabs;
        public PrefabsData Prefabs { get { return prefabs; } }
        [Serializable]
        public class PrefabsData
        {
            [SerializeField]
            protected GameObject template;
            public GameObject Template { get { return template; } }

            [SerializeField]
            protected GameObject seperator;
            public GameObject Seperator { get { return seperator; } }
        }

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

        protected virtual void HideAllEntries()
        {
            Entries.ForEach(Action);

            void Action(LeaderboardUITemplate template) => template.Element.SetActive(false);
        }
        #endregion

        #region Elements
        public List<UIElement> Elements { get; protected set; }

        protected virtual void HideAllElements()
        {
            Elements.ForEach(Action);

            void Action(UIElement template) => template.SetActive(false);
        }
        #endregion

        public LeaderboardModule Leaderboard { get; protected set; }

        public UIElement Element { get; protected set; }

        public Core Core => Core.Instance;

        public virtual void Configure()
        {
            Leaderboard = Core.Leaderboards.Find(ID);
            if(Leaderboard == null)
            {
                Debug.LogError("No Leaderboard defined with ID: " + ID, gameObject);
                enabled = false;
                return;
            }

            Element = GetComponent<UIElement>();

            Entries = new List<LeaderboardUITemplate>();

            Elements = new List<UIElement>();

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
                HideAllElements();

                bool IsTransitionComplete() => Element.Transition == null ? true : Element.Transition.Value == 1f;

                yield return new WaitUntil(IsTransitionComplete);

                yield return ChainShow();
            }
        }

        Coroutine ChainShowCoroutine;
        Coroutine ChainShow()
        {
            if (ChainShowCoroutine != null) StopCoroutine(ChainShowCoroutine);

            ChainShowCoroutine = UIElement.Utility.ChainShow(Elements, this);

            return ChainShowCoroutine;
        }

        protected virtual void UpdateState()
        {
            Scroll.gameObject.SetActive(HasEntries);

            emptyIndicator.IsOn = !HasEntries;
        }

        private void UpdateCallback(LeaderboardModule result) => Create();

        protected virtual void Create()
        {
            Clear();

            foreach (var entry in Leaderboard.IEnumerate())
            {
                if (entry.Value == 0 && (entry.DisplayName == null || entry.DisplayName == string.Empty)) continue;

                if(Entries.Count > 0 && entry.Position != Entries.Last().Reference.Position + 1)
                {
                    Elements.Add(Seperator());
                }

                var instance = LeaderboardUITemplate.Create(entry, prefabs.Template, Scroll.content);

                Entries.Add(instance);
                Elements.Add(instance.Element);
            }

            UpdateState();

            if (Element.Visible) ChainShow();

            UIElement Seperator()
            {
                var instance = Instantiate(prefabs.Seperator, Scroll.content);

                instance.name = prefabs.Seperator.name;

                Initializer.Perform(instance);

                var element = instance.GetComponent<UIElement>();

                return element;
            }
        }

        protected virtual void Clear()
        {
            for (int i = 0; i < Elements.Count; i++)
                Destroy(Elements[i].gameObject);

            Elements.Clear();
            Entries.Clear();
        }

        protected virtual void OnDestroy()
        {
            Leaderboard.OnUpdate -= UpdateCallback;
        }
    }
}