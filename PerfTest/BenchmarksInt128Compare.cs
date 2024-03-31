using BenchmarkDotNet.Attributes;

namespace PerfTest;

[MaxIterationCount(20)]
public class BenchmarksInt128Compare
{
    private const long _magicPositiveOverflowBasedOnUpperBits = 100;
    private const long _magicNegativeOverflowBasedOnUpperBits = 101;
    internal const long MinMicroseconds = -922_337_203_685_477_580;
    internal const long MaxMicroseconds = 922_337_203_685_477_580;

    [Params(
        -1, 
        0, 
        1, 
        MinMicroseconds, 
        MaxMicroseconds, 
        MinMicroseconds - 1,
        MaxMicroseconds + 1,
        _magicPositiveOverflowBasedOnUpperBits,
        _magicNegativeOverflowBasedOnUpperBits)]
    public Int128 MicrosecondsInput;

    public Int128 totalMicroseconds;
    public ulong totalMicrosecondsUpper;
    public ulong totalMicrosecondsLower;

    [GlobalSetup]
    public void GlobalSetup()
    {
        totalMicroseconds = GetTotalMicrosecondsFromInput(MicrosecondsInput);
        var hex = totalMicroseconds.ToString("X32");
        totalMicrosecondsUpper = ulong.Parse(hex.Substring(0, 16), System.Globalization.NumberStyles.HexNumber);
        totalMicrosecondsLower = ulong.Parse(hex.Substring(16), System.Globalization.NumberStyles.HexNumber);
    }

    private static Int128 GetTotalMicrosecondsFromInput(Int128 input)
    {
        if (input == _magicPositiveOverflowBasedOnUpperBits)
        {
            return new Int128(0x0000000000000001, 0x0);
        }
        else if (input == _magicNegativeOverflowBasedOnUpperBits)
        {
            return new Int128(0xfffffffffffffffe, 0xffffffffffffffff);
        }
        return input;
    }

    [Benchmark(Baseline = true)]
    public bool Int128() => (totalMicroseconds > MaxMicroseconds) || (totalMicroseconds < MinMicroseconds);

    [Benchmark]
    public bool UpperLower() => !(totalMicrosecondsUpper == 0UL && totalMicrosecondsLower <= (ulong)MaxMicroseconds)
                             && !(totalMicrosecondsUpper == unchecked((ulong)-1) && totalMicrosecondsLower >= unchecked((ulong)MinMicroseconds));
}
