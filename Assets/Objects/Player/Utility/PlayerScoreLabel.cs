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
    [RequireComponent(typeof(Text))]
	public class PlayerScoreLabel : MonoBehaviour
	{
        Text text;

        public Player Player => Level.Instance.Player.Instance;

        private void Start()
        {
            text = GetComponent<Text>();

            Player.Score.OnValueChange += ChangeCallback;

            UpdateState();
        }

        private void ChangeCallback(int value) => UpdateState();

        void UpdateState()
        {
            text.text = Player.Score.Value.ToString("N0");
        }
    }
}