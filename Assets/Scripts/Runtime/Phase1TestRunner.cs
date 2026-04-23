using System.Collections.Generic;
using System.Linq;
using ArrowPuzzle.Core;
using ArrowPuzzle.Data;
using UnityEngine;

namespace ArrowPuzzle.Runtime
{
    public class Phase1TestSceneController : MonoBehaviour
    {
        [Header("Auto Run")]
        [SerializeField] private bool runOnStart = true;

        private LevelRuntimeState runtimeState;
        private HintService hintService;

        private void Start()
        {
            if (!runOnStart)
            {
                return;
            }

            RunPhase1Test();
        }

        [ContextMenu("Run Phase 1 Test")]
        public void RunPhase1Test()
        {
            Debug.Log("========== PHASE 1 TEST START ==========");

            LevelData levelData = BuildSampleLevel();
            runtimeState = new LevelRuntimeState(levelData);
            hintService = new HintService();

            Debug.Log($"Level created: {levelData.levelId}");
            Debug.Log($"Board Size: {levelData.width} x {levelData.height}");
            Debug.Log($"Initial Arrow Count: {runtimeState.BoardState.ArrowCount}");

            LogAllArrows();
            LogRemovableArrows("Initial removable arrows");

            Debug.Log("Step 1 - Try removing blocked arrow C first (expected: false)");
            bool removeCFirst = runtimeState.TryRemoveArrow("C");
            Debug.Log($"TryRemoveArrow(\"C\") => {removeCFirst}");
            LogRemovableArrows("After trying C");

            Debug.Log("Step 2 - Remove arrow B (expected: true)");
            bool removeB = runtimeState.TryRemoveArrow("B");
            Debug.Log($"TryRemoveArrow(\"B\") => {removeB}");
            LogRemovableArrows("After removing B");

            Debug.Log("Step 3 - Remove arrow C again (expected: true now)");
            bool removeCSecond = runtimeState.TryRemoveArrow("C");
            Debug.Log($"TryRemoveArrow(\"C\") => {removeCSecond}");
            LogRemovableArrows("After removing C");

            Debug.Log("Step 4 - Remove arrow A (expected: true)");
            bool removeA = runtimeState.TryRemoveArrow("A");
            Debug.Log($"TryRemoveArrow(\"A\") => {removeA}");
            LogRemovableArrows("After removing A");

            Debug.Log($"Removed IDs: {string.Join(", ", runtimeState.RemovedArrowIds)}");
            Debug.Log($"Final Arrow Count: {runtimeState.BoardState.ArrowCount}");
            Debug.Log($"PlayState: {runtimeState.PlayState}");
            Debug.Log("========== PHASE 1 TEST END ==========");
        }

        [ContextMenu("Log Current Removable Arrows")]
        public void LogCurrentRemovableArrows()
        {
            if (runtimeState == null)
            {
                Debug.LogWarning("RuntimeState is null. Run test first.");
                return;
            }

            LogRemovableArrows("Current removable arrows");
        }

        [ContextMenu("Try Remove Hint Arrow")]
        public void TryRemoveHintArrow()
        {
            if (runtimeState == null)
            {
                Debug.LogWarning("RuntimeState is null. Run test first.");
                return;
            }

            string hintArrowId = hintService.GetFirstRemovableArrowId(runtimeState);
            if (string.IsNullOrEmpty(hintArrowId))
            {
                Debug.Log("No removable arrow available.");
                return;
            }

            bool result = runtimeState.TryRemoveArrow(hintArrowId);
            Debug.Log($"TryRemoveHintArrow => ID: {hintArrowId}, Result: {result}");
            LogRemovableArrows("After TryRemoveHintArrow");
        }

        private void LogAllArrows()
        {
            if (runtimeState == null)
            {
                return;
            }

            List<ArrowModel> arrows = runtimeState.BoardState.Arrows
                .OrderBy(a => a.Id)
                .ToList();

            foreach (ArrowModel arrow in arrows)
            {
                string cells = string.Join(" -> ", arrow.OccupiedCells.Select(c => c.ToString()));
                Debug.Log($"Arrow {arrow.Id} | Head: {arrow.HeadCell} | Dir: {arrow.HeadDirection} | Cells: {cells}");
            }
        }

        private void LogRemovableArrows(string title)
        {
            if (runtimeState == null)
            {
                return;
            }

            IReadOnlyList<ArrowModel> removable = runtimeState.GetRemovableArrows();
            string removableIds = removable.Count > 0
                ? string.Join(", ", removable.Select(a => a.Id))
                : "(none)";

            string firstHint = hintService != null && hintService.HasAnyHint(runtimeState)
                ? hintService.GetFirstRemovableArrowId(runtimeState)
                : "(none)";

            Debug.Log($"{title}: {removableIds}");
            Debug.Log($"Hint Arrow: {firstHint}");
        }

        private LevelData BuildSampleLevel()
        {
            LevelData level = new LevelData
            {
                levelId = "phase1_sample_test",
                width = 5,
                height = 5,
                allowedMistakes = 0,
                arrows = new List<ArrowPathData>()
            };

            // Arrow A:
            // cells: (1,0) -> (1,1)
            // head at (1,1), direction Up
            // path to edge is clear => removable from start
            level.arrows.Add(new ArrowPathData
            {
                id = "A",
                color = ArrowColor.Red,
                headDirection = Direction.Up,
                cells = new List<Int2>
                {
                    new Int2(1, 0),
                    new Int2(1, 1)
                }
            });

            // Arrow B:
            // cells: (3,1) -> (3,2)
            // head at (3,2), direction Right
            // path to edge is clear => removable from start
            level.arrows.Add(new ArrowPathData
            {
                id = "B",
                color = ArrowColor.Blue,
                headDirection = Direction.Right,
                cells = new List<Int2>
                {
                    new Int2(3, 1),
                    new Int2(3, 2)
                }
            });

            // Arrow C:
            // cells: (0,2) -> (1,2)
            // head at (1,2), direction Right
            // cursor goes through (2,2) empty, then (3,2) occupied by B => blocked at start
            // after B is removed, C becomes removable
            level.arrows.Add(new ArrowPathData
            {
                id = "C",
                color = ArrowColor.Green,
                headDirection = Direction.Right,
                cells = new List<Int2>
                {
                    new Int2(0, 2),
                    new Int2(1, 2)
                }
            });

            return level;
        }
    }
}