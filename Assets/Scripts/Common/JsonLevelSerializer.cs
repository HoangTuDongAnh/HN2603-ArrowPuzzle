using System;
using System.Collections.Generic;
using ArrowPuzzle.Data;
using UnityEngine;

namespace ArrowPuzzle.Common
{
    public static class JsonLevelSerializer
    {
        public static string Serialize(LevelData levelData, bool prettyPrint = true)
        {
            if (levelData == null)
            {
                return string.Empty;
            }

            EnsureValid(levelData);
            return JsonUtility.ToJson(levelData, prettyPrint);
        }

        public static LevelData Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            LevelData levelData = JsonUtility.FromJson<LevelData>(json);
            if (levelData == null)
            {
                return null;
            }

            EnsureValid(levelData);
            return levelData;
        }

        public static LevelData Deserialize(TextAsset textAsset)
        {
            if (textAsset == null)
            {
                return null;
            }

            return Deserialize(textAsset.text);
        }

        public static LevelData Clone(LevelData levelData)
        {
            if (levelData == null)
            {
                return null;
            }

            return Deserialize(Serialize(levelData, false));
        }

        public static void EnsureValid(LevelData levelData)
        {
            if (levelData == null)
            {
                throw new ArgumentNullException(nameof(levelData));
            }

            if (string.IsNullOrWhiteSpace(levelData.levelId))
            {
                levelData.levelId = "level_001";
            }
            else
            {
                levelData.levelId = levelData.levelId.Trim();
            }

            levelData.width = Mathf.Max(1, levelData.width);
            levelData.height = Mathf.Max(1, levelData.height);
            levelData.allowedMistakes = Mathf.Max(0, levelData.allowedMistakes);

            if (levelData.arrows == null)
            {
                levelData.arrows = new List<ArrowPathData>();
            }

            HashSet<string> seenIds = new HashSet<string>();
            Dictionary<Int2, string> occupiedCells = new Dictionary<Int2, string>();

            for (int i = 0; i < levelData.arrows.Count; i++)
            {
                ArrowPathData arrow = levelData.arrows[i];
                if (arrow == null)
                {
                    throw new InvalidOperationException($"Arrow at index {i} is null.");
                }

                if (string.IsNullOrWhiteSpace(arrow.id))
                {
                    arrow.id = $"AR_{i + 1:000}";
                }
                else
                {
                    arrow.id = arrow.id.Trim();
                }

                if (!seenIds.Add(arrow.id))
                {
                    throw new InvalidOperationException($"Duplicate arrow id detected: '{arrow.id}'.");
                }

                if (arrow.cells == null)
                {
                    arrow.cells = new List<Int2>();
                }

                if (arrow.cells.Count == 0)
                {
                    throw new InvalidOperationException($"Arrow '{arrow.id}' has no cells.");
                }

                if (!arrow.IsValidShape())
                {
                    throw new InvalidOperationException($"Arrow '{arrow.id}' has a non-contiguous path.");
                }

                HashSet<Int2> uniqueCellsInArrow = new HashSet<Int2>();
                for (int cellIndex = 0; cellIndex < arrow.cells.Count; cellIndex++)
                {
                    Int2 cell = arrow.cells[cellIndex];
                    if (!levelData.IsInside(cell))
                    {
                        throw new InvalidOperationException($"Arrow '{arrow.id}' has out-of-bounds cell {cell}.");
                    }

                    if (!uniqueCellsInArrow.Add(cell))
                    {
                        throw new InvalidOperationException($"Arrow '{arrow.id}' contains duplicated cell {cell}.");
                    }

                    if (occupiedCells.TryGetValue(cell, out string otherArrowId))
                    {
                        throw new InvalidOperationException($"Cell {cell} is shared by arrows '{otherArrowId}' and '{arrow.id}'.");
                    }

                    occupiedCells[cell] = arrow.id;
                }
            }
        }
    }
}
