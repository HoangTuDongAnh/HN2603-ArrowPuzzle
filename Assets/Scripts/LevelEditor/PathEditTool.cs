using System.Collections.Generic;
using ArrowPuzzle.Data;

namespace ArrowPuzzle.LevelEditor
{
    [System.Serializable]
    public class PathEditTool
    {
        private readonly List<Int2> previewCells = new List<Int2>();

        public IReadOnlyList<Int2> PreviewCells => previewCells;
        public bool IsDrawing => previewCells.Count > 0;
        public bool CanFinalize => previewCells.Count >= 2;

        public void Clear()
        {
            previewCells.Clear();
        }

        public bool TryStartOrAppend(Int2 cell)
        {
            if (previewCells.Count == 0)
            {
                previewCells.Add(cell);
                return true;
            }

            Int2 last = previewCells[previewCells.Count - 1];
            if (last.Equals(cell))
            {
                return false;
            }

            if (previewCells.Contains(cell))
            {
                return false;
            }

            int manhattan = System.Math.Abs(last.x - cell.x) + System.Math.Abs(last.y - cell.y);
            if (manhattan != 1)
            {
                return false;
            }

            previewCells.Add(cell);
            return true;
        }

        public ArrowPathData BuildArrow(string arrowId, ArrowColor color, Direction headDirection)
        {
            ArrowPathData data = new ArrowPathData
            {
                id = arrowId,
                color = color,
                headDirection = headDirection,
                cells = new List<Int2>(previewCells)
            };

            return data;
        }
    }
}
