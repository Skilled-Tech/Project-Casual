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
	public class PhysicsRewind : MonoBehaviour
	{
        public event Action<Collider> TriggerEnterEvent;
        private void OnTriggerEnter(Collider other)
        {
            TriggerEnterEvent?.Invoke(other);
        }

        public event Action<Collider> TriggerExitEvent;
        private void OnTriggerExit(Collider other)
        {
            TriggerExitEvent?.Invoke(other);
        }
    }
}