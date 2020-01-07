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
        protected LeaderboardProperty leaderboards;
        public LeaderboardProperty Leaderboards { get { return leaderboards; } }
        [Serializable]
        public class LeaderboardProperty : Property
        {
            [SerializeField]
            protected LeaderboardUI score;
            public LeaderboardUI Score { get { return score; } }

            public override void Init()
            {
                base.Init();

                score.Element.SetActive(false);
            }
        }

        [SerializeField]
        protected TextInputUI textInput;
        public TextInputUI TextInput { get { return textInput; } }

        [SerializeField]
        protected ChoiceUI choice;
        public ChoiceUI Choice { get { return choice; } }

        [SerializeField]
        protected PopupUI popup;
        public PopupUI Popup { get { return popup; } }

        [SerializeField]
        protected FaderUI fader;
        public FaderUI Fader { get { return fader; } }

        [Serializable]
        public class Property : Core.Property<UICore>
        {

        }

        public override void Configure(Core reference)
        {
            base.Configure(reference);

            Register(this, leaderboards);
        }

        public override void Init()
        {
            base.Init();

            optionsMenu.SetActive(false);

            textInput.Element.SetActive(false);
            textInput.Validator.Add(BadWordsFilter.IsClean);

            choice.Element.SetActive(false);

            popup.Element.SetActive(false);

            fader.Value = 0f;
        }
    }
}