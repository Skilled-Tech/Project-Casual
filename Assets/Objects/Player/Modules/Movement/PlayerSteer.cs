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
	public class PlayerSteer : Player.Module, PlayerMovement.IInterface
	{
        [SerializeField]
        ForceData force = new ForceData(10f, 3f);
        [Serializable]
        public class ForceData
        {
            [SerializeField]
            protected float ground;
            public float Ground { get { return ground; } }

            [SerializeField]
            protected float air;
            public float Air { get { return air; } }

            public float Sample(bool isGrounded)
            {
                return isGrounded ? ground : air;
            }

            public ForceData(float ground, float air)
            {
                this.ground = ground;
                this.air = air;
            }
        }

        public Vector3 Calculate()
        {
            return Player.transform.right * Player.Input.Movement.x * force.Sample(Player.IsGrounded);
        }
    }
}