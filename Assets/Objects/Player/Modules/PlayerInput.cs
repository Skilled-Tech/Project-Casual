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
	public class PlayerInput : Player.Module
	{
		public Vector2 Movement { get; protected set; }

        public override void Init()
        {
            base.Init();

            Player.OnProcess += Process;
        }

        void Process()
        {
            ProcessSway();
        }

        void ProcessSway()
        {
            Movement = Player.Controls.Swipe.Vector;

#if UNITY_EDITOR
            Movement += Vector2.right * Input.GetAxisRaw("Horizontal");
#endif
        }
    }
}