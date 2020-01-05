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
        protected int _value = 50;
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

        public const string ID = "Score";

        public virtual void UpdateStatistic()
        {
            if(Core.PlayFab.IsLoggedIn)
            {
                Core.PlayFab.Player.Statistics.Update.Request(ID, Value);
            }
            else
            {

            }
        }
    }
}