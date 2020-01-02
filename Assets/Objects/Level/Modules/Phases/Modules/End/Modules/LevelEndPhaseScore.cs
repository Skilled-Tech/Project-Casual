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
	public class LevelEndPhaseScore : LevelEndPhase.Module
	{
        [SerializeField]
        protected int adMultiplier = 2;
        public int AdMultiplier { get { return adMultiplier; } }

        public override void Configure(LevelEndPhase reference)
        {
            base.Configure(reference);

            Phase.OnBegin += BeginCallback;
            Phase.OnEnd += EndCallback;
        }

        void BeginCallback()
        {
            Level.Player.Instance.Score.UpdateStatistic();

            Core.Ads.Placements.Common.RewardedVideo.OnFinish += RewardedVideoAdFinishCallback;
        }

        private void RewardedVideoAdFinishCallback(UnityEngine.Advertisements.ShowResult result)
        {
            if (result == UnityEngine.Advertisements.ShowResult.Finished)
            {
                Level.Menu.End.Ad.Interactable = false;

                Level.Player.Instance.Score.Value *= adMultiplier;
                Level.Player.Instance.Score.UpdateStatistic();
            }
        }

        private void EndCallback()
        {
            Core.Ads.Placements.Common.RewardedVideo.OnFinish -= RewardedVideoAdFinishCallback;
        }
    }
}