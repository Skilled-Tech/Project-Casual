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

            if (optionsMenu.Visible) optionsMenu.Hide();

            if (choice.Visible) choice.Hide();

            if(popup.Visible) popup.Hide();

            if(fader.Visible == false) fader.Show();
            fader.Value = 0f;
        }
    }
}