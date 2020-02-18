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

using UnityEngine.Advertisements;

namespace Game
{
	public class UnityAdsPlacementModule : UnityAdsPlacementsCore.Module
	{
        [SerializeField]
        protected string _ID = "";
        public string ID { get { return _ID; } }

        public PlacementState State => Advertisement.GetPlacementState(ID);

        public bool IsReady => Advertisement.IsReady(ID);

        public override void Configure(UnityAdsPlacementsCore reference)
        {
            base.Configure(reference);

            Ads.Listener.ReadyEvent += ReadyCallback;
            Ads.Listener.StartEvent += StartCallback;
            Ads.Listener.FinishEvent += FinishCallback;
        }

        public virtual bool Show()
        {
            if (IsReady == false) return false;

            Advertisement.Show(ID);
            return true;
        }

        public virtual void Load()
        {
            Advertisement.Load(ID);
        }

        #region Events
        protected virtual void ReadyCallback(string placementId)
        {
            if (placementId == ID)
                ReadyAction();
        }
        public event Action OnReady;
        protected virtual void ReadyAction()
        {
            Debug.Log(ID + " Ad ready");

            OnReady?.Invoke();
        }

        protected virtual void StartCallback(string placementId)
        {
            if (placementId == ID)
                StartAction();
        }
        public event Action OnStart;
        protected virtual void StartAction()
        {
            Debug.Log(ID + " Ad start");

            OnStart?.Invoke();
        }

        protected virtual void FinishCallback(string placementId, ShowResult result)
        {
            if (placementId == ID)
                FinishAction(result);
        }
        public delegate void FinishDelegate(ShowResult result);
        public event FinishDelegate OnFinish;
        protected virtual void FinishAction(ShowResult result)
        {
            Debug.Log(ID + " Ad finished, result: " + result);

            OnFinish?.Invoke(result);
        }
        #endregion
    }
}