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

        public Vector3 Velocity
        {
            get => Player.rigidbody.velocity;
            protected set => Player.rigidbody.velocity = value;
        }
        public Vector3 DirectionalVelocity
        {
            get
            {
                var value = Velocity;

                value.y = 0f;

                return value;
            }
            set
            {
                value.y = Velocity.y;

                Velocity = value;
            }
        }

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

            DirectionalVelocity = Vector3.MoveTowards(DirectionalVelocity, Target, acceleration * Time.deltaTime);

            Debug.DrawRay(Player.transform.position, Target, Color.yellow);
            Debug.DrawRay(Player.transform.position, DirectionalVelocity, Color.green);
        }
    }
}