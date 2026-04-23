using System;
using System.Collections.Generic;
using System.Linq;
using ArrowPuzzle.Common;
using ArrowPuzzle.Core;
using ArrowPuzzle.Data;
using UnityEngine;

namespace ArrowPuzzle.Runtime
{
    public class GameController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private BoardView boardView;
        [SerializeField] private InputController inputController;
        [SerializeField] private HUDController hudController;
        [SerializeField] private FeedbackController feedbackController;

        [Header("Level Source")]
        [SerializeField] private bool autoStartOnAwake = true;
        [SerializeField] private bool useSampleLevel = false;
        [SerializeField] private TextAsset levelJsonTextAsset;
        [SerializeField] private string resourcesLevelPath = "Levels/phase2_runtime_sample";

        private LevelRuntimeState runtimeState;
        private HintService hintService;
        private LevelData lastLoadedLevelData;

        public LevelRuntimeState RuntimeState => runtimeState;

        private void Awake()
        {
            if (inputController != null)
            {
                inputController.Initialize(this);
            }
        }

        private void Start()
        {
            if (autoStartOnAwake)
            {
                StartGame();
            }
        }

        [ContextMenu("Start Game")]
        public void StartGame()
        {
            try
            {
                LevelData levelData = LoadLevelData();
                StartGame(levelData);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to start game: {exception.Message}");
            }
        }

        public void StartGame(LevelData levelData)
        {
            if (levelData == null)
            {
                throw new ArgumentNullException(nameof(levelData));
            }

            JsonLevelSerializer.EnsureValid(levelData);
            lastLoadedLevelData = levelData;
            runtimeState = new LevelRuntimeState(levelData);
            hintService = new HintService();

            if (boardView != null)
            {
                boardView.Build(runtimeState);
            }

            RefreshRuntimeUI();
            Debug.Log($"Gameplay started. Level: {levelData.levelId}, Size: {levelData.width}x{levelData.height}, Arrow Count: {runtimeState.BoardState.ArrowCount}");
        }

        public void HandleArrowClicked(string arrowId)
        {
            if (runtimeState == null || runtimeState.PlayState == LevelPlayState.Won)
            {
                return;
            }

            Vector3 interactionCenter = Vector3.zero;
            bool hasCenter = boardView != null && boardView.TryGetArrowCenterWorld(arrowId, out interactionCenter);

            bool removed = runtimeState.TryRemoveArrow(arrowId);
            if (removed)
            {
                if (boardView != null)
                {
                    boardView.RemoveArrow(arrowId);
                }

                if (feedbackController != null && hasCenter)
                {
                    feedbackController.PlayRemoveFeedback(interactionCenter);
                }

                RefreshRuntimeUI();

                if (runtimeState.PlayState == LevelPlayState.Won && feedbackController != null)
                {
                    feedbackController.PlayWinFeedback();
                }

                return;
            }

            if (feedbackController != null && hasCenter)
            {
                feedbackController.PlayBlockedFeedback(interactionCenter);
            }
        }

        [ContextMenu("Reload Current Level")]
        public void ReloadCurrentLevel()
        {
            if (lastLoadedLevelData == null)
            {
                StartGame();
                return;
            }

            string json = JsonLevelSerializer.Serialize(lastLoadedLevelData, false);
            StartGame(JsonLevelSerializer.Deserialize(json));
        }

        [ContextMenu("Load Level From TextAsset")]
        public void LoadLevelFromTextAsset()
        {
            if (levelJsonTextAsset == null)
            {
                Debug.LogWarning("LoadLevelFromTextAsset called but no TextAsset is assigned.");
                return;
            }

            try
            {
                LevelData levelData = JsonLevelSerializer.Deserialize(levelJsonTextAsset.text);
                StartGame(levelData);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to load level from TextAsset: {exception.Message}");
            }
        }

        [ContextMenu("Load Level From Resources")]
        public void LoadLevelFromResources()
        {
            try
            {
                LevelData levelData = LoadLevelFromResources(resourcesLevelPath);
                StartGame(levelData);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to load level from Resources: {exception.Message}");
            }
        }

        [ContextMenu("Remove First Hint Arrow")]
        public void RemoveFirstHintArrow()
        {
            if (runtimeState == null || hintService == null)
            {
                return;
            }

            string arrowId = hintService.GetFirstRemovableArrowId(runtimeState);
            if (!string.IsNullOrEmpty(arrowId))
            {
                HandleArrowClicked(arrowId);
            }
        }

        [ContextMenu("Log Removable Arrows")]
        public void LogRemovableArrows()
        {
            if (runtimeState == null)
            {
                return;
            }

            IReadOnlyList<ArrowModel> removable = runtimeState.GetRemovableArrows();
            string result = removable.Count > 0 ? string.Join(", ", removable.Select(a => a.Id)) : "(none)";
            Debug.Log($"Removable Arrows: {result}");
        }

        private void RefreshRuntimeUI()
        {
            if (runtimeState == null)
            {
                return;
            }

            IReadOnlyList<ArrowModel> removableArrows = runtimeState.GetRemovableArrows();
            string hintArrowId = hintService != null ? hintService.GetFirstRemovableArrowId(runtimeState) : string.Empty;

            if (boardView != null)
            {
                boardView.SetRemovableState(removableArrows.Select(a => a.Id));
                boardView.SetHintArrow(hintArrowId);
            }

            if (hudController != null)
            {
                hudController.Refresh(runtimeState, hintArrowId, removableArrows.Count);
            }
        }

        private LevelData LoadLevelData()
        {
            if (!useSampleLevel && levelJsonTextAsset != null)
            {
                return JsonLevelSerializer.Deserialize(levelJsonTextAsset.text);
            }

            if (!string.IsNullOrWhiteSpace(resourcesLevelPath))
            {
                return LoadLevelFromResources(resourcesLevelPath);
            }

            return BuildSampleLevel();
        }

        private static LevelData LoadLevelFromResources(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidOperationException("Resources level path is empty.");
            }

            TextAsset resource = Resources.Load<TextAsset>(path);
            if (resource == null)
            {
                throw new InvalidOperationException($"No TextAsset found in Resources at path '{path}'.");
            }

            return JsonLevelSerializer.Deserialize(resource.text);
        }

        private LevelData BuildSampleLevel()
        {
            return new LevelData
            {
                levelId = "phase2_runtime_line_renderer_sample",
                width = 5,
                height = 5,
                allowedMistakes = 0,
                arrows = new List<ArrowPathData>
                {
                    new ArrowPathData
                    {
                        id = "A",
                        color = ArrowColor.Red,
                        headDirection = Direction.Up,
                        cells = new List<Int2>
                        {
                            new Int2(1, 0),
                            new Int2(1, 1)
                        }
                    },
                    new ArrowPathData
                    {
                        id = "B",
                        color = ArrowColor.Blue,
                        headDirection = Direction.Right,
                        cells = new List<Int2>
                        {
                            new Int2(3, 1),
                            new Int2(3, 2)
                        }
                    },
                    new ArrowPathData
                    {
                        id = "C",
                        color = ArrowColor.Green,
                        headDirection = Direction.Right,
                        cells = new List<Int2>
                        {
                            new Int2(0, 2),
                            new Int2(1, 2)
                        }
                    }
                }
            };
        }
    }
}
