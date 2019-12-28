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
	public class ReturnHook : MonoBehaviour
	{
        [SerializeField]
        protected bool use = true;
        public bool Use { get { return use; } }

        [SerializeField]
        protected Relay relay;
        public Relay Relay { get { return relay; } }

        public static List<ReturnHook> list = new List<ReturnHook>();

        private void OnEnable()
        {
            list.Add(this);
        }

        private void Update()
        {
            if(list[list.Count - 1] == this && use)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Home))
                    relay.Invoke();
            }
        }

        private void OnDisable()
        {
            list.Remove(this);
        }
    }
}