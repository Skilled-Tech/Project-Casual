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
	public class Crusher : MonoBehaviour
	{
        [SerializeField]
        AnimationCurve curve = new AnimationCurve();

        [SerializeField]
        float speed = 1f;

        [SerializeField]
        float distance = 1f;

        [SerializeField]
        Rigidbody platform = null;

        [SerializeField]
        Vector3 axis = Vector3.up;

        [SerializeField]
        float time = 0f;

        public Vector3 Offset { get; protected set; }

        void Update()
        {
            platform.position -= Offset;

            time += speed * Time.deltaTime;

            Offset = axis * curve.Evaluate(time) * distance;

            platform.position += Offset;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = platform.transform.localToWorldMatrix;

            Gizmos.DrawLine(Vector3.zero, axis * distance - Offset);
            Gizmos.DrawSphere(axis * distance - Offset, 0.2f);
        }
    }
}