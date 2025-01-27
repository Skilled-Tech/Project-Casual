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
    public class Transition : MonoBehaviour, IInitialize
    {
        [SerializeField]
        protected float speed = 4f;
        public float Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
            }
        }

        protected float _value = 0f;
        public virtual float Value
        {
            get
            {
                return _value;
            }
            set
            {
                value = Mathf.Clamp01(value);

                _value = value;

                OnValueChange?.Invoke(Value);
            }
        }

        public delegate void ValueChangeDelegate(float value);
        public event ValueChangeDelegate OnValueChange;

        public class Module : MonoBehaviour, IReference<Transition>
        {
            public Transition Transition { get; protected set; }

            public virtual void Configure(Transition reference)
            {
                Transition = reference;
            }

            public virtual void Init()
            {
                Transition.OnValueChange += ValueChangeCallback;

                UpdateState(Transition.Value);
            }

            protected virtual void UpdateState(float value)
            {

            }

            private void ValueChangeCallback(float value) => UpdateState(value);

            protected virtual void OnDestroy()
            {
                Transition.OnValueChange -= ValueChangeCallback;
            }
        }

        public virtual void Configure()
        {
            References.Configure(this);
        }
        public virtual void Init()
        {
            References.Init(this);
        }

        protected Coroutine coroutine;
        public bool InProcess => coroutine != null;

        public delegate void PerformDelegate(float target);
        public event PerformDelegate OnPerform;

        public virtual void Perform(float target)
        {
            if (coroutine != null) StopCoroutine(coroutine);

            if(gameObject.activeInHierarchy)
            {
                coroutine = StartCoroutine(Procedure(target));
            }
            else
            {
                Value = target;
            }

            OnPerform?.Invoke(target);
        }
        protected virtual IEnumerator Procedure(float target)
        {
            while (Value != target)
            {
                Value = Mathf.MoveTowards(Value, target, speed * Time.unscaledDeltaTime);

                yield return new WaitForEndOfFrame();
            }

            coroutine = null;
        }
    }
}