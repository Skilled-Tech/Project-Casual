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
    [RequireComponent(typeof(Button))]
	public class NewsButton : MonoBehaviour, IInitialize
	{
        [SerializeField]
        protected BellUI notification;
        public BellUI Notification { get { return notification; } }

        public Button Button { get; protected set; }

        public Core Core => Core.Instance;

        public virtual void Configure()
        {
            Button = GetComponent<Button>();
            Button.onClick.AddListener(Action);
        }
        public virtual void Init()
        {
            notification.Element.Hide();

            Core.News.OnUpdate.Add(CoreUpdateCallback);
            Core.UI.News.Element.OnShow += ElementShowCallback;
        }

        protected virtual void OnEnable()
        {
            StartCoroutine(Procedure());

            IEnumerator Procedure()
            {
                yield return new WaitForEndOfFrame();

                UpdateState();
            }
        }

        void Action()
        {
            if(Core.News.Reports.Count > 0)
            {
                Core.UI.News.Show();
            }
            else
            {
                Core.UI.Popup.Show("No News To See Now");
            }
        }

        void UpdateState()
        {
            if (Core.UI.News.Element.Visible)
            {
                if (notification.IsOn)
                    notification.Hide();
            }
            else
            {
                if (Core.News.ContainsUnseenReports)
                    notification.Show();
                else
                    notification.Hide();
            }
        }

        private void CoreUpdateCallback()
        {
            UpdateState();
        }
        private void ElementShowCallback()
        {
            UpdateState();
        }
    }
}