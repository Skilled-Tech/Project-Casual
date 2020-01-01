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
	public class MainMenu : MonoBehaviour
	{
        public Core Core => Core.Instance;
        public PlayFabCore PlayFab => Core.PlayFab;

        private void Start()
        {
            PlayFab.Login.OnResponse += LoginCallback;
            PlayFab.Login.CustomID.Request();
        }

        void LoginCallback(LoginResult result, PlayFabError error)
        {
            PlayFab.Login.OnResponse -= LoginCallback;

            if (error == null)
            {
                PlayFab.Player.Statistics.Update.OnResponse += UpdatePlayerStatisticsCallback;
                PlayFab.Player.Statistics.Update.Request("Score", 400);
            }
            else
            {
                Debug.LogError(error.GenerateErrorReport());
            }
        }

        private void UpdatePlayerStatisticsCallback(UpdatePlayerStatisticsResult result, PlayFabError error)
        {
            PlayFab.Player.Statistics.Update.OnResponse -= UpdatePlayerStatisticsCallback;

            if (error == null)
            {
                Debug.Log("Posted new statistic");

                Core.Leaderboards[0].Request();
            }
            else
            {
                Debug.LogError(error.GenerateErrorReport());
            }
        }
    }
}