using BenchmarkDotNet.Attributes;

namespace PerfTest;

public class Benchmarks
{
    private int _daysInt;
    private double _daysDouble;
    private int _hoursInt;
    private double _hoursDouble;
    private int _minutesInt;
    private double _minutesDouble;
    private int _secondsInt;
    private double _secondsDouble;
    private int _millisecondsInt;
    private double _millisecondsDouble;
    private int _microsecondsInt;
    private double _microsecondsDouble;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var random = new Random(100);
        _daysInt = random.Next(1, 9);
        _daysDouble = _daysInt;
        _hoursInt = random.Next(1, 9);
        _hoursDouble = _hoursInt;
        _minutesInt = random.Next(1, 9);
        _minutesDouble = _minutesInt;
        _secondsInt = random.Next(1, 9);
        _secondsDouble = _secondsInt;
        _millisecondsInt = random.Next(1, 9);
        _millisecondsDouble = _millisecondsInt;
        _microsecondsInt = random.Next(1, 9);
        _microsecondsDouble = _microsecondsInt;
    }

    [Benchmark(Baseline = true)]
    public System.PerfTest.TimeSpan Doubles()
    {
        return System.PerfTest.TimeSpan.FromDays(_daysDouble)
                + System.PerfTest.TimeSpan.FromHours(_hoursDouble)
                + System.PerfTest.TimeSpan.FromMinutes(_minutesDouble)
                + System.PerfTest.TimeSpan.FromSeconds(_secondsDouble)
                + System.PerfTest.TimeSpan.FromMilliseconds(_millisecondsDouble)
                + System.PerfTest.TimeSpan.FromMicroseconds(_microsecondsDouble);
    }

    // [Benchmark]
    // public System.PerfTest.TimeSpan Doubles2()
    // {
    //     return System.PerfTest.TimeSpan.FromDaysDoubles(2, 3, 4, 5, 6, 7);
    // }
    
    [Benchmark]
    public System.PerfTest.TimeSpan Int128()
    {
        return System.PerfTest.TimeSpan.FromDays(_daysInt, _hoursInt, _minutesInt, _secondsInt, _millisecondsInt, _microsecondsInt);
    }

    [Benchmark]
    public System.PerfTest.TimeSpan Int128_BigMul()
    {
        return System.PerfTest.TimeSpan.FromDays2(_daysInt, _hoursInt, _minutesInt, _secondsInt, _millisecondsInt, _microsecondsInt);
    }

    [Benchmark]
    public System.PerfTest.TimeSpan Int64()
    {
        return new System.PerfTest.TimeSpan(_daysInt, _hoursInt, _minutesInt, _secondsInt, _millisecondsInt, _microsecondsInt);
    }

    // [Benchmark]
    // public System.PerfTest.TimeSpan IntSingleParamOptimized()
    // {
    //     return System.PerfTest.TimeSpan.FromDaysOptimized(_daysInt);
    // }
}
