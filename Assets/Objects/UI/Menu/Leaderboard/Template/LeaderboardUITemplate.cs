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

using PlayFab.ClientModels;

namespace Game
{
	public class LeaderboardUITemplate : UITemplate<LeaderboardEntry, LeaderboardUITemplate>
	{
        [SerializeField]
        protected Image location;
        public Image Location { get { return location; } }

        [SerializeField]
        protected Text displayName;
        public Text DisplayName { get { return displayName; } }

        [SerializeField]
        protected Text position;
        public Text Position { get { return position; } }

        [SerializeField]
        protected Text value;
        public Text Value { get { return value; } }

        protected override void UpdateState(LeaderboardEntry reference)
        {
            base.UpdateState(reference);

            UpdateLocation(reference.Location);

            displayName.text = string.IsNullOrEmpty(reference.DisplayName) ? "Anonymous Player" : reference.DisplayName;

            position.text = (reference.Position + 1).ToString("N0") + GameTools.Text.GetOrdinalIndicator(reference.Position + 1);

            value.text = reference.Value.ToString("N0");

            if (reference.ID == Core.PlayFab.Player.Profile.ID)
            {
                DisplayName.text += " (You)";
            }

            gameObject.name = displayName.text;
        }

        protected virtual void UpdateLocation(LocationModel model)
        {
            if(model?.CountryCode == null)
            {
                location.sprite = Core.Countries.Default.Sprite;
            }
            else
            {
                var element = Core.Countries.From(model.CountryCode.Value);

                if (element == null)
                    location.sprite = Core.Countries.Default.Sprite;
                else
                    location.sprite = element.Sprite;
            }
        }
    }
}