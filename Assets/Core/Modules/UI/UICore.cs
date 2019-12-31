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
	public class UICore : Core.Module
	{
		[SerializeField]
        protected UIElement optionsMenu;
        public UIElement OptionsMenu { get { return optionsMenu; } }

        [SerializeField]
        protected UIElement leaderboards;
        public UIElement Leaderboards { get { return leaderboards; } }

        [SerializeField]
        protected ChoiceUI choice;
        public ChoiceUI Choice { get { return choice; } }

        [SerializeField]
        protected PopupUI popup;
        public PopupUI Popup { get { return popup; } }

        [SerializeField]
        protected FaderUI fader;
        public FaderUI Fader { get { return fader; } }

        public override void Init()
        {
            base.Init();

            if (optionsMenu.IsOn) optionsMenu.SetActive(false);

            if (leaderboards.IsOn) leaderboards.SetActive(false);

            if (choice.Element.IsOn) choice.Element.SetActive(false);

            if(popup.Element.IsOn) popup.Element.SetActive(false);

            fader.Value = 0f;
        }
    }
}