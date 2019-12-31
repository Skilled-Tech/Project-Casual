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
    public static partial class GameTools
    {
        public static class Audio
        {
            public static float LinearToDecibel(float linear)
            {
                if (linear == 0)
                    return -144.0f;
                else
                    return Mathf.Log10(linear) * 20.0f;
            }

            public static float DecibelToLinear(float dB)
            {
                return Mathf.Pow(10.0f, dB / 20.0f);
            }
        }

        public static class Layer
        {
            public static void Set(GameObject gameObject, int layer)
            {
                Set(gameObject.transform, layer);
            }
            public static void Set(Transform transform, int layer)
            {
                transform.gameObject.layer = layer;

                for (int i = 0; i < transform.childCount; i++)
                    Set(transform.GetChild(i), layer);
            }
        }

        public static class SortingLayer
        {
            public static void Set(GameObject gameObject, string layer)
            {
                Set(gameObject.transform, layer);
            }
            public static void Set(Transform transform, string layer)
            {
                var renderes = Dependancy.GetAll<Renderer>(transform.gameObject);

                for (int i = 0; i < renderes.Count; i++)
                    renderes[i].sortingLayerName = layer;
            }
        }

        public static class Bounds
        {
            public static UnityEngine.Bounds FromRenderer(GameObject gameObject)
            {
                UnityEngine.Bounds Func(Renderer component) { return component.bounds; }

                return Calculate<Renderer>(gameObject, Func);
            }
            public static UnityEngine.Bounds FromCollider3D(GameObject gameObject)
            {
                UnityEngine.Bounds Func(Collider component) { return component.bounds; }

                return Calculate<Collider>(gameObject, Func);
            }
            public static UnityEngine.Bounds FromCollider2D(GameObject gameObject)
            {
                UnityEngine.Bounds Func(Collider2D component) { return component.bounds; }

                return Calculate<Collider2D>(gameObject, Func);
            }
            public static UnityEngine.Bounds Calculate<TSource>(GameObject gameObject, Func<TSource, UnityEngine.Bounds> extractor)
                where TSource : class
            {
                var value = new UnityEngine.Bounds(gameObject.transform.position, Vector3.zero);

                var components = Dependancy.GetAll<TSource>(gameObject);

                for (int i = 0; i < components.Count; i++)
                {
                    if (i == 0)
                        value = extractor(components[i]);
                    else
                        value.Encapsulate(extractor(components[i]));
                }

                value.center = gameObject.transform.InverseTransformPoint(value.center);

                return value;
            }
        }

        public static class Text
        {
            public static string AddSpacesToCamelCase(string text)
            {
                var result = "";

                for (int i = 0; i < text.Length; i++)
                {
                    if (i > 0 && char.IsUpper(text[i]))
                        result += " ";

                    result += text[i];
                }

                return result;
            }

            public static string GetOrdinalIndicator(int number)
            {
                switch (number)
                {
                    case 1:
                        return "st";

                    case 2:
                        return "nd";

                    case 3:
                        return "rd";
                }

                return "th";
            }
        }
    }
}