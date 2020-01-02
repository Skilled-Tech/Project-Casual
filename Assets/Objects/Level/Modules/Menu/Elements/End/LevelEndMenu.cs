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

        public UIElement Element { get; protected set; }

        public Core Core => Core.Instance;
        public Level Level => Level.Instance;

        public virtual void Configure()
        {
            Element = GetComponent<UIElement>();
        }
        public virtual void Init()
        {
            leaderboard.OnInvoke += LeaderboardAction;
        }

        private void LeaderboardAction()
        {
            var leaderboard = Core.UI.Leaderboards.Score;

            Core.UI.Leaderboards.Score.Show();
            Core.UI.Leaderboards.Score.Submit.Element.Show();
        }

        public virtual void Show()
        {
            score.text = Level.Player.Instance.Score.Value.ToString("N0");

            Element.Show();
        }
    }
}