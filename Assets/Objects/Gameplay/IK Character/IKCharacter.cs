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
    [RequireComponent(typeof(Animator))]
	public class IKCharacter : MonoBehaviour
	{
        public Animator Animator { get; protected set; }

        [SerializeField]
        ElementData[] elements = new ElementData[] { };
        [Serializable]
        public class ElementData
        {
            [SerializeField]
            protected AvatarIKGoal goal;
            public AvatarIKGoal Goal { get { return goal; } }

            [SerializeField]
            protected Transform transform;
            public Transform Transform { get { return transform; } }
        }

        void Awake()
        {
            Animator = GetComponent<Animator>();
        }

        void OnAnimatorIK(int layerIndex)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                Animator.SetIKPositionWeight(elements[i].Goal, 1f);
                Animator.SetIKPosition(elements[i].Goal, elements[i].Transform.position);
            }
        }
    }
}