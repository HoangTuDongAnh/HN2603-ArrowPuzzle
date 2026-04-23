using System;

namespace ArrowPuzzle.Data
{
    public enum Direction
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    }

    public enum ArrowColor
    {
        None = 0,
        Red = 1,
        Blue = 2,
        Green = 3,
        Yellow = 4,
        Purple = 5,
        Orange = 6
    }

    public enum LevelPlayState
    {
        NotStarted = 0,
        Playing = 1,
        Won = 2,
        Lost = 3
    }

    public static class DirectionExtensions
    {
        public static Int2 ToInt2(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return Int2.Up;
                case Direction.Right:
                    return Int2.Right;
                case Direction.Down:
                    return Int2.Down;
                case Direction.Left:
                    return Int2.Left;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}
