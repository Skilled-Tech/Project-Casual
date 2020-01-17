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

        public TextValidator Validator { get; protected set; }

        public UIElement Element { get; protected set; }

        public Core Core => Core.Instance;

        public PopupUI Popup => Core.UI.Popup;

        public virtual void Configure()
        {
            Element = GetComponent<UIElement>();

            Validator = new TextValidator();
            Validator.Add(TextValidator.Defaults.NotEmpty);
        }
        public virtual void Init()
        {
            controls.Register(ConfirmCallback, DenyCallback);
        }

        public virtual void Show(string text, ResponseDelegate callback)
        {
            label.text = text;

            Clear();

            this.callback = callback;

            if (Element.IsOn == false) Element.Show();
        }
        public virtual void Hide() => Element.Hide();

        public virtual void Clear()
        {
            inputField.text = string.Empty;
        }

        #region Callback
        private void DenyCallback() => Action(null, true);
        private void ConfirmCallback()
        {
            if (Validator.Check(inputField.text))
                Action(inputField.text, false);
            else
                Popup.Show("Invalid Text", "Ok");
        }
        
        void Action(string result, bool canceled)
        {
            var response = new Response(result, canceled);

            Action(response);
        }
        void Action(Response response)
        {
            callback?.Invoke(response);
        }

        public delegate void ResponseDelegate(Response response);
        ResponseDelegate callback;

        public class Response
        {
            public string Text { get; protected set; }

            public bool Canceled { get; protected set; }

            public bool Success => Canceled == false;

            public Response(string text, bool canceled)
            {
                this.Text = text;

                this.Canceled = canceled;
            }
        }
        #endregion
    }
}