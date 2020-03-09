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
	public class PlayerJump : Player.Module
	{
        [SerializeField]
        float force = 5;

        [SerializeField]
        Vector3 direction = Vector3.up;

        public override void Init()
        {
            base.Init();

            Player.Controls.Click.OnClick += ClickCallback;
        }

        private void ClickCallback()
        {
            Player.rigidbody.AddForce(direction * force, ForceMode.VelocityChange);
        }
    }
}