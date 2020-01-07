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
	public class PlayerScoreModificationPopup : MonoBehaviour
	{
		[SerializeField]
        protected GameObject prefab;
        public GameObject Prefab { get { return prefab; } }

        [SerializeField]
        protected Vector2 position;
        public Vector2 Position { get { return position; } }

        public Player Player => Level.Instance.Player.Instance;

        public PopupUITemplate Instance { get; protected set; }

        private void Start()
        {
            Player.Score.OnValueModified += ModificationCallback;
        }

        private void ModificationCallback(int value, int change)
        {
            if (Instance != null) Destroy(Instance.gameObject);

            Instance = PopupUITemplate.Create(prefab, transform);

            Instance.Set(GetSignText(change) + Mathf.Abs(change));
            Instance.transform.anchoredPosition = position;
            Instance.Animate();
        }

        public static string GetSignText(int value)
        {
            if (value > 0) return "+";

            if (value < 0) return "-";

            return "";
        }
    }
}