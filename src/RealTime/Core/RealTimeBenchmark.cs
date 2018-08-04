// <copyright file="RealTimeBenchmark.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Core
{
#if BENCHMARK
    using ColossalFramework.IO;
    using SkyTools.Benchmarks;
    using SkyTools.Tools;

    /// <summary>
    /// A special class that handles the performance benchmarking.
    /// </summary>
    internal static class RealTimeBenchmark
    {
        /// <summary>
        /// Initializes the benchmarking.
        /// </summary>
        public static void Setup()
        {
            var benchmark = new BenchmarkSimulationManager();
            SimulationManager.RegisterSimulationManager(benchmark);
            LoadingManager.instance.m_levelUnloaded += benchmark.Stop;
        }

        private sealed class BenchmarkSimulationManager : ISimulationManager
        {
            private readonly Benchmark benchmark;
            private bool isRunning;

            public BenchmarkSimulationManager()
            {
                benchmark = Benchmark.Create(0x1000);
                SetupMethods();
            }

            public void Stop()
            {
                benchmark.Stop();
                benchmark.Dump();
                isRunning = false;
                Log.Info("Benchmarking stopped.");
            }

            public void EarlyUpdateData()
            {
            }

            public void GetData(FastList<IDataContainer> data)
            {
            }

            public string GetName()
            {
                return nameof(BenchmarkSimulationManager);
            }

            public ThreadProfiler GetSimulationProfiler()
            {
                return null;
            }

            public void LateUpdateData(SimulationManager.UpdateMode mode)
            {
            }

            public void SimulationStep(int subStep)
            {
                if (subStep == 1000)
                {
                    // This is the 'late update data' phase
                    return;
                }
                else if (!isRunning)
                {
                    isRunning = true;

                    try
                    {
                        // On failure, don't try to activate benchmark on each step
                        benchmark.Start();
                        Log.Info("Benchmarking started.");
                    }
                    catch
                    {
                    }
                }

                if ((SimulationManager.instance.m_currentFrameIndex & 0xFFF) == 0xFFF)
                {
                    benchmark.MakeSnapshot();
                }
            }

            public void UpdateData(SimulationManager.UpdateMode mode)
            {
            }

            private void SetupMethods()
            {
                try
                {
                    benchmark.BenchmarkMethod(typeof(ResidentAI), "SimulationStep", typeof(uint), typeof(Citizen).MakeByRefType());
                    benchmark.BenchmarkMethod(typeof(TouristAI), "SimulationStep", typeof(uint), typeof(Citizen).MakeByRefType());
                    benchmark.BenchmarkMethod(typeof(SimulationManager), "SimulationStep");
                }
                catch
                {
                }
            }
        }
    }
#endif
}
