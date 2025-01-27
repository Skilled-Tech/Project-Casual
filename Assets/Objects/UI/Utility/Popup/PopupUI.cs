﻿using System;
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
    public class PopupUI : MonoBehaviour, IInitialize
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
        protected UIElement controls;
        public UIElement Controls { get { return controls; } }

        [SerializeField]
        protected UIElement loading;
        public UIElement Loading { get { return loading; } }

        public UIElement Element { get; protected set; }

        public virtual void Configure()
        {
            Element = GetComponent<UIElement>();

            button.onClick.AddListener(Action);
        }
        public virtual void Init()
        {

        }

        public virtual void Lock(string text) => Show(text, null, null);

        public virtual void Show(string text) => Show(text, "Okay");
        public virtual void Show(string text, string instructions) => Show(text, null, instructions);

        public virtual void Show(string text, Action callback, string instructions)
        {
            label.text = text;
            this.callback = callback;

            button.gameObject.SetActive(instructions != null);

            if(controls != null) controls.IsOn = instructions != null;

            if(loading != null) loading.IsOn = instructions == null;

            if (string.IsNullOrEmpty(instructions))
            {

            }
            else
            {
                this.instructions.text = instructions;
            }

            if(Element.IsOn == false) Element.Show();
        }

        public virtual void Hide() => Element.Hide();

        Action callback;
        void Action()
        {
            Hide();

            callback?.Invoke();
        }
    }
}