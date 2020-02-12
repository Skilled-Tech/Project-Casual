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
	public class NewsReportLinkUITemplate : UITemplate<NewsReport.LinkData, NewsReportLinkUITemplate>
	{
        [SerializeField]
        protected Text title;
        public Text Title { get { return title; } }

        public Button Button { get; protected set; }

        public override void Configure()
        {
            base.Configure();

            Button = GetComponent<Button>();
        }

        public override void Init()
        {
            base.Init();

            Button.onClick.AddListener(Action);
        }

        protected override void UpdateState(NewsReport.LinkData reference)
        {
            base.UpdateState(reference);

            gameObject.name = reference.Title;

            title.text = reference.Title;
        }

        protected virtual void Action()
        {
            Application.OpenURL(Reference.URL);
        }
    }
}