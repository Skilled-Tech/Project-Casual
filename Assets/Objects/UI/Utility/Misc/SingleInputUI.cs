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
    public class SingleInputUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public int? PointerID { get; protected set; }

        public bool IsDown => PointerID.HasValue;

        public event Action OnClick;
        public void OnPointerDown(PointerEventData eventData)
        {
            if(PointerID == null)
            {
                PointerID = eventData.pointerId;

                OnClick?.Invoke();
            }
        }

        public event Action OnRelease;
        public void OnPointerUp(PointerEventData eventData)
        {
            if(PointerID == eventData.pointerId)
            {
                PointerID = null;

                OnRelease?.Invoke();
            }
        }
    }
}