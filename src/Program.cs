using BenchmarkDotNet.Running;
using ReflectionToIL.Benchmarking;

namespace ReflectionToIL
{
    public class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<GetDataFromClosureBenchmark>();
        }
    }
}
