// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace System.PerfTest
{
    // TimeSpan represents a duration of time.  A TimeSpan can be negative
    // or positive.
    //
    // TimeSpan is internally represented as a number of ticks. A tick is equal
    // to 100 nanoseconds. While this maps well into units of time such as hours
    // and days, any periods longer than that aren't representable in a nice fashion.
    // For instance, a month can be between 28 and 31 days, while a year
    // can contain 365 or 366 days.  A decade can have between 1 and 3 leapyears,
    // depending on when you map the TimeSpan into the calendar.  This is why
    // we do not provide Years() or Months().
    //
    // Note: System.TimeSpan needs to interop with the WinRT structure
    // type Windows::Foundation:TimeSpan. These types are currently binary-compatible in
    // memory so no custom marshalling is required. If at any point the implementation
    // details of this type should change, or new fields added, we need to remember to add
    // an appropriate custom ILMarshaler to keep WInRT interop scenarios enabled.
    //
    [Serializable]
    public readonly struct TimeSpan
        : IComparable,
          IComparable<TimeSpan>,
          IEquatable<TimeSpan>
    {
        /// <summary>
        /// Represents the number of nanoseconds per tick. This field is constant.
        /// </summary>
        /// <remarks>
        /// The value of this constant is 100.
        /// </remarks>
        public const long NanosecondsPerTick = 100;

        /// <summary>
        /// Represents the number of ticks in 1 microsecond. This field is constant.
        /// </summary>
        /// <remarks>
        /// The value of this constant is 10.
        /// </remarks>
        public const long TicksPerMicrosecond = 10;

        /// <summary>
        /// Represents the number of ticks in 1 millisecond. This field is constant.
        /// </summary>
        /// <remarks>
        /// The value of this constant is 10 thousand; that is, 10,000.
        /// </remarks>
        public const long TicksPerMillisecond = TicksPerMicrosecond * 1000;                         //          10,000
        public const long TicksPerSecond = TicksPerMillisecond * 1000;                              //      10,000,000
        public const long TicksPerMinute = TicksPerSecond * 60;                                     //     600,000,000
        public const long TicksPerHour = TicksPerMinute * 60;                                       //  36,000,000,000
        public const long TicksPerDay = TicksPerHour * 24;                                          // 864,000,000,000

        internal const long MicrosecondsPerMillisecond = TicksPerMillisecond / TicksPerMicrosecond; //           1,000
        internal const long MicrosecondsPerSecond = TicksPerSecond / TicksPerMicrosecond;           //       1,000,000
        internal const long MicrosecondsPerMinute = TicksPerMinute / TicksPerMicrosecond;           //      60,000,000
        internal const long MicrosecondsPerHour = TicksPerHour / TicksPerMicrosecond;               //   3,600,000,000
        internal const long MicrosecondsPerDay = TicksPerDay / TicksPerMicrosecond;                 //  86,400,000,000

        internal const long MillisecondsPerSecond = TicksPerSecond / TicksPerMillisecond;           //           1,000
        internal const long MillisecondsPerMinute = TicksPerMinute / TicksPerMillisecond;           //          60,000
        internal const long MillisecondsPerHour = TicksPerHour / TicksPerMillisecond;               //       3,600,000
        internal const long MillisecondsPerDay = TicksPerDay / TicksPerMillisecond;                 //      86,400,000

        internal const long SecondsPerMinute = TicksPerMinute / TicksPerSecond;                     //              60
        internal const long SecondsPerHour = TicksPerHour / TicksPerSecond;                         //           3,600
        internal const long SecondsPerDay = TicksPerDay / TicksPerSecond;                           //          86,400

        internal const long MinutesPerHour = TicksPerHour / TicksPerMinute;                         //              60
        internal const long MinutesPerDay = TicksPerDay / TicksPerMinute;                           //           1,440

        internal const long HoursPerDay = TicksPerDay / TicksPerHour;                               //              24

        internal const long MinTicks = long.MinValue;                                               // -9,223,372,036,854,775,808
        internal const long MaxTicks = long.MaxValue;                                               // +9,223,372,036,854,775,807

        internal const long MinMicroseconds = MinTicks / TicksPerMicrosecond;                       // -  922,337,203,685,477,580
        internal const long MaxMicroseconds = MaxTicks / TicksPerMicrosecond;                       // +  922,337,203,685,477,580

        internal const long MinMilliseconds = MinTicks / TicksPerMillisecond;                       // -      922,337,203,685,477
        internal const long MaxMilliseconds = MaxTicks / TicksPerMillisecond;                       // +      922,337,203,685,477

        internal const long MinSeconds = MinTicks / TicksPerSecond;                                 // -          922,337,203,685
        internal const long MaxSeconds = MaxTicks / TicksPerSecond;                                 // +          922,337,203,685

        internal const long MinMinutes = MinTicks / TicksPerMinute;                                 // -           15,372,286,728
        internal const long MaxMinutes = MaxTicks / TicksPerMinute;                                 // +           15,372,286,728

        internal const long MinHours = MinTicks / TicksPerHour;                                     // -              256,204,778
        internal const long MaxHours = MaxTicks / TicksPerHour;                                     // +              256,204,778

        internal const long MinDays = MinTicks / TicksPerDay;                                       // -               10,675,199
        internal const long MaxDays = MaxTicks / TicksPerDay;                                       // +               10,675,199

        internal const long TicksPerTenthSecond = TicksPerMillisecond * 100;

        public static readonly TimeSpan Zero = new TimeSpan(0);

        public static readonly TimeSpan MaxValue = new TimeSpan(MaxTicks);
        public static readonly TimeSpan MinValue = new TimeSpan(MinTicks);

        // internal so that DateTime doesn't have to call an extra get
        // method for some arithmetic operations.
        internal readonly long _ticks; // Do not rename (binary serialization)

        public TimeSpan(long ticks)
        {
            _ticks = ticks;
        }

        public TimeSpan(int hours, int minutes, int seconds)
        {
            _ticks = TimeToTicks(hours, minutes, seconds);
        }

        public TimeSpan(int days, int hours, int minutes, int seconds)
            : this(days, hours, minutes, seconds, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
        /// days, hours, minutes, seconds, and milliseconds.
        /// </summary>
        /// <param name="days">Number of days.</param>
        /// <param name="hours">Number of hours.</param>
        /// <param name="minutes">Number of minutes.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <param name="milliseconds">Number of milliseconds.</param>
        /// <remarks>
        /// The specified <paramref name="days"/>, <paramref name="hours"/>, <paramref name="minutes"/>, <paramref name="seconds"/>
        /// and <paramref name="milliseconds"/> are converted to ticks, and that value initializes this instance.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
        /// </exception>
        public TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds) :
            this(days, hours, minutes, seconds, milliseconds, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
        /// days, hours, minutes, seconds, and milliseconds.
        /// </summary>
        /// <param name="days">Number of days.</param>
        /// <param name="hours">Number of hours.</param>
        /// <param name="minutes">Number of minutes.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <param name="milliseconds">Number of milliseconds.</param>
        /// <param name="microseconds">Number of microseconds.</param>
        /// <remarks>
        /// The specified <paramref name="days"/>, <paramref name="hours"/>, <paramref name="minutes"/>, <paramref name="seconds"/>
        /// <paramref name="milliseconds"/> and <paramref name="microseconds"/> are converted to ticks, and that value initializes this instance.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
        /// </exception>
        public TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds, int microseconds)
        {
            long totalMicroseconds = (days * MicrosecondsPerDay)
                                   + (hours * MicrosecondsPerHour)
                                   + (minutes * MicrosecondsPerMinute)
                                   + (seconds * MicrosecondsPerSecond)
                                   + (milliseconds * MicrosecondsPerMillisecond)
                                   + microseconds;

            if ((totalMicroseconds > MaxMicroseconds) || (totalMicroseconds < MinMicroseconds))
            {
                ThrowHelper.ThrowArgumentOutOfRange_TimeSpanTooLong();
            }
            _ticks = totalMicroseconds * TicksPerMicrosecond;
        }

        public long Ticks => _ticks;

        public int Days => (int)(_ticks / TicksPerDay);

        public int Hours => (int)(_ticks / TicksPerHour % HoursPerDay);

        public int Milliseconds => (int)(_ticks / TicksPerMillisecond % MillisecondsPerSecond);

        /// <summary>
        /// Gets the microseconds component of the time interval represented by the current <see cref="TimeSpan"/> structure.
        /// </summary>
        /// <remarks>
        /// The <see cref="Microseconds"/> property represents whole microseconds, whereas the
        /// <see cref="TotalMicroseconds"/> property represents whole and fractional microseconds.
        /// </remarks>
        public int Microseconds => (int)(_ticks / TicksPerMicrosecond % MicrosecondsPerMillisecond);

        /// <summary>
        /// Gets the nanoseconds component of the time interval represented by the current <see cref="TimeSpan"/> structure.
        /// </summary>
        /// <remarks>
        /// The <see cref="Nanoseconds"/> property represents whole nanoseconds, whereas the
        /// <see cref="TotalNanoseconds"/> property represents whole and fractional nanoseconds.
        /// </remarks>
        public int Nanoseconds => (int)(_ticks % TicksPerMicrosecond * NanosecondsPerTick);

        public int Minutes => (int)(_ticks / TicksPerMinute % MinutesPerHour);

        public int Seconds => (int)(_ticks / TicksPerSecond % SecondsPerMinute);

        public double TotalDays => (double)_ticks / TicksPerDay;

        public double TotalHours => (double)_ticks / TicksPerHour;

        public double TotalMilliseconds
        {
            get
            {
                double temp = (double)_ticks / TicksPerMillisecond;

                if (temp > MaxMilliseconds)
                {
                    return MaxMilliseconds;
                }

                if (temp < MinMilliseconds)
                {
                    return MinMilliseconds;
                }
                return temp;
            }
        }

        /// <summary>
        /// Gets the value of the current <see cref="TimeSpan"/> structure expressed in whole and fractional microseconds.
        /// </summary>
        /// <remarks>
        /// This property converts the value of this instance from ticks to microseconds.
        /// This number might include whole and fractional microseconds.
        ///
        /// The <see cref="TotalMicroseconds"/> property represents whole and fractional microseconds,
        /// whereas the <see cref="Microseconds"/> property represents whole microseconds.
        /// </remarks>
        public double TotalMicroseconds => (double)_ticks / TicksPerMicrosecond;

        /// <summary>
        /// Gets the value of the current <see cref="TimeSpan"/> structure expressed in whole and fractional nanoseconds.
        /// </summary>
        /// <remarks>
        /// This property converts the value of this instance from ticks to nanoseconds.
        /// This number might include whole and fractional nanoseconds.
        ///
        /// The <see cref="TotalNanoseconds"/> property represents whole and fractional nanoseconds,
        /// whereas the <see cref="Nanoseconds"/> property represents whole nanoseconds.
        /// </remarks>
        public double TotalNanoseconds => (double)_ticks * NanosecondsPerTick;

        public double TotalMinutes => (double)_ticks / TicksPerMinute;

        public double TotalSeconds => (double)_ticks / TicksPerSecond;

        public TimeSpan Add(TimeSpan ts) => this + ts;

        // Compares two TimeSpan values, returning an integer that indicates their
        // relationship.
        //
        public static int Compare(TimeSpan t1, TimeSpan t2) => t1._ticks.CompareTo(t2._ticks);

        // Returns a value less than zero if this  object
        public int CompareTo(object? value)
        {
            if (value is null)
            {
                return 1;
            }

            if (value is TimeSpan other)
            {
                return CompareTo(other);
            }

            throw new ArgumentException("Arg_MustBeTimeSpan");
        }

        public int CompareTo(TimeSpan value) => Compare(this, value);

        public static TimeSpan FromDays(double value) => Interval(value, TicksPerDay);

        public TimeSpan Duration()
        {
            if (_ticks == MinTicks)
            {
                ThrowHelper.ThrowOverflowException_TimeSpanDuration();
            }
            return new TimeSpan(_ticks >= 0 ? _ticks : -_ticks);
        }

        public override bool Equals([NotNullWhen(true)] object? value) => (value is TimeSpan other) && Equals(other);

        public bool Equals(TimeSpan obj) => Equals(this, obj);

        public static bool Equals(TimeSpan t1, TimeSpan t2) => t1 == t2;

        public override int GetHashCode() => _ticks.GetHashCode();

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
        /// days.
        /// </summary>
        /// <param name="days">Number of days.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
        /// </exception>
        public static TimeSpan FromDaysOptimized(int days)
        {
            if ((days > MaxDays) || (days < MinDays))
            {
                ThrowHelper.ThrowArgumentOutOfRange_TimeSpanTooLong();
            }
            var ticks = (long)days * TicksPerDay;
            return new TimeSpan(ticks);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
        /// days, hours, minutes, seconds, milliseconds, and microseconds.
        /// </summary>
        /// <param name="days">Number of days.</param>
        /// <param name="hours">Number of hours.</param>
        /// <param name="minutes">Number of minutes.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <param name="milliseconds">Number of milliseconds.</param>
        /// <param name="microseconds">Number of microseconds.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
        /// </exception>
        public static TimeSpan FromDays(int days, int hours = 0, long minutes = 0, long seconds = 0, long milliseconds = 0, long microseconds = 0)
        {
            Int128 totalMicroseconds = ((Int128)days * (Int128)MicrosecondsPerDay)
                                     + ((Int128)hours * (Int128)MicrosecondsPerHour)
                                     + ((Int128)minutes * (Int128)MicrosecondsPerMinute)
                                     + ((Int128)seconds * (Int128)MicrosecondsPerSecond)
                                     + ((Int128)milliseconds * (Int128)MicrosecondsPerMillisecond)
                                     + (Int128)microseconds;

            if ((totalMicroseconds > MaxMicroseconds) || (totalMicroseconds < MinMicroseconds))
            {
                ThrowHelper.ThrowArgumentOutOfRange_TimeSpanTooLong();
            }
            var ticks = (long)totalMicroseconds * TicksPerMicrosecond;
            return new TimeSpan(ticks);
        }

        public static TimeSpan FromDays2(int days, int hours = 0, long minutes = 0, long seconds = 0, long milliseconds = 0, long microseconds = 0)
        {
            Int128 totalMicroseconds = BigMul(days, MicrosecondsPerDay)
                                     + BigMul(hours, MicrosecondsPerHour)
                                     + BigMul(minutes, MicrosecondsPerMinute)
                                     + BigMul(seconds, MicrosecondsPerSecond)
                                     + BigMul(milliseconds, MicrosecondsPerMillisecond)
                                     + microseconds;
  
            if ((totalMicroseconds > MaxMicroseconds) || (totalMicroseconds < MinMicroseconds))
            {
                ThrowHelper.ThrowArgumentOutOfRange_TimeSpanTooLong();
            }
            var ticks = (long)totalMicroseconds * TicksPerMicrosecond;
            return new TimeSpan(ticks);
        }
        internal static Int128 BigMul(long a, long b)
        {
            long upper = Math.BigMul(a, b, out long lower);
            return new Int128((ulong)upper, (ulong)lower);
        }

        public static TimeSpan FromDaysDoubles(int days, int hours = 0, long minutes = 0, long seconds = 0, long milliseconds = 0, long microseconds = 0)
        {
            return FromDays((double)days)
                + FromHours((double)hours)
                + FromMinutes((double)minutes)
                + FromSeconds((double)seconds)
                + FromMilliseconds((double)milliseconds)
                + FromMicroseconds((double)microseconds);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
        /// hours.
        /// </summary>
        /// <param name="hours">Number of hours.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
        /// </exception>
        public static TimeSpan FromHours(int hours) => FromDays(0, hours: hours);

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
        /// hours, minutes, seconds, milliseconds, and microseconds.
        /// </summary>
        /// <param name="hours">Number of hours.</param>
        /// <param name="minutes">Number of minutes.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <param name="milliseconds">Number of milliseconds.</param>
        /// <param name="microseconds">Number of microseconds.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
        /// </exception>
        public static TimeSpan FromHours(int hours, long minutes = 0, long seconds = 0, long milliseconds = 0, long microseconds = 0)
            => FromDays(0, hours: hours, minutes: minutes, seconds: seconds, milliseconds: milliseconds, microseconds: microseconds);

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
        /// minutes.
        /// </summary>
        /// <param name="minutes">Number of minutes.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
        /// </exception>
        public static TimeSpan FromMinutes(long minutes) => FromDays(0, minutes: minutes);

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
        /// minutes, seconds, milliseconds, and microseconds.
        /// </summary>
        /// <param name="minutes">Number of minutes.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <param name="milliseconds">Number of milliseconds.</param>
        /// <param name="microseconds">Number of microseconds.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
        /// </exception>
        public static TimeSpan FromMinutes(long minutes, long seconds = 0, long milliseconds = 0, long microseconds = 0)
            => FromDays(0, minutes: minutes, seconds: seconds, milliseconds: milliseconds, microseconds: microseconds);

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
        /// seconds.
        /// </summary>
        /// <param name="seconds">Number of seconds.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
        /// </exception>
        public static TimeSpan FromSeconds(long seconds) => FromDays(0, seconds: seconds);

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
        /// seconds, milliseconds, and microseconds.
        /// </summary>
        /// <param name="seconds">Number of seconds.</param>
        /// <param name="milliseconds">Number of milliseconds.</param>
        /// <param name="microseconds">Number of microseconds.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
        /// </exception>
        public static TimeSpan FromSeconds(long seconds, long milliseconds = 0, long microseconds = 0)
            => FromDays(0, seconds: seconds, milliseconds: milliseconds, microseconds: microseconds);

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
        /// milliseconds, and microseconds.
        /// </summary>
        /// <param name="milliseconds">Number of milliseconds.</param>
        /// <param name="microseconds">Number of microseconds.</param>
        /// <returns>Returns a <see cref="TimeSpan"/> that represents a specified number of milliseconds, and microseconds.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
        /// </exception>
        public static TimeSpan FromMilliseconds(long milliseconds, long microseconds = 0)
        {
            Int128 totalMicroseconds = BigMul(milliseconds, MicrosecondsPerMillisecond)
                                     + microseconds;

            return FromMicroseconds(totalMicroseconds);
        }

        public static TimeSpan FromMilliseconds2(long milliseconds, long microseconds = 0)
        {
            if (microseconds == 0)
            {
                return FromUnits(milliseconds, TicksPerMillisecond, MinMilliseconds, MaxMilliseconds);
            }

            Int128 totalMicroseconds = BigMul(milliseconds, MicrosecondsPerMillisecond)
                                     + microseconds;

            return FromMicroseconds(totalMicroseconds);
        }

        public static TimeSpan FromMilliseconds(long milliseconds)
            => FromUnits(milliseconds, TicksPerMillisecond, MinMilliseconds, MaxMilliseconds);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TimeSpan FromUnits(long units, long ticksPerUnit, long minUnits, long maxUnits)
        {
            System.Diagnostics.Debug.Assert(minUnits < 0);
            System.Diagnostics.Debug.Assert(maxUnits > 0);

            if (units > maxUnits || units < minUnits)
            {
                ThrowHelper.ThrowArgumentOutOfRange_TimeSpanTooLong();
            }
            return TimeSpan.FromTicks(units * ticksPerUnit);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TimeSpan FromMicroseconds(Int128 microseconds)
        {
            if ((microseconds > MaxMicroseconds) || (microseconds < MinMicroseconds))
            {
                ThrowHelper.ThrowArgumentOutOfRange_TimeSpanTooLong();
            }
            long ticks = (long)microseconds * TicksPerMicrosecond;
            return TimeSpan.FromTicks(ticks);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
        /// microseconds.
        /// </summary>
        /// <param name="microseconds">Number of microseconds.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
        /// </exception>
        public static TimeSpan FromMicroseconds(long microseconds) => FromDays(0, microseconds: microseconds);

        public static TimeSpan FromHours(double value) => Interval(value, TicksPerHour);

        private static TimeSpan Interval(double value, double scale)
        {
            if (double.IsNaN(value))
            {
                ThrowHelper.ThrowArgumentException_Arg_CannotBeNaN();
            }
            return IntervalFromDoubleTicks(value * scale);
        }

        private static TimeSpan IntervalFromDoubleTicks(double ticks)
        {
            if ((ticks > MaxTicks) || (ticks < MinTicks) || double.IsNaN(ticks))
            {
                ThrowHelper.ThrowOverflowException_TimeSpanTooLong();
            }
            if (ticks == MaxTicks)
            {
                return MaxValue;
            }
            return new TimeSpan((long)ticks);
        }

        public static TimeSpan FromMilliseconds(double value) => Interval(value, TicksPerMillisecond);

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> that represents a specified number of microseconds.
        /// </summary>
        /// <param name="value">A number of microseconds.</param>
        /// <returns>An object that represents <paramref name="value"/>.</returns>
        /// <exception cref="OverflowException">
        /// <paramref name="value"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.
        ///
        /// -or-
        ///
        /// <paramref name="value"/> is <see cref="double.PositiveInfinity"/>
        ///
        /// -or-
        ///
        /// <paramref name="value"/> is <see cref="double.NegativeInfinity"/>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="value"/> is equal to <see cref="double.NaN"/>.
        /// </exception>
        public static TimeSpan FromMicroseconds(double value) => Interval(value, TicksPerMicrosecond); // ISSUE: https://github.com/dotnet/runtime/issues/66815

        public static TimeSpan FromMinutes(double value) => Interval(value, TicksPerMinute);

        public TimeSpan Negate() => -this;

        public static TimeSpan FromSeconds(double value) => Interval(value, TicksPerSecond);

        public TimeSpan Subtract(TimeSpan ts) => this - ts;

        public TimeSpan Multiply(double factor) => this * factor;

        public TimeSpan Divide(double divisor) => this / divisor;

        public double Divide(TimeSpan ts) => this / ts;

        public static TimeSpan FromTicks(long value) => new TimeSpan(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static long TimeToTicks(int hour, int minute, int second)
        {
            // totalSeconds is bounded by 2^31 * 2^12 + 2^31 * 2^8 + 2^31,
            // which is less than 2^44, meaning we won't overflow totalSeconds.
            long totalSeconds = (hour * SecondsPerHour)
                              + (minute * SecondsPerMinute)
                              + second;

            if ((totalSeconds > MaxSeconds) || (totalSeconds < MinSeconds))
            {
                ThrowHelper.ThrowArgumentOutOfRange_TimeSpanTooLong();
            }
            return totalSeconds * TicksPerSecond;
        }

        // See System.Globalization.TimeSpanParse and System.Globalization.TimeSpanFormat
        #region ParseAndFormat
     
        public override string ToString() => $"{Days}.{Hours}:{Minutes}:{Seconds}";
        #endregion

        public static TimeSpan operator -(TimeSpan t)
        {
            if (t._ticks == MinTicks)
            {
                ThrowHelper.ThrowOverflowException_NegateTwosCompNum();
            }
            return new TimeSpan(-t._ticks);
        }

        public static TimeSpan operator -(TimeSpan t1, TimeSpan t2)
        {
            long result = t1._ticks - t2._ticks;
            long t1Sign = t1._ticks >> 63;

            if ((t1Sign != (t2._ticks >> 63)) && (t1Sign != (result >> 63)))
            {
                // Overflow if signs of operands was different and result's sign was opposite.
                // >> 63 gives the sign bit (either 64 1's or 64 0's).
                ThrowHelper.ThrowOverflowException_TimeSpanTooLong();
            }
            return new TimeSpan(result);
        }

        public static TimeSpan operator +(TimeSpan t) => t;

        public static TimeSpan operator +(TimeSpan t1, TimeSpan t2)
        {
            long result = t1._ticks + t2._ticks;
            long t1Sign = t1._ticks >> 63;

            if ((t1Sign == (t2._ticks >> 63)) && (t1Sign != (result >> 63)))
            {
                // Overflow if signs of operands was identical and result's sign was opposite.
                // >> 63 gives the sign bit (either 64 1's or 64 0's).
                ThrowHelper.ThrowOverflowException_TimeSpanTooLong();
            }
            return new TimeSpan(result);
        }

        /// <inheritdoc cref="IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
        public static TimeSpan operator *(TimeSpan timeSpan, double factor)
        {
            if (double.IsNaN(factor))
            {
                ThrowHelper.ThrowArgumentException_Arg_CannotBeNaN("factor");
            }

            // Rounding to the nearest tick is as close to the result we would have with unlimited
            // precision as possible, and so likely to have the least potential to surprise.
            double ticks = Math.Round(timeSpan.Ticks * factor);
            return IntervalFromDoubleTicks(ticks);
        }

        /// <inheritdoc cref="IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
        public static TimeSpan operator *(double factor, TimeSpan timeSpan) => timeSpan * factor;

        /// <inheritdoc cref="IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
        public static TimeSpan operator /(TimeSpan timeSpan, double divisor)
        {
            if (double.IsNaN(divisor))
            {
                ThrowHelper.ThrowArgumentException_Arg_CannotBeNaN("divisor");
            }

            double ticks = Math.Round(timeSpan.Ticks / divisor);
            return IntervalFromDoubleTicks(ticks);
        }

        // Using floating-point arithmetic directly means that infinities can be returned, which is reasonable
        // if we consider TimeSpan.FromHours(1) / TimeSpan.Zero asks how many zero-second intervals there are in
        // an hour for which infinity is the mathematic correct answer. Having TimeSpan.Zero / TimeSpan.Zero return NaN
        // is perhaps less useful, but no less useful than an exception.
        /// <inheritdoc cref="IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
        public static double operator /(TimeSpan t1, TimeSpan t2) => t1.Ticks / (double)t2.Ticks;

        /// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
        public static bool operator ==(TimeSpan t1, TimeSpan t2) => t1._ticks == t2._ticks;

        /// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
        public static bool operator !=(TimeSpan t1, TimeSpan t2) => t1._ticks != t2._ticks;

        /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
        public static bool operator <(TimeSpan t1, TimeSpan t2) => t1._ticks < t2._ticks;

        /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
        public static bool operator <=(TimeSpan t1, TimeSpan t2) => t1._ticks <= t2._ticks;

        /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
        public static bool operator >(TimeSpan t1, TimeSpan t2) => t1._ticks > t2._ticks;

        /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
        public static bool operator >=(TimeSpan t1, TimeSpan t2) => t1._ticks >= t2._ticks;
    }
}
