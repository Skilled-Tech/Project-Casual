﻿using System;
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
	public class PlayerMovement : Player.Module
	{
        [SerializeField]
        protected float acceleration = 15;

        public List<IInterface> Interfaces { get; protected set; }
        public interface IInterface
        {
            Vector3 Calculate();
        }

        public override void Configure(Player reference)
        {
            base.Configure(reference);

            Interfaces = Player.GetAllDependancies<IInterface>();
        }

        public override void Init()
        {
            base.Init();

            Player.OnProcess += Process;
        }

        private void Process()
        {
            var target = Vector3.zero;

            for (int i = 0; i < Interfaces.Count; i++)
                target += Interfaces[i].Calculate();

            var velocity = Player.rigidbody.velocity;
            velocity.y = 0f;
            {
                velocity = Vector3.MoveTowards(velocity, target, acceleration * Time.deltaTime);

                Debug.DrawRay(Player.transform.position, target, Color.yellow);
                Debug.DrawRay(Player.transform.position, velocity, Color.green);
            }
            velocity.y = Player.rigidbody.velocity.y;
            Player.rigidbody.velocity = velocity;
        }
    }
}