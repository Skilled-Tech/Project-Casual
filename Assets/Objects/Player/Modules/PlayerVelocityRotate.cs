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
	public class PlayerVelocityRotate : Player.Module
	{
        [SerializeField]
        Vector3 axis = Vector3.right;

        [SerializeField]
        float speed = 10f;

        [SerializeField]
        Space space = Space.Self;

        public override void Init()
        {
            base.Init();

            Player.OnProcess += Process;
        }

        void Process()
        {
            transform.Rotate(axis * speed * Player.rigidbody.velocity.magnitude, space);
        }
    }
}