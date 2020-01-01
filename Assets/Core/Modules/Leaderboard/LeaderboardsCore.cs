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

using PlayFab;
using PlayFab.ClientModels;

namespace Game
{
	public class LeaderboardsCore : Core.Module
	{
        #region List
        public List<LeaderboardModule> List { get; protected set; }

        public int Count => List.Count;

        public LeaderboardModule this[int index] => List[index];
        public LeaderboardModule this[string ID] => Find(ID);

        public virtual LeaderboardModule Find(string ID)
        {
            for (int i = 0; i < Count; i++)
                if (this[i].ID == ID)
                    return this[i];

            return null;
        }
        #endregion

        public class Module : Core.Module<LeaderboardsCore>
        {
            public LeaderboardsCore Leaderboards => Reference;
        }

        public PlayFabCore PlayFab => Core.PlayFab;

        public override void Configure(Core reference)
        {
            base.Configure(reference);

            List = this.GetAllDependancies<LeaderboardModule>();

            References.Configure(this);
        }

        public override void Init()
        {
            base.Init();

            References.Init(this);
        }
    }
}