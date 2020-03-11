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
	public class JumpPad : MonoBehaviour
	{
        [SerializeField]
        float multiplier = 1f;

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.attachedRigidbody == null) return;

            var player = collider.attachedRigidbody.GetComponent<Player>();

            if (player != null) player.Jump.Perform(multiplier);
        }
    }
}