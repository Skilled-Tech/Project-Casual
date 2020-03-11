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
    [RequireComponent(typeof(Animator))]
	public class PlayerModel : Player.Module
	{
		public Animator Animator { get; protected set; }

        public override void Configure(Player reference)
        {
            base.Configure(reference);

            Animator = GetComponent<Animator>();
        }

        public override void Init()
        {
            base.Init();

            Player.OnProcess += Process;
        }

        private void Process()
        {
            var speed = Player.rigidbody.velocity.magnitude;

            Animator.SetFloat("Speed", 2f);

            Animator.SetFloat("Speed Multiplier", speed / 8f);
        }
    }
}