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
        [SerializeField]
        float speed = 240f;

        public override void Init()
        {
            base.Init();

            Player.OnProcess += Process;
        }

        void Process()
        {
            var direction = Player.Movement.Velocity;
            direction.y = 0f;

            if(direction.magnitude > 0f)
            {
                var target = Quaternion.LookRotation(direction);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, speed * Time.deltaTime);
            }
        }
    }
}