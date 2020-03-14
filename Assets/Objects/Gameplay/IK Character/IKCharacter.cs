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

            [SerializeField]
            protected WeightData weight;
            public WeightData Weight { get { return weight; } }
            [Serializable]
            public class WeightData
            {
                [SerializeField]
                [Range(0f, 1f)]
                protected float position = 1f;
                public float Position { get { return position; } }

                [SerializeField]
                [Range(0f, 1f)]
                protected float rotation = 1f;
                public float Rotation { get { return rotation; } }
            }

            public virtual void Process(Animator animator)
            {
                animator.SetIKPositionWeight(goal, weight.Position);
                animator.SetIKPosition(goal, transform.position);

                animator.SetIKRotationWeight(goal, weight.Rotation);
                animator.SetIKRotation(goal, transform.rotation);
            }
        }

        void Awake()
        {
            Animator = GetComponent<Animator>();
        }

        void OnAnimatorIK(int layerIndex)
        {
            for (int i = 0; i < elements.Length; i++)
                elements[i].Process(Animator);
        }
    }
}