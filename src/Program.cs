using BenchmarkDotNet.Running;
using ReflectionToIL.Benchmarking;

namespace ReflectionToIL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<GetDataFromClosureBenchmark>();
        }
    }
}
