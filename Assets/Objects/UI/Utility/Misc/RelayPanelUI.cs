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
	public class RelayPanelUI : MonoBehaviour, IInitialize
	{
		[SerializeField]
        protected Relay relay;
        public Relay Relay { get { return relay; } }

        public event Action OnInvoke
        {
            add => relay.OnInvoke += value;
            remove => relay.OnInvoke -= value;
        }

        public UIElement Element { get; protected set; }

        public virtual void Configure()
        {
            Element = GetComponent<UIElement>();
        }

        public virtual void Init()
        {
            
        }
    }
}