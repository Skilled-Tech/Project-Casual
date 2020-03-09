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
    public class ClickInputUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IBeginDragHandler
    {
        public int? PointerID { get; protected set; }

        public bool IsDown => PointerID.HasValue;

        public event Action OnDown;
        public void OnPointerDown(PointerEventData eventData)
        {
            if(PointerID == null)
            {
                PointerID = eventData.pointerId;

                OnDown?.Invoke();
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

        public event Action OnClick;
        public void OnPointerClick(PointerEventData eventData)
        {
            if(Drag)
            {
                Drag = false;
            }
            else
            {
                OnClick?.Invoke();
            }
        }

        public bool Drag { get; protected set; }
        public void OnBeginDrag(PointerEventData eventData)
        {
            Drag = true;
        }
    }
}