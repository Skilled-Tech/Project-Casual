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
	public class LeaderboardGlobalModule : LeaderboardModule.Element<GetLeaderboardResult>
    {
        [SerializeField]
        protected int max = 4;
        public int Max { get { return max; } }

        public override string ID => "Global";

        public override void Request()
        {
            base.Request();

            Core.PlayFab.Title.Leaderboards.Get.OnResponse.Enque(ResponseCallback);
            Core.PlayFab.Title.Leaderboards.Get.Request(Leaderboard.ID, max);
        }

        public override IList<PlayerLeaderboardEntry> ResultToList(GetLeaderboardResult result) => result.Leaderboard;
    }
}