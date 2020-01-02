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

        public bool RequireManualSubmit
        {
            get
            {
                if (Core.PlayFab.Player.Profile.HasDisplayName == false) return true;

                return false;
            }
        }

        public override void Configure(LevelEndPhase reference)
        {
            base.Configure(reference);

            Phase.OnBegin += BeginCallback;
            Phase.OnEnd += EndCallback;

            Core.UI.Leaderboards.Score.Submit.OnInvoke += LeaderboardSubmitCallback;
        }

        void BeginCallback()
        {
            if(RequireManualSubmit == false) Submit();

            Core.Ads.Placements.Common.RewardedVideo.OnFinish += RewardedVideoAdFinishCallback;
        }

        private void RewardedVideoAdFinishCallback(UnityEngine.Advertisements.ShowResult result)
        {
            if (result == UnityEngine.Advertisements.ShowResult.Finished)
            {
                Level.Menu.End.Ad.Interactable = false;

                Level.Player.Instance.Score.Value *= adMultiplier;

                if (RequireManualSubmit == false) Submit();
            }
        }
        private void LeaderboardSubmitCallback()
        {
            if(RequireManualSubmit)
            {
                Core.Procedures.UpdateDisplayName.Start();

                Core.Procedures.UpdateDisplayName.OnResponse += UpdateUserNameProcedureCallback;
            }
            else
            {
                Submit();
            }
        }

        private void UpdateUserNameProcedureCallback(string error)
        {
            Core.Procedures.UpdateDisplayName.OnResponse -= UpdateUserNameProcedureCallback;

            if (error == null)
            {
                Core.UI.Leaderboards.Score.Submit.Element.Hide();

                Submit();
            }
            else
            {
                
            }
        }

        public virtual void Submit()
        {
            Level.Player.Instance.Score.UpdateStatistic();
        }

        private void EndCallback()
        {
            ClearCallbacks();
        }

        private void ClearCallbacks()
        {
            Core.Ads.Placements.Common.RewardedVideo.OnFinish -= RewardedVideoAdFinishCallback;

            Core.UI.Leaderboards.Score.Submit.OnInvoke -= LeaderboardSubmitCallback;

            Core.Procedures.UpdateDisplayName.OnResponse -= UpdateUserNameProcedureCallback;
        }

        protected virtual void OnDestroy()
        {
            ClearCallbacks();
        }
    }
}