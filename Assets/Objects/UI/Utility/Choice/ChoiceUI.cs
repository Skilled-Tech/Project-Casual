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
    public class ChoiceUI : MonoBehaviour, IInitialize
    {
        [SerializeField]
        protected Text label;
        public Text Label { get { return label; } }

        public string Text { get { return label.text; } set { label.text = value; } }

        [SerializeField]
        protected ControlsProperty controls;
        public ControlsProperty Controls { get { return controls; } }
        [Serializable]
        public class ControlsProperty
        {
            [SerializeField]
            protected Element confirm;
            public Element Confirm { get { return confirm; } }

            [SerializeField]
            protected Element deny;
            public Element Deny { get { return deny; } }

            [Serializable]
            public class Element
            {
                [SerializeField]
                protected Relay relay;
                public Relay Relay { get { return relay; } }

                [SerializeField]
                protected Text label;
                public Text Label { get { return label; } }

                public string Text { get { return label.text; } set { label.text = value; } }
            }
        }

        public UIElement Element { get; protected set; }

        public virtual void Configure()
        {
            Element = GetComponent<UIElement>();

            controls.Confirm.Relay.OnInvoke += ConfirmCallback;
            controls.Deny.Relay.OnInvoke += DenyCallback;
        }
        public virtual void Init()
        {

        }

        public virtual void Show(string text, ResultDelegate callback)
        {
            label.text = text;

            this.callback = callback;

            Element.Show();
        }

        public virtual void Hide() => Element.Hide();

        #region Callback
        private void DenyCallback() => Action(false);
        private void ConfirmCallback() => Action(true);

        public delegate void ResultDelegate(bool result);
        ResultDelegate callback;
        void Action(bool result)
        {
            callback?.Invoke(result);
        }
        #endregion
    }
}