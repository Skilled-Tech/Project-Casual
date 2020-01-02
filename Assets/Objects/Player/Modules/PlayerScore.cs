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
	public class PlayerScore : Player.Module
	{
		[SerializeField]
        protected int _value;
        public int Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value < 0) value = 0;

                this._value = value;

                OnValueChange?.Invoke(Value);
            }
        }

        public delegate void ValueChangeDelegate(int value);
        public event ValueChangeDelegate OnValueChange;

        public override void Configure(Player reference)
        {
            base.Configure(reference);

            Value += 2000;
        }
    }
}