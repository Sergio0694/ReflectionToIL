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

        private ClosureLoaderWithILGetters _ILGetterLoaderForSmall;

        private ClosureLoaderWithILGetters _ILGetterLoaderForMedium;

        private ClosureLoaderWithILGetters _ILGetterLoaderForLarge;

        private ClosureLoaderWithUnwrappedILGetters _UnwrappedILGetterLoaderForSmall;

        private ClosureLoaderWithUnwrappedILGetters _UnwrappedILGetterLoaderForMedium;

        private ClosureLoaderWithUnwrappedILGetters _UnwrappedILGetterLoaderForLarge;

        private ClosureLoaderWithSingleILGetter _SingleILGetterLoaderForSmall;

        private ClosureLoaderWithSingleILGetter _SingleILGetterLoaderForMedium;

        private ClosureLoaderWithSingleILGetter _SingleILGetterLoaderForLarge;

        [GlobalSetup]
        public void Setup()
        {
            _ReflectionLoaderForSmall = ClosureLoaderWithReflection.GetLoaderForDelegate(SmallDelegate);
            _ReflectionLoaderForMedium = ClosureLoaderWithReflection.GetLoaderForDelegate(MediumDelegate);
            _ReflectionLoaderForLarge = ClosureLoaderWithReflection.GetLoaderForDelegate(LargeDelegate);

            _ILGetterLoaderForSmall = ClosureLoaderWithILGetters.GetLoaderForDelegate(SmallDelegate);
            _ILGetterLoaderForMedium = ClosureLoaderWithILGetters.GetLoaderForDelegate(MediumDelegate);
            _ILGetterLoaderForLarge = ClosureLoaderWithILGetters.GetLoaderForDelegate(LargeDelegate);

            _UnwrappedILGetterLoaderForSmall = ClosureLoaderWithUnwrappedILGetters.GetLoaderForDelegate(SmallDelegate);
            _UnwrappedILGetterLoaderForMedium = ClosureLoaderWithUnwrappedILGetters.GetLoaderForDelegate(MediumDelegate);
            _UnwrappedILGetterLoaderForLarge = ClosureLoaderWithUnwrappedILGetters.GetLoaderForDelegate(LargeDelegate);

            _SingleILGetterLoaderForSmall = ClosureLoaderWithSingleILGetter.GetLoaderForDelegate(SmallDelegate);
            _SingleILGetterLoaderForMedium = ClosureLoaderWithSingleILGetter.GetLoaderForDelegate(MediumDelegate);
            _SingleILGetterLoaderForLarge = ClosureLoaderWithSingleILGetter.GetLoaderForDelegate(LargeDelegate);
        }

        // Reflection ====================================

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

        [BenchmarkCategory(nameof(Delegates.Medium)), Benchmark(Baseline = true)]
        public void ReflectionMedium()
        {
            var instance = MediumDelegate;
            var loader = _ReflectionLoaderForMedium;

            for (int i = 0; i < N; i++)
            {
                using var _ = loader.GetData(instance);
            }
        }

        [BenchmarkCategory(nameof(Delegates.Large)), Benchmark(Baseline = true)]
        public void ReflectionLarge()
        {
            var instance = LargeDelegate;
            var loader = _ReflectionLoaderForLarge;

            for (int i = 0; i < N; i++)
            {
                using var _ = loader.GetData(instance);
            }
        }

        // IL getters ====================================

        [BenchmarkCategory(nameof(Delegates.Small)), Benchmark]
        public void ILGetterSmall()
        {
            var instance = SmallDelegate;
            var loader = _ILGetterLoaderForSmall;

            for (int i = 0; i < N; i++)
            {
                using var _ = loader.GetData(instance);
            }
        }

        [BenchmarkCategory(nameof(Delegates.Medium)), Benchmark]
        public void ILGetterMedium()
        {
            var instance = MediumDelegate;
            var loader = _ILGetterLoaderForMedium;

            for (int i = 0; i < N; i++)
            {
                using var _ = loader.GetData(instance);
            }
        }

        [BenchmarkCategory(nameof(Delegates.Large)), Benchmark]
        public void ILGetterLarge()
        {
            var instance = LargeDelegate;
            var loader = _ILGetterLoaderForLarge;

            for (int i = 0; i < N; i++)
            {
                using var _ = loader.GetData(instance);
            }
        }

        // Unwrapped IL getters =========================

        [BenchmarkCategory(nameof(Delegates.Small)), Benchmark]
        public void UnwrappedILGetterSmall()
        {
            var instance = SmallDelegate;
            var loader = _UnwrappedILGetterLoaderForSmall;

            for (int i = 0; i < N; i++)
            {
                using var _ = loader.GetData(instance);
            }
        }

        [BenchmarkCategory(nameof(Delegates.Medium)), Benchmark]
        public void UnwrappedILGetterMedium()
        {
            var instance = MediumDelegate;
            var loader = _UnwrappedILGetterLoaderForMedium;

            for (int i = 0; i < N; i++)
            {
                using var _ = loader.GetData(instance);
            }
        }

        [BenchmarkCategory(nameof(Delegates.Large)), Benchmark]
        public void UnwrappedILGetterLarge()
        {
            var instance = LargeDelegate;
            var loader = _UnwrappedILGetterLoaderForLarge;

            for (int i = 0; i < N; i++)
            {
                using var _ = loader.GetData(instance);
            }
        }

        // Single getter =========================

        [BenchmarkCategory(nameof(Delegates.Small)), Benchmark]
        public void SingleILGetterSmall()
        {
            var instance = SmallDelegate;
            var loader = _SingleILGetterLoaderForSmall;

            for (int i = 0; i < N; i++)
            {
                using var _ = loader.GetData(instance);
            }
        }

        [BenchmarkCategory(nameof(Delegates.Medium)), Benchmark]
        public void SingleILGetterMedium()
        {
            var instance = MediumDelegate;
            var loader = _SingleILGetterLoaderForMedium;

            for (int i = 0; i < N; i++)
            {
                using var _ = loader.GetData(instance);
            }
        }

        [BenchmarkCategory(nameof(Delegates.Large)), Benchmark]
        public void SingleILGetterLarge()
        {
            var instance = LargeDelegate;
            var loader = _SingleILGetterLoaderForLarge;

            for (int i = 0; i < N; i++)
            {
                using var _ = loader.GetData(instance);
            }
        }
    }
}
