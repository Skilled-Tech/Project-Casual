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

        public bool RequireManualSubmit => ProcessManualSubmit(false);

        public PopupUI Popup => Core.UI.Popup;

        public override void Configure(LevelEndPhase reference)
        {
            base.Configure(reference);

            Phase.OnBegin += BeginCallback;
            Phase.OnEnd += EndCallback;
            Level.OnExit += ExitCallback;
        }

        void BeginCallback()
        {
            if(RequireManualSubmit == false) Submit();

            Core.Unity.Ads.Placements.Common.RewardedVideo.OnFinish += RewardedVideoAdFinishCallback;

            Core.PlayFab.Player.Profile.OnUpdate += PlayerProfileUpdateCallback;

            Core.UI.Leaderboards.Score.Submit.Relay.OnInvoke += LeaderboardSubmitCallback;

            UpdateSubmitState();
        }

        #region Callbacks
        private void PlayerProfileUpdateCallback() => UpdateSubmitState();

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
            ProcessManualSubmit();
        }
        #endregion

        #region Manual Submit
        protected virtual void ProcessManualSubmit() => ProcessManualSubmit(true);
        protected virtual bool ProcessManualSubmit(bool execute)
        {
            if (Core.PlayFab.IsLoggedIn == false)
            {
                if(execute) RequireLogin();

                return true;
            }
            else if (Core.PlayFab.Player.Profile.HasDisplayName == false)
            {
                if(execute) RequestDisplayNameUpdate();

                return true;
            }
            else
            {
                if(execute)
                {
                    Submit();

                    UpdateSubmitState();
                }

                return false;
            }
        }

        private void RequireLogin()
        {
            Core.Procedures.Login.OnResponse.Enque(Callback);
            Core.Procedures.Login.Require();

            void Callback(ProceduresCore.LoginProperty.Element result, Procedure.Response response)
            {
                if(response.Success)
                    ProcessManualSubmit();
                else
                {
                    //TODO provide some feedback ?
                }
            }
        }

        private void RequestDisplayNameUpdate()
        {
            Core.Procedures.UpdateDisplayName.OnResponse.Enque(Callback);
            Core.Procedures.UpdateDisplayName.Require();

            void Callback(Procedure.Response response)
            {
                if (response.Success)
                    ProcessManualSubmit();
                else
                {
                    //TODO provide some feedback ?
                }
            }
        }

        protected virtual void UpdateSubmitState()
        {
            Core.UI.Leaderboards.Score.Submit.Panel.IsOn = RequireManualSubmit;
        }
        #endregion

        public virtual void Submit()
        {
            Level.Player.Instance.Score.UpdateStatistic();
        }

        private void EndCallback()
        {
            ClearCallbacks();
        }

        private void ExitCallback()
        {
            Level.Player.Instance.Score.Roundup();
        }

        private void ClearCallbacks()
        {
            Core.Unity.Ads.Placements.Common.RewardedVideo.OnFinish -= RewardedVideoAdFinishCallback;

            Core.UI.Leaderboards.Score.Submit.Relay.OnInvoke -= LeaderboardSubmitCallback;

            Core.PlayFab.Player.Profile.OnUpdate -= PlayerProfileUpdateCallback;
        }

        protected virtual void OnDestroy()
        {
            ClearCallbacks();
        }
    }
}