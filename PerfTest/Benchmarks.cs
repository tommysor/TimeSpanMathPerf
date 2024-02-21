using BenchmarkDotNet.Attributes;

namespace PerfTest;

public class Benchmarks
{
    [Benchmark(Baseline = true)]
    public System.PerfTest.TimeSpan Doubles()
    {
        return System.PerfTest.TimeSpan.FromDays(2.0)
                + System.PerfTest.TimeSpan.FromHours(3.0)
                + System.PerfTest.TimeSpan.FromMinutes(4.0)
                + System.PerfTest.TimeSpan.FromSeconds(5.0)
                + System.PerfTest.TimeSpan.FromMilliseconds(6.0)
                + System.PerfTest.TimeSpan.FromMicroseconds(7.0);
    }

    // [Benchmark]
    // public System.PerfTest.TimeSpan Doubles2()
    // {
    //     return System.PerfTest.TimeSpan.FromDaysDoubles(2, 3, 4, 5, 6, 7);
    // }
    
    [Benchmark]
    public System.PerfTest.TimeSpan Int128()
    {
        return System.PerfTest.TimeSpan.FromDays(2, 3, 4, 5, 6, 7);
    }

    [Benchmark]
    public System.PerfTest.TimeSpan Int64()
    {
        return new System.PerfTest.TimeSpan(2, 3, 4, 5, 6, 7);
    }

    // [Benchmark]
    // public System.PerfTest.TimeSpan IntSingleParamOptimized()
    // {
    //     return System.PerfTest.TimeSpan.FromDaysOptimized(2);
    // }
}
