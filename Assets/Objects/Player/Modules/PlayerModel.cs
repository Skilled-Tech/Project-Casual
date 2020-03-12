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

        [SerializeField]
        protected float maxVelocity = 14f;

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
            var magnitude = Player.rigidbody.velocity.magnitude;

            var speed = Mathf.LerpUnclamped(0f, 1f, magnitude / maxVelocity);

            Animator.SetFloat("Speed", speed);

            Animator.SetFloat("Speed Multiplier", Mathf.Clamp(speed, 1, 10));
        }
    }
}