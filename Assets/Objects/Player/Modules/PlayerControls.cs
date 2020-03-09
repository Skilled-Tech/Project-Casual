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
    public class PlayerControls : Player.Module
    {
        public SwipeInputUI Swipe { get; protected set; }

        public ClickInputUI Click { get; protected set; }

        public override void Configure(Player reference)
        {
            base.Configure(reference);

            Swipe = FindObjectOfType<SwipeInputUI>();
            Click = FindObjectOfType<ClickInputUI>();
        }
    }
}