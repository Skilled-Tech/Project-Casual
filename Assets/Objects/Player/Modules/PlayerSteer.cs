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
	public class PlayerSteer : Player.Module
	{
        [SerializeField]
        float maximum = 5f;

        [SerializeField]
        float acceleration = 5f;

        public override void Init()
        {
            base.Init();

            Player.OnProcess += Process;
        }

        private void Process()
        {
            var velocity = Player.rigidbody.velocity;
            {
                var target = Player.Controls.Swipe.Vector.x * maximum;

                velocity.x = Mathf.MoveTowards(velocity.x, target, acceleration * Time.deltaTime);
            }
            Player.rigidbody.velocity = velocity;
        }
    }
}