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
    [RequireComponent(typeof(Image))]
    public class FaderUI : MonoBehaviour, IInitialize
    {
		public Image Image { get; protected set; }

        public float Value
        {
            get
            {
                return Image.color.a;
            }
            set
            {
                var color = Image.color;
                color.a = value;
                Image.color = color;

                Image.raycastTarget = value > 0f;
            }
        }

        public virtual void Configure()
        {
            Image = GetComponent<Image>();
        }
        public virtual void Init()
        {
            if(gameObject.activeSelf == false)
            {
                gameObject.SetActive(true);
                Value = 0f;
            }
        }

        public virtual Coroutine To(float target) => To(target, 0.4f);
        public virtual Coroutine To(float target, float duration)
        {
            if(duration == 0f)
            {
                Value = target;
            }

            IEnumerator Procedure()
            {
                var timer = duration;

                var initialValue = Value;

                while (timer > 0f)
                {
                    timer = Mathf.MoveTowards(timer, 0f, Time.unscaledDeltaTime);

                    Value = Mathf.Lerp(target, initialValue, timer / duration);

                    yield return new WaitForEndOfFrame();
                }
            }

            return StartCoroutine(Procedure());
        }
    }
}