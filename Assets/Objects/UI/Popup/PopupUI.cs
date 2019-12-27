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
    public class PopupUI : UIElement
    {
        [SerializeField]
        protected Text label;
        public Text Label { get { return label; } }

        public string Text { get { return label.text; } set { label.text = value; } }

        [SerializeField]
        protected Button button;
        public Button Button { get { return button; } }

        [SerializeField]
        protected Text instructions;
        public Text Instructions { get { return instructions; } }

        [SerializeField]
        protected GameObject controls;
        public GameObject Controls { get { return controls; } }

        [SerializeField]
        protected GameObject loading;
        public GameObject Loading { get { return loading; } }

        protected virtual void Awake()
        {
            button.onClick.AddListener(OnButton);
        }

        public virtual void Show(string text)
        {
            Show(text, null, null);
        }
        public virtual void Show(string text, Action action, string instructions)
        {
            label.text = text;
            this.action = action;

            button.gameObject.SetActive(action != null);
            if(controls != null) controls.SetActive(action != null);

            if(loading != null) loading.gameObject.SetActive(action == null);

            if (action == null)
            {

            }
            else
            {
                this.instructions.text = instructions;
            }

            base.Show();
        }

        Action action;
        void OnButton()
        {
            if (action != null) action();
        }
    }
}