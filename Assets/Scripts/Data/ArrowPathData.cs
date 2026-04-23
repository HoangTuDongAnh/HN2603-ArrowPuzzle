using System;
using System.Collections.Generic;

namespace ArrowPuzzle.Data
{
    [Serializable]
    public class ArrowPathData
    {
        public string id;
        public ArrowColor color = ArrowColor.None;
        public Direction headDirection = Direction.Up;
        public List<Int2> cells = new List<Int2>();

        public bool IsValidShape()
        {
            if (cells == null || cells.Count == 0)
            {
                return false;
            }

            for (int i = 1; i < cells.Count; i++)
            {
                Int2 delta = cells[i] - cells[i - 1];
                int manhattan = Math.Abs(delta.x) + Math.Abs(delta.y);
                if (manhattan != 1)
                {
                    return false;
                }
            }

            return true;
        }

        public Int2 GetHeadCell()
        {
            return cells != null && cells.Count > 0 ? cells[cells.Count - 1] : Int2.Zero;
        }

        public bool ContainsCell(Int2 cell)
        {
            if (cells == null)
            {
                return false;
            }

            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i] == cell)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
