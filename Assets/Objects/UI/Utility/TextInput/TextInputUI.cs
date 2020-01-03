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
    public class TextInputUI : MonoBehaviour, IInitialize
    {
        [SerializeField]
        protected Text label;
        public Text Label { get { return label; } }
        public string Text { get { return label.text; } set { label.text = value; } }

        [SerializeField]
        protected InputField inputField;
        public InputField InputField { get { return inputField; } }
        public InputField.ContentType ContentType
        {
            get => inputField.contentType;
            set => inputField.contentType = value;
        }

        [SerializeField]
        protected ChoiceUI.ControlsProperty controls;
        public ChoiceUI.ControlsProperty Controls { get { return controls; } }

        public StringValidator Validator { get; protected set; }

        public UIElement Element { get; protected set; }

        public Core Core => Core.Instance;

        public PopupUI Popup => Core.UI.Popup;

        public virtual void Configure()
        {
            Element = GetComponent<UIElement>();

            Validator = new StringValidator();
            Validator.Add(StringValidator.Entries.NotEmpty);
        }
        public virtual void Init()
        {
            controls.Register(ConfirmCallback, DenyCallback);
        }

        public virtual void Show(string text, ResultDelegate callback)
        {
            label.text = text;

            this.callback = callback;

            if (Element.IsOn == false) Element.Show();
        }
        public virtual void Hide() => Element.Hide();

        #region Callback
        private void DenyCallback() => Action(null);
        private void ConfirmCallback()
        {
            if(Validator.Check(inputField.text))
            {
                Action(inputField.text);
            }
            else
            {
                Popup.Show("Invalid Text", "Ok");
            }
        }

        public delegate void ResultDelegate(string result);
        ResultDelegate callback;
        void Action(string result)
        {
            callback?.Invoke(result);
        }
        #endregion
    }

    [Serializable]
    public class StringValidator
    {
        public delegate bool Delegate(string input);

        public List<Delegate> List { get; protected set; }

        public virtual void Add(Delegate validator)
        {
            List.Add(validator);
        }
        public virtual void Remove(Delegate validator)
        {
            List.Remove(validator);
        }

        public virtual bool Check(string input)
        {
            for (int i = 0; i < List.Count; i++)
                if (List[i](input) == false)
                    return false;

            return true;
        }

        public static class Entries
        {
            public static bool NotEmpty(string input) => !string.IsNullOrEmpty(input);
        }

        public StringValidator()
        {
            List = new List<Delegate>();
        }
    }
}