﻿using System;
using System.Threading.Tasks;
using Intervals;
using NLog;
using Profiler.Basics;
using Profiler.Core;
using Utils.General;

namespace TorchMonitor.ProfilerMonitors
{
    public abstract class ProfilerMonitorBase<T> : IIntervalListener
    {
        protected static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        protected abstract int SamplingSeconds { get; }

        public void OnInterval(int intervalsSinceStart)
        {
            if (intervalsSinceStart < TorchMonitorConfig.Instance.FirstIgnoredSeconds) return;
            if (intervalsSinceStart % SamplingSeconds != 0) return;

            Profile().Forget(Log);
        }

        async Task Profile()
        {
            using (var profiler = MakeProfiler())
            using (ProfilerResultQueue.Profile(profiler))
            {
                profiler.MarkStart();
                await Task.Delay(TimeSpan.FromSeconds(SamplingSeconds));
                profiler.MarkEnd();

                var result = profiler.GetResult();
                OnProfilingFinished(result);
            }
        }

        protected abstract BaseProfiler<T> MakeProfiler();
        protected abstract void OnProfilingFinished(BaseProfilerResult<T> result);
    }
}