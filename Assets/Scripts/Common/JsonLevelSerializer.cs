using System;
using ArrowPuzzle.Data;
using UnityEngine;

namespace ArrowPuzzle.Common
{
    public static class JsonLevelSerializer
    {
        public static LevelData Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("JSON content is null or empty.", nameof(json));
            }

            LevelData levelData = JsonUtility.FromJson<LevelData>(json);
            if (levelData == null)
            {
                throw new InvalidOperationException("Failed to deserialize LevelData from JSON.");
            }

            EnsureValid(levelData);
            return levelData;
        }

        public static string Serialize(LevelData levelData, bool prettyPrint = true)
        {
            if (levelData == null)
            {
                throw new ArgumentNullException(nameof(levelData));
            }

            EnsureValid(levelData);
            return JsonUtility.ToJson(levelData, prettyPrint);
        }

        public static void EnsureValid(LevelData levelData)
        {
            if (levelData == null)
            {
                throw new ArgumentNullException(nameof(levelData));
            }

            if (levelData.width <= 0)
            {
                throw new InvalidOperationException("Level width must be greater than zero.");
            }

            if (levelData.height <= 0)
            {
                throw new InvalidOperationException("Level height must be greater than zero.");
            }

            if (levelData.arrows == null)
            {
                throw new InvalidOperationException("Level arrows collection cannot be null.");
            }

            for (int i = 0; i < levelData.arrows.Count; i++)
            {
                ArrowPathData arrow = levelData.arrows[i];
                if (arrow == null)
                {
                    throw new InvalidOperationException($"Arrow at index {i} is null.");
                }

                if (!arrow.IsValidShape())
                {
                    throw new InvalidOperationException($"Arrow '{arrow.id}' contains an invalid path shape.");
                }

                for (int j = 0; j < arrow.cells.Count; j++)
                {
                    if (!levelData.IsInside(arrow.cells[j]))
                    {
                        throw new InvalidOperationException($"Arrow '{arrow.id}' has a cell outside the board: {arrow.cells[j]}");
                    }
                }
            }
        }
    }
}
