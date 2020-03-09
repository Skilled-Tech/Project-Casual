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

using UnityEngine.EventSystems;

namespace Game
{
    [RequireComponent(typeof(RectTransform))]
    public class SwipeInputUI : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        Vector2 sensitivity = new Vector2(4f, 4f);

        public Vector2 Vector { get; protected set; }

        public Vector2 Delta { get; protected set; }

        public int? PointerID { get; protected set; }

        public RectTransform RectTransform { get; protected set; }

        protected virtual void Awake()
        {
            RectTransform = transform as RectTransform;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (PointerID == null)
                PointerID = eventData.pointerId;

            if(PointerID == eventData.pointerId)
            {
                Process(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == PointerID)
            {
                PointerID = null;

                Delta = Vector2.zero;
                Vector = Vector2.zero;
            }
        }

        protected virtual void Process(PointerEventData eventData)
        {
            Delta = eventData.position - eventData.pressPosition;

            Vector = new Vector2()
            {
                x = Delta.x / (RectTransform.rect.width / sensitivity.x),
                y = Delta.y / (RectTransform.rect.height / sensitivity.y)
            };
        }
    }
}