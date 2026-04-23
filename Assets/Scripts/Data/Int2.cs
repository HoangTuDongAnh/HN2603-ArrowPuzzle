using System;
using UnityEngine;

namespace ArrowPuzzle.Data
{
    [Serializable]
    public struct Int2 : IEquatable<Int2>
    {
        public int x;
        public int y;

        public Int2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Int2 Zero => new Int2(0, 0);
        public static Int2 Up => new Int2(0, 1);
        public static Int2 Down => new Int2(0, -1);
        public static Int2 Left => new Int2(-1, 0);
        public static Int2 Right => new Int2(1, 0);

        public bool Equals(Int2 other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is Int2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (x * 397) ^ y;
            }
        }

        public static Int2 operator +(Int2 a, Int2 b)
        {
            return new Int2(a.x + b.x, a.y + b.y);
        }

        public static Int2 operator -(Int2 a, Int2 b)
        {
            return new Int2(a.x - b.x, a.y - b.y);
        }

        public static bool operator ==(Int2 left, Int2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Int2 left, Int2 right)
        {
            return !left.Equals(right);
        }

        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(x, y);
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }
}
