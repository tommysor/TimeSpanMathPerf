using BenchmarkDotNet.Attributes;

namespace PerfTest;

[MaxIterationCount(20)]
public class BenchmarksMilliseconds
{
    [Params(10)]
    public long MillisecondInput { get; set; }

    [Benchmark(Baseline = true)]
    public System.PerfTest.TimeSpan MainConst()
        => System.PerfTest.TimeSpan.FromMilliseconds(10, 0);

    [Benchmark]
    public System.PerfTest.TimeSpan OptimizedConst()
        => System.PerfTest.TimeSpan.FromMilliseconds(10);

    [Benchmark]
    public System.PerfTest.TimeSpan AltOptimizedConst()
        => System.PerfTest.TimeSpan.FromMilliseconds2(10, 0);

    [Benchmark]
    public System.PerfTest.TimeSpan MainDynamic()
        => System.PerfTest.TimeSpan.FromMilliseconds(MillisecondInput, 0);

    [Benchmark]
    public System.PerfTest.TimeSpan OptimizedDynamic()
        => System.PerfTest.TimeSpan.FromMilliseconds(MillisecondInput);

    [Benchmark]
    public System.PerfTest.TimeSpan AltOptimizedDynamic()
        => System.PerfTest.TimeSpan.FromMilliseconds2(MillisecondInput, 0);
}
