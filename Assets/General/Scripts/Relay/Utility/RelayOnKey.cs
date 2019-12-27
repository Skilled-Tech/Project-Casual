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
    [RequireComponent(typeof(Relay))]
	public class RelayOnKey : MonoBehaviour
	{
        public KeyCode key;

        Relay relay;

        void Awake()
        {
            relay = GetComponent<Relay>();
        }

        void Update()
        {
            if (Input.GetKeyDown(key))
                relay.Invoke();
        }
    }
}