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
	public class PlayerDrive : Player.Module, PlayerMovement.IInterface
	{
        [SerializeField]
        float speed = 5f;
        public float Speed => speed;

        public Vector3 Calculate()
        {
            return transform.forward * speed;
        }
    }
}