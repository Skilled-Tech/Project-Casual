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
        float force = 5f;

        public Vector3 Calculate()
        {
            return Player.transform.right * Player.Controls.Swipe.Vector.x * this.force;
        }
    }
}