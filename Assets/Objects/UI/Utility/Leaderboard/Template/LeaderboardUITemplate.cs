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

using PlayFab.ClientModels;

namespace Game
{
	public class LeaderboardUITemplate : UITemplate<PlayerLeaderboardEntry, LeaderboardUITemplate>
	{
        [SerializeField]
        protected Text displayName;
        public Text DisplayName { get { return displayName; } }

        [SerializeField]
        protected Text position;
        public Text Position { get { return position; } }

        [SerializeField]
        protected Text value;
        public Text Value { get { return value; } }

        protected override void UpdateState(PlayerLeaderboardEntry reference)
        {
            base.UpdateState(reference);

            gameObject.name = reference.DisplayName;

            displayName.text = reference.DisplayName;

            position.text = (reference.Position + 1).ToString("N0") + GameTools.Text.GetOrdinalIndicator(reference.Position + 1);

            value.text = reference.StatValue.ToString("N0");
        }
    }
}