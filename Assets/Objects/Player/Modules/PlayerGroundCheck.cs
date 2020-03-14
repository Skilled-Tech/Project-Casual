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
	public class PlayerGroundCheck : Player.Module
	{
        [SerializeField]
        float radius = 0.4f;

        [SerializeField]
        float range = 0.1f;

        [SerializeField]
        LayerMask mask = Physics.DefaultRaycastLayers;

        public const float offset = 0.1f;

        public Vector3 Origin => Player.transform.position + Direction * (Player.collider.bounds.extents.y - offset);

        public Vector3 Direction => Vector3.down;

        RaycastHit hit;
        public bool IsGrounded => hit.collider != null;

        public override void Init()
        {
            base.Init();

            Player.OnProcess += Process;
        }

        void Process()
        {
            if(Physics.Raycast(Origin, Direction, out hit, range + offset, mask))
            {

            }
            else
            {

            }

            Debug.DrawRay(Origin, Direction * (range + offset), Color.green);
        }
    }
}