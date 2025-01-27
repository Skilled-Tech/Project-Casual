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
	public class PlayerJump : Player.Module
	{
        [SerializeField]
        float force = 5;

        [SerializeField]
        Vector3 direction = Vector3.up;

        public virtual void Perform() => Perform(1f);
        public virtual void Perform(float multiplier)
        {
            Player.rigidbody.AddForce(direction * (force * multiplier), ForceMode.VelocityChange);
        }
    }
}