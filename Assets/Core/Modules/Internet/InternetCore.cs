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
	public class InternetCore : Core.Module
	{
        [SerializeField]
        protected InternetState _state = InternetState.Unavailable;
        public InternetState State
        {
            get => _state;
            set
            {
                _state = value;

                OnStateChange?.Invoke(State);
            }
        }

        public delegate void StateChangeDelegate(InternetState state);
        public event StateChangeDelegate OnStateChange;

        [SerializeField]
        protected PingProcedure ping;
        public PingProcedure Ping { get { return ping; } }
        [Serializable]
        public class PingProcedure : Procedure
        {
            [SerializeField]
            protected string address = "8.8.8.8";
            public string Address { get { return address; } }

            [SerializeField]
            [Min(1)]
            protected int retries = 4;
            public int Retries { get { return retries; } }

            [SerializeField]
            protected float timeout = 5f;
            public float TimeOut { get { return timeout; } }

            [SerializeField]
            protected IntervalData interval = new IntervalData(5f, 40f);
            public IntervalData Interval { get { return interval; } }
            [Serializable]
            public struct IntervalData
            {
                [SerializeField]
                private float fail;
                public float Fail { get { return fail; } }

                [SerializeField]
                private float success;
                public float Success { get { return success; } }

                public float Sample(bool condition)
                {
                    return condition ? success : fail;
                }

                public IntervalData(float fail, float success)
                {
                    this.fail = fail;
                    this.success = success;
                }
            }

            public override void Request()
            {
                base.Request();

                if (coroutine != null) Internet.StopCoroutine(coroutine);

                coroutine = Internet.StartCoroutine(Procedure());
            }

            Coroutine coroutine;
            IEnumerator Procedure()
            {
                var results = new List<Result>();

                for (int i = 0; i < retries; i++)
                {
                    var ping = new Ping(address);

                    var time = 0f;

                    while(true)
                    {
                        if (ping.isDone) break;

                        if (time >= timeout) break;

                        yield return new WaitForEndOfFrame();

                        time += Time.unscaledDeltaTime;
                    }

                    var result = new Result(ping);

                    results.Add(result);

                    if (result.Success) break;
                }

                bool IsSuccess(Result result) => result.Success;

                bool success = results.Any(IsSuccess);

                if (success)
                    End();
                else
                    Cancel();

                yield return new WaitForSecondsRealtime(interval.Sample(success));

                Request();
            }

            public class Result
            {
                public float Time { get; protected set; }

                public bool Success { get; protected set; }

                public Result(Ping ping)
                {
                    Success = ping.isDone;

                    Time = ping.isDone ? ping.time : Mathf.Infinity;
                }
            }
        }

		[Serializable]
        public class Procedure : Core.Procedure<InternetCore>
        {
            public InternetCore Internet => Reference;
        }

        public override void Configure(Core reference)
        {
            base.Configure(reference);

            Register(this, ping);
        }

        public override void Init()
        {
            base.Init();

            ping.OnResponse.Add(PingResponseCallback);
            ping.Request();
        }

        private void PingResponseCallback(Core.Procedure.Response response)
        {
            var newState = response.Success ? InternetState.Available : InternetState.Unavailable;

            if (State != newState) State = newState;
        }
    }

    public enum InternetState
    {
        Available, Unavailable
    }
}