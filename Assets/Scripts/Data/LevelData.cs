using System;
using System.Collections.Generic;

namespace ArrowPuzzle.Data
{
    [Serializable]
    public class LevelData
    {
        public string levelId = "level_001";
        public int width = 5;
        public int height = 5;
        public int allowedMistakes = 0;
        public List<ArrowPathData> arrows = new List<ArrowPathData>();

        public bool IsInside(Int2 cell)
        {
            return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
        }
    }
}
