using System;
using System.Collections.Generic;
using ArrowPuzzle.Data;

namespace ArrowPuzzle.Core
{
    public sealed class ArrowModel
    {
        private readonly HashSet<Int2> occupiedCellSet;

        public ArrowModel(ArrowPathData sourceData)
        {
            if (sourceData == null)
            {
                throw new ArgumentNullException(nameof(sourceData));
            }

            if (!sourceData.IsValidShape())
            {
                throw new ArgumentException("ArrowPathData must contain a continuous orthogonal path.", nameof(sourceData));
            }

            Id = string.IsNullOrWhiteSpace(sourceData.id) ? Guid.NewGuid().ToString("N") : sourceData.id;
            Color = sourceData.color;
            HeadDirection = sourceData.headDirection;
            OccupiedCells = new List<Int2>(sourceData.cells).AsReadOnly();
            occupiedCellSet = new HashSet<Int2>(sourceData.cells);
        }

        public string Id { get; }
        public ArrowColor Color { get; }
        public Direction HeadDirection { get; }
        public IReadOnlyList<Int2> OccupiedCells { get; }
        public Int2 HeadCell => OccupiedCells[OccupiedCells.Count - 1];

        public bool OccupiesCell(Int2 cell)
        {
            return occupiedCellSet.Contains(cell);
        }
    }
}
