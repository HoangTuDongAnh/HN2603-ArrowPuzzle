using ArrowPuzzle.Data;

namespace ArrowPuzzle.Common
{
    public static class RuntimeLevelSession
    {
        private static LevelData pendingLevelData;

        public static bool HasPendingLevel => pendingLevelData != null;

        public static void SetPendingLevel(LevelData levelData)
        {
            pendingLevelData = JsonLevelSerializer.Clone(levelData);
        }

        public static LevelData ConsumePendingLevel()
        {
            if (pendingLevelData == null)
            {
                return null;
            }

            LevelData clone = JsonLevelSerializer.Clone(pendingLevelData);
            pendingLevelData = null;
            return clone;
        }

        public static void Clear()
        {
            pendingLevelData = null;
        }
    }
}
