using System;
using System.Collections.Generic;
using ArrowPuzzle.Data;

namespace ArrowPuzzle.Core
{
    public sealed class LevelRuntimeState
    {
        private readonly PuzzleRuleService ruleService;
        private readonly List<string> removedArrowIds = new List<string>();

        public LevelRuntimeState(LevelData levelData, PuzzleRuleService ruleService = null)
        {
            LevelData = levelData ?? throw new ArgumentNullException(nameof(levelData));
            BoardState = BoardState.CreateFromLevel(levelData);
            this.ruleService = ruleService ?? new PuzzleRuleService();
            PlayState = LevelPlayState.NotStarted;
        }

        public LevelData LevelData { get; }
        public BoardState BoardState { get; }
        public LevelPlayState PlayState { get; private set; }
        public IReadOnlyList<string> RemovedArrowIds => removedArrowIds;

        public void StartLevel()
        {
            if (PlayState == LevelPlayState.NotStarted)
            {
                PlayState = LevelPlayState.Playing;
            }
        }

        public bool CanRemove(string arrowId)
        {
            if (!BoardState.TryGetArrow(arrowId, out ArrowModel arrow))
            {
                return false;
            }

            return ruleService.CanRemoveArrow(BoardState, arrow);
        }

        public bool TryRemoveArrow(string arrowId)
        {
            if (PlayState == LevelPlayState.NotStarted)
            {
                StartLevel();
            }

            if (PlayState != LevelPlayState.Playing)
            {
                return false;
            }

            if (!BoardState.TryGetArrow(arrowId, out ArrowModel arrow))
            {
                return false;
            }

            if (!ruleService.CanRemoveArrow(BoardState, arrow))
            {
                return false;
            }

            if (!BoardState.RemoveArrow(arrowId))
            {
                return false;
            }

            removedArrowIds.Add(arrowId);
            if (BoardState.ArrowCount == 0)
            {
                PlayState = LevelPlayState.Won;
            }

            return true;
        }

        public IReadOnlyList<ArrowModel> GetRemovableArrows()
        {
            return ruleService.GetRemovableArrows(BoardState);
        }
    }
}
