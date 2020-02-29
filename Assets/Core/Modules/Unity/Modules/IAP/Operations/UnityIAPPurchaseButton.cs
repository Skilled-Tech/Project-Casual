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

using UnityEngine.Purchasing;

namespace Game
{
    [RequireComponent(typeof(Button))]
	public class UnityIAPPurchaseButton : MonoBehaviour
	{
        [SerializeField]
        protected string _ID;
        public string ID { get { return _ID; } }

        Button button;

        public bool Interactable
        {
            get => button.interactable;
            set => button.interactable = value;
        }

        public Core Core => Core.Instance;

        protected virtual void Awake()
        {
            button = GetComponent<Button>();
        }

        protected virtual void Start()
        {
            button.onClick.AddListener(ClickAction);

            Core.Unity.IAP.OnInitialize += IAPInitializeCallback;

            UpdateState();
        }

        private void IAPInitializeCallback(IStoreController controller, IExtensionProvider extensions) => UpdateState();

        protected virtual void UpdateState()
        {
            Interactable = Core.Unity.IAP.Active;
        }

        protected virtual void ClickAction()
        {
            if (Core.Unity.IAP.Active)
                Core.Unity.IAP.Purchase(ID);
            else
                Core.UI.Popup.Show("Purchase Not Available at the Moment." + Environment.NewLine + "Please Try Again Later");
        }

        protected virtual void OnDestroy()
        {
            Core.Unity.IAP.OnInitialize -= IAPInitializeCallback;
        }
    }
}