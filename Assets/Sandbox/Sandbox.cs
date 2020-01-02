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
	public class Sandbox : MonoBehaviour
	{
        public Core Core => Core.Instance;

        public PlayFabCore PlayFab => Core.PlayFab;

        public string ID;

        public string displayName;

        public int score;

        private void Start()
        {
            PlayFab.Login.OnResponse += LoginCallback;
            PlayFab.Login.CustomID.Request(ID);
        }

        private void LoginCallback(LoginResult result, PlayFabError error)
        {
            PlayFab.Login.OnResponse -= LoginCallback;

            if (error == null)
            {
                Debug.Log("Login Successfull");

                if(result.InfoResultPayload?.PlayerProfile?.DisplayName == displayName)
                {
                    PlayFab.Player.Statistics.Update.OnResponse += UpdateStatisticCallback;
                    PlayFab.Player.Statistics.Update.Request("Score", score);
                }
                else
                {
                    PlayFab.Player.Info.UpdateDisplayName.OnResponse += UpdateDisplayNameCallback;
                    PlayFab.Player.Info.UpdateDisplayName.Request(displayName);
                }
            }
            else
            {
                ErrorCallback(error);
            }
        }

        private void UpdateDisplayNameCallback(UpdateUserTitleDisplayNameResult result, PlayFabError error)
        {
            PlayFab.Player.Info.UpdateDisplayName.OnResponse -= UpdateDisplayNameCallback;

            if (error == null)
            {
                Debug.Log("Updated Display Name");

                PlayFab.Player.Statistics.Update.OnResponse += UpdateStatisticCallback;
                PlayFab.Player.Statistics.Update.Request("Score", score);
            }
            else
            {
                ErrorCallback(error);
            }
        }

        private void UpdateStatisticCallback(UpdatePlayerStatisticsResult result, PlayFabError error)
        {
            PlayFab.Player.Statistics.Update.OnResponse -= UpdateStatisticCallback;

            if (error == null)
            {
                Debug.Log("Update Statistics Complete");

                PlayFabClientAPI.GetLeaderboardAroundPlayer(new GetLeaderboardAroundPlayerRequest
                {
                    MaxResultsCount = 1,
                    StatisticName = "Score",
                }, GetLeaderboardAroundPlayerCallback, ErrorCallback);
            }
            else
            {
                ErrorCallback(error);
            }
        }

        private void GetLeaderboardAroundPlayerCallback(GetLeaderboardAroundPlayerResult result)
        {
            Debug.Log(result.Leaderboard[0].Position);
            Debug.Log(JsonUtility.ToJson(result, true));
        }

        private void ErrorCallback(PlayFabError error)
        {
            Debug.LogError(error.GenerateErrorReport());
        }
    }
}