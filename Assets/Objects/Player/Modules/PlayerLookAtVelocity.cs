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
	public class PlayerLookAtVelocity : Player.Module
	{
        public override void Init()
        {
            base.Init();

            Player.OnProcess += Process;
        }

        void Process()
        {
            var direction = Player.rigidbody.velocity;
            direction.y = 0f;

            var rotation = Quaternion.LookRotation(direction);

            transform.rotation = rotation;
        }
    }
}