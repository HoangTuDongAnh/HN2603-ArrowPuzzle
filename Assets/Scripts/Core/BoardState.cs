using System;
using System.Collections.Generic;
using ArrowPuzzle.Data;

namespace ArrowPuzzle.Core
{
    public sealed class BoardState
    {
        private readonly Dictionary<string, ArrowModel> arrowsById = new Dictionary<string, ArrowModel>();
        private readonly Dictionary<Int2, string> occupiedByArrowId = new Dictionary<Int2, string>();

        public BoardState(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            Width = width;
            Height = height;
        }

        public int Width { get; }
        public int Height { get; }
        public IReadOnlyCollection<ArrowModel> Arrows => arrowsById.Values;
        public int ArrowCount => arrowsById.Count;

        public static BoardState CreateFromLevel(LevelData levelData)
        {
            if (levelData == null)
            {
                throw new ArgumentNullException(nameof(levelData));
            }

            BoardState boardState = new BoardState(levelData.width, levelData.height);
            if (levelData.arrows == null)
            {
                return boardState;
            }

            for (int i = 0; i < levelData.arrows.Count; i++)
            {
                boardState.AddArrow(new ArrowModel(levelData.arrows[i]));
            }

            return boardState;
        }

        public bool IsInside(Int2 cell)
        {
            return cell.x >= 0 && cell.x < Width && cell.y >= 0 && cell.y < Height;
        }

        public bool IsOccupied(Int2 cell)
        {
            return occupiedByArrowId.ContainsKey(cell);
        }

        public bool TryGetOccupantId(Int2 cell, out string arrowId)
        {
            return occupiedByArrowId.TryGetValue(cell, out arrowId);
        }

        public bool TryGetArrow(string arrowId, out ArrowModel arrow)
        {
            return arrowsById.TryGetValue(arrowId, out arrow);
        }

        public void AddArrow(ArrowModel arrow)
        {
            if (arrow == null)
            {
                throw new ArgumentNullException(nameof(arrow));
            }

            if (arrowsById.ContainsKey(arrow.Id))
            {
                throw new InvalidOperationException($"Arrow id already exists: {arrow.Id}");
            }

            for (int i = 0; i < arrow.OccupiedCells.Count; i++)
            {
                Int2 cell = arrow.OccupiedCells[i];
                if (!IsInside(cell))
                {
                    throw new InvalidOperationException($"Arrow {arrow.Id} has cell outside board: {cell}");
                }

                if (occupiedByArrowId.ContainsKey(cell))
                {
                    throw new InvalidOperationException($"Cell already occupied: {cell}");
                }
            }

            arrowsById.Add(arrow.Id, arrow);

            for (int i = 0; i < arrow.OccupiedCells.Count; i++)
            {
                occupiedByArrowId.Add(arrow.OccupiedCells[i], arrow.Id);
            }
        }

        public bool RemoveArrow(string arrowId)
        {
            if (!arrowsById.TryGetValue(arrowId, out ArrowModel arrow))
            {
                return false;
            }

            for (int i = 0; i < arrow.OccupiedCells.Count; i++)
            {
                occupiedByArrowId.Remove(arrow.OccupiedCells[i]);
            }

            arrowsById.Remove(arrowId);
            return true;
        }
    }
}
