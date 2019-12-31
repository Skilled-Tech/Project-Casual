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
    [RequireComponent(typeof(UIElement))]
    public class UITemplate : MonoBehaviour, IInitialize
    {
        public UIElement Element { get; protected set; }

        public virtual void Configure()
        {
            Element = GetComponent<UIElement>();

            Debug.Log(Element);
        }
        public virtual void Init()
        {

        }

        public static class Utility
        {
            public static Coroutine ChainDisplay<TTemplate>(IList<TTemplate> list, MonoBehaviour behaviour)
            where TTemplate : UITemplate
            {
                var elements = new UIElement[list.Count];

                for (int i = 0; i < list.Count; i++)
                    elements[i] = list[i].Element;

                return UIElement.Utility.ChainDisplay(elements, behaviour);
            }
        }
    }

    public abstract class UITemplate<TReference> : UITemplate
    {
        public TReference Reference { get; protected set; }
        public virtual void Set(TReference reference)
        {
            this.Reference = reference;

            UpdateState();
        }

        protected void UpdateState() => UpdateState(Reference);
        protected virtual void UpdateState(TReference reference)
        {

        }

        public static TTemplate Create<TTemplate>(TReference reference, GameObject prefab, Transform parent)
            where TTemplate : UITemplate<TReference>
        {
            var instance = Instantiate(prefab, parent);

            instance.name = prefab.name + " (Instance)";

            Initializer.Perform(instance);

            var script = instance.GetComponent<TTemplate>();

            script.Set(reference);

            return script;
        }
        public static List<TTemplate> Create<TTemplate>(IList<TReference> references, GameObject prefab, Transform parent)
            where TTemplate : UITemplate<TReference>
        {
            var list = new List<TTemplate>();

            for (int i = 0; i < references.Count; i++)
            {
                var instance = Create<TTemplate>(references[i], prefab, parent);

                list.Add(instance);
            }

            return list;
        }
    }

    public abstract class UITemplate<TReference, TTemplate> : UITemplate<TReference>
        where TTemplate : UITemplate<TReference>
    {
        public static TTemplate Create(TReference reference, GameObject prefab, Transform parent)
        {
            return Create<TTemplate>(reference, prefab, parent);
        }
        public static List<TTemplate> Create(IList<TReference> references, GameObject prefab, Transform parent)
        {
            return Create<TTemplate>(references, prefab, parent);
        }
    }
}