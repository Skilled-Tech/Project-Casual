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
    [RequireComponent(typeof(UIElement))]
	public class LevelEndMenu : MonoBehaviour, IInitialize
	{
        [SerializeField]
        protected Text score;
        public Text Score { get { return score; } }

        [SerializeField]
        protected Relay leaderboard;
        public Relay Leaderboard { get { return leaderboard; } }

        [SerializeField]
        protected SelectableRelay ad;
        public SelectableRelay Ad { get { return ad; } }

        public UIElement Element { get; protected set; }

        public Level Level => Level.Instance;

        public Core Core => Core.Instance;

        public virtual void Configure()
        {
            Element = GetComponent<UIElement>();
        }
        public virtual void Init()
        {
            leaderboard.OnInvoke += LeaderboardAction;
            ad.OnInvoke += AdAction;
        }

        protected virtual void OnEnable()
        {
            Level.Player.Instance.Score.OnValueChange += PlayerScoreChangeCallback;

            UpdateState();
        }

        private void PlayerScoreChangeCallback(int value) => UpdateState();

        private void AdAction()
        {
            if(Core.Unity.Ads.Placements.Common.RewardedVideo.IsReady)
            {
                Core.Unity.Ads.Placements.Common.RewardedVideo.Show();
            }
            else
            {
                Core.UI.Popup.Show("No Ad Available, Please Try Again Later", "Ok");
            }
        }
        private void LeaderboardAction()
        {
            var leaderboard = Core.UI.Leaderboards.Score;

            Core.UI.Leaderboards.Score.Show();
        }

        public virtual void Show()
        {
            Element.Show();
        }

        protected virtual void UpdateState()
        {
            score.text = Level.Player.Instance.Score.Value.ToString("N0");
        }

        protected virtual void OnDisable()
        {
            Level.Player.Instance.Score.OnValueChange -= PlayerScoreChangeCallback;
        }
    }
}