using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using ReflectionToIL.Implementations;

namespace ReflectionToIL.Benchmarking
{
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class GetDataFromClosureBenchmark
    {
        private const int N = 1000000;

        private readonly Delegate SmallDelegate = Delegates.Small;

        private readonly Delegate MediumDelegate = Delegates.Medium;

        private readonly Delegate LargeDelegate = Delegates.Large;

        private ClosureLoaderWithReflection _ReflectionLoaderForSmall;

        private ClosureLoaderWithReflection _ReflectionLoaderForMedium;

        private ClosureLoaderWithReflection _ReflectionLoaderForLarge;

        [GlobalSetup]
        public void Setup()
        {
            _ReflectionLoaderForSmall = ClosureLoaderWithReflection.GetLoaderForDelegate(SmallDelegate);
            _ReflectionLoaderForMedium = ClosureLoaderWithReflection.GetLoaderForDelegate(MediumDelegate);
            _ReflectionLoaderForLarge = ClosureLoaderWithReflection.GetLoaderForDelegate(LargeDelegate);
        }

        [BenchmarkCategory(nameof(Delegates.Small)), Benchmark(Baseline = true)]
        public void ReflectionSmall()
        {
            var instance = SmallDelegate;
            var loader = _ReflectionLoaderForSmall;

            for (int i = 0; i < N; i++)
            {
                using var _ = loader.GetData(instance);
            }
        }

        [BenchmarkCategory(nameof(Delegates.Small)), Benchmark(Baseline = true)]
        public void ReflectionMedium()
        {
            var instance = MediumDelegate;
            var loader = _ReflectionLoaderForMedium;

            for (int i = 0; i < N; i++)
            {
                using var _ = loader.GetData(instance);
            }
        }

        [BenchmarkCategory(nameof(Delegates.Small)), Benchmark(Baseline = true)]
        public void ReflectionLarge()
        {
            var instance = LargeDelegate;
            var loader = _ReflectionLoaderForLarge;

            for (int i = 0; i < N; i++)
            {
                using var _ = loader.GetData(instance);
            }
        }
    }
}
