using System;
using System.Diagnostics.CodeAnalysis;

namespace OrchardCore.ShortCodes
{
    public readonly struct ShortCodeSpan : IEquatable<ShortCodeSpan>, IComparable<ShortCodeSpan>
    {
        public static readonly ShortCodeSpan Default = new ShortCodeSpan(0, 0);

        public ShortCodeSpan(int start, int length)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            if (start + length < start)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            Start = start;
            Length = length;
        }

        public int Start { get; }

        public int Length { get; }

        public int End => Start + Length;

        public bool IsEmpty => Length == 0;

        public int CompareTo([AllowNull] ShortCodeSpan other)
        {
            var diff = Start - other.Start;
            if (diff != 0)
            {
                return diff;
            }

            return Length - other.Length;
        }

        public bool Equals([AllowNull] ShortCodeSpan other)
            => Start == other.Start && Length == other.Length;

        public override int GetHashCode() => HashCode.Combine(Start, Length);

        public override string ToString() => $"[{Start}..{End})";
    }
}
