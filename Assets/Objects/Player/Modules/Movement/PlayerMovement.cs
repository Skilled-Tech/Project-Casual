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
	public class PlayerMovement : Player.Module
	{
        [SerializeField]
        protected float acceleration = 15;

        public List<IInterface> Interfaces { get; protected set; }
        public interface IInterface
        {
            Vector3 Calculate();
        }

        public Vector3 Target { get; protected set; }

        public Vector3 Velocity => Player.rigidbody.velocity;

        public PlayerDrive Drive { get; protected set; }

        public PlayerSteer Steer { get; protected set; }

        public override void Configure(Player reference)
        {
            base.Configure(reference);

            Interfaces = Player.GetAllDependancies<IInterface>();

            Drive = Player.GetDependancy<PlayerDrive>();

            Steer = Player.GetDependancy<PlayerSteer>();
        }

        public override void Init()
        {
            base.Init();

            Player.OnProcess += Process;
        }

        private void Process()
        {
            Target = Vector3.zero;

            for (int i = 0; i < Interfaces.Count; i++)
                Target += Interfaces[i].Calculate();

            var velocity = Player.rigidbody.velocity;
            velocity.y = 0f;
            {
                velocity = Vector3.MoveTowards(velocity, Target, acceleration * Time.deltaTime);

                Debug.DrawRay(Player.transform.position, Target, Color.yellow);
                Debug.DrawRay(Player.transform.position, velocity, Color.green);
            }
            velocity.y = Player.rigidbody.velocity.y;
            Player.rigidbody.velocity = velocity;
        }
    }
}