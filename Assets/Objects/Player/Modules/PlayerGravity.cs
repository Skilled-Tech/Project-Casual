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
	public class PlayerGravity : Player.Module
	{
        [SerializeField]
        float force = 9.8f;

        public override void Init()
        {
            base.Init();

            Player.OnProcess += Process;
        }

        private void Process()
        {
            Player.rigidbody.AddForce(Vector3.down * force, ForceMode.Acceleration);
        }
    }
}