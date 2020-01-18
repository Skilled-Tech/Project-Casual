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
    public abstract class MoeEventBase<TDelegate>
    {
        public List<Element> List { get; protected set; }

        public struct Element
        {
            public TDelegate Action { get; private set; }

            public bool Remove { get; private set; }

            public Element(TDelegate action, bool remove)
            {
                this.Action = action;
                this.Remove = remove;
            }
            public Element(TDelegate action) : this(action, false)
            {

            }
        }

        public virtual void Invoke(MoeAction<TDelegate> action)
        {
            for (int i = 0; i < List.Count; i++)
                action(List[i].Action);

            List.RemoveAll(IsMarkedRemove);
            bool IsMarkedRemove(Element element) => element.Remove;
        }

        public virtual void Add(TDelegate action)
        {
            var instance = new Element(action);

            List.Add(instance);
        }

        public virtual void Enque(TDelegate action)
        {
            var instance = new Element(action, true);

            List.Add(instance);
        }

        public virtual void Remove(TDelegate action)
        {
            for (int i = List.Count - 1; i >= 0; i--)
            {
                if (List[i].Action.Equals(action))
                {
                    List.RemoveAt(i);
                    break;
                }
            }
        }

        public virtual bool Contains(TDelegate action)
        {
            for (int i = 0; i < List.Count; i++)
                if (List[i].Action.Equals(action))
                    return true;

            return false;
        }

        public MoeEventBase()
        {
            List = new List<Element>();
        }
    }

    public sealed class MoeEvent : MoeEventBase<MoeAction>
    {
        public void Invoke()
        {
            Invoke(Callback);

            void Callback(MoeAction action) => action();
        }

        #region Operators
        public static MoeEvent operator +(MoeEvent moeEvent, MoeAction callback)
        {
            moeEvent.Add(callback);

            return moeEvent;
        }

        public static MoeEvent operator -(MoeEvent moeEvent, MoeAction callback)
        {
            moeEvent.Remove(callback);

            return moeEvent;
        }
        #endregion
    }
    public sealed class MoeEvent<T1> : MoeEventBase<MoeAction<T1>>
    {
        public void Invoke(T1 arg1)
        {
            Invoke(Callback);

            void Callback(MoeAction<T1> action) => action(arg1);
        }

        #region Operators
        public static MoeEvent<T1> operator +(MoeEvent<T1> moeEvent, MoeAction<T1> callback)
        {
            moeEvent.Add(callback);

            return moeEvent;
        }

        public static MoeEvent<T1> operator -(MoeEvent<T1> moeEvent, MoeAction<T1> callback)
        {
            moeEvent.Remove(callback);

            return moeEvent;
        }
        #endregion
    }
    public sealed class MoeEvent<T1, T2> : MoeEventBase<MoeAction<T1, T2>>
    {
        public void Invoke(T1 arg1, T2 arg2)
        {
            Invoke(Callback);

            void Callback(MoeAction<T1, T2> action) => action(arg1, arg2);
        }

        #region Operators
        public static MoeEvent<T1, T2> operator +(MoeEvent<T1, T2> moeEvent, MoeAction<T1, T2> callback)
        {
            moeEvent.Add(callback);

            return moeEvent;
        }

        public static MoeEvent<T1, T2> operator -(MoeEvent<T1, T2> moeEvent, MoeAction<T1, T2> callback)
        {
            moeEvent.Remove(callback);

            return moeEvent;
        }
        #endregion
    }

    public delegate void MoeAction();
    public delegate void MoeAction<T1>(T1 arg1);
    public delegate void MoeAction<T1, T2>(T1 arg1, T2 arg2);
}