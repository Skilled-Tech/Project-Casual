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
	public class Pickup : MonoBehaviour
	{
        public Level Level => Level.Instance;

        public delegate void ActionDelegate(Player player);
        public event ActionDelegate OnAction;
        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody == null) return;

            var player = other.attachedRigidbody.GetComponent<Player>();

            OnAction?.Invoke(player);
        }
    }
}