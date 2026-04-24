using System.Collections.Generic;
using ArrowPuzzle.Common;
using ArrowPuzzle.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace ArrowPuzzle.LevelEditor
{
    public class LevelEditorController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EditorBoardView boardView;
        [SerializeField] private LevelEditorIO levelEditorIO;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text selectionText;
        [SerializeField] private TMP_Text controlsText;

        [Header("Level Settings")]
        [SerializeField] private int boardWidth = 6;
        [SerializeField] private int boardHeight = 8;
        [SerializeField] private string levelId = "editor_level_01";
        [SerializeField] private ArrowColor currentArrowColor = ArrowColor.Red;
        [SerializeField] private Direction currentHeadDirection = Direction.Up;
        [SerializeField] private int allowedMistakes = 0;

        [Header("Input")]
        [SerializeField] private bool createNewLevelOnStart = true;
        [SerializeField] private bool enableSceneInput = true;

        [Header("Play Test")]
        [SerializeField] private string gameplaySceneName = "Gameplay_Phase2_LineRenderer";
        [SerializeField] private bool autoSaveBeforePlayTest = false;

        private LevelData currentLevel;
        private readonly PathEditTool pathEditTool = new PathEditTool();
        private readonly EditorSelection selection = new EditorSelection();
        private int nextArrowIndex = 1;

        public LevelData CurrentLevel => currentLevel;

        private void Start()
        {
            if (boardView != null)
            {
                boardView.Initialize();
            }

            if (createNewLevelOnStart)
            {
                CreateNewLevel();
            }

            UpdateAllTexts();
            SetStatus("Editor ready.");
        }

        private void Update()
        {
            if (!enableSceneInput || currentLevel == null)
            {
                return;
            }

            if (IsPointerOverUi())
            {
                return;
            }

            HandleMouseInput();
            HandleKeyboardInput();
        }

        public void CreateNewLevel()
        {
            currentLevel = new LevelData
            {
                levelId = string.IsNullOrWhiteSpace(levelId) ? "editor_level_01" : levelId.Trim(),
                width = Mathf.Max(2, boardWidth),
                height = Mathf.Max(2, boardHeight),
                allowedMistakes = Mathf.Max(0, allowedMistakes),
                arrows = new List<ArrowPathData>()
            };

            JsonLevelSerializer.EnsureValid(currentLevel);
            pathEditTool.Clear();
            selection.Clear();
            nextArrowIndex = 1;

            RefreshBoard();
            UpdateAllTexts();
            SetStatus($"Created new level: {currentLevel.levelId}");
        }

        public void SaveCurrentLevel()
        {
            if (currentLevel == null || levelEditorIO == null)
            {
                return;
            }

            JsonLevelSerializer.EnsureValid(currentLevel);
            levelEditorIO.SetFileName(currentLevel.levelId);
            levelEditorIO.SaveLevel(currentLevel);
            UpdateAllTexts();
        }

        public void LoadConfiguredLevel()
        {
            if (levelEditorIO == null)
            {
                return;
            }

            LevelData loaded = levelEditorIO.LoadLevel();
            if (loaded == null)
            {
                return;
            }

            JsonLevelSerializer.EnsureValid(loaded);
            currentLevel = loaded;
            boardWidth = currentLevel.width;
            boardHeight = currentLevel.height;
            levelId = currentLevel.levelId;
            allowedMistakes = currentLevel.allowedMistakes;
            pathEditTool.Clear();
            selection.Clear();
            nextArrowIndex = CalculateNextArrowIndex();

            RefreshBoard();
            UpdateAllTexts();
            SetStatus($"Loaded level: {currentLevel.levelId}");
        }

        public void CopyCurrentLevelJson()
        {
            if (levelEditorIO == null)
            {
                return;
            }

            levelEditorIO.CopyLevelJsonToClipboard(currentLevel);
        }

        public void StartPlayTest()
        {
            if (currentLevel == null)
            {
                SetStatus("Play test failed: current level is null.");
                return;
            }

            try
            {
                JsonLevelSerializer.EnsureValid(currentLevel);

                if (autoSaveBeforePlayTest && levelEditorIO != null)
                {
                    levelEditorIO.SetFileName(currentLevel.levelId);
                    levelEditorIO.SaveLevel(currentLevel);
                }

                RuntimeLevelSession.SetPendingLevel(currentLevel);
                SceneManager.LoadScene(gameplaySceneName);
            }
            catch (System.Exception exception)
            {
                SetStatus($"Play test failed: {exception.Message}");
            }
        }

        public void FinalizePendingArrow()
        {
            if (currentLevel == null || !pathEditTool.CanFinalize)
            {
                return;
            }

            ArrowPathData arrow = pathEditTool.BuildArrow(GenerateNextArrowId(), currentArrowColor, currentHeadDirection);
            if (DoesPathOverlapExistingArrows(arrow.cells))
            {
                SetStatus("Cannot finalize: path overlaps existing arrow.");
                return;
            }

            currentLevel.arrows.Add(arrow);
            JsonLevelSerializer.EnsureValid(currentLevel);
            pathEditTool.Clear();
            selection.Select(arrow.id);

            RefreshBoard();
            UpdateAllTexts();
            SetStatus($"Created arrow: {arrow.id}");
        }

        public void CancelPendingPath()
        {
            pathEditTool.Clear();
            RefreshBoard();
            UpdateAllTexts();
            SetStatus("Pending path canceled.");
        }

        public void RemoveSelectedArrow()
        {
            if (currentLevel == null || !selection.HasSelection || currentLevel.arrows == null)
            {
                return;
            }

            string removedId = selection.SelectedArrowId;
            currentLevel.arrows.RemoveAll(a => a != null && a.id == removedId);
            selection.Clear();
            RefreshBoard();
            UpdateAllTexts();
            SetStatus($"Removed arrow: {removedId}");
        }

        public void SetCurrentArrowColor(int enumValue)
        {
            currentArrowColor = (ArrowColor)enumValue;
            UpdateAllTexts();
        }

        public void SetCurrentHeadDirection(int enumValue)
        {
            currentHeadDirection = (Direction)enumValue;
            UpdateAllTexts();
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!boardView.TryGetGridCellFromScreen(Input.mousePosition, currentLevel, out Int2 cell))
                {
                    return;
                }

                string occupiedArrowId = boardView.GetArrowIdAtCell(currentLevel, cell);
                if (!string.IsNullOrEmpty(occupiedArrowId) && !pathEditTool.IsDrawing)
                {
                    selection.Select(occupiedArrowId);
                    SetStatus($"Selected arrow: {occupiedArrowId}");
                    RefreshBoard();
                    UpdateAllTexts();
                    return;
                }

                if (DoesCellBelongToExistingArrow(cell))
                {
                    SetStatus("Cell is already occupied by another arrow.");
                    return;
                }

                bool appended = pathEditTool.TryStartOrAppend(cell);
                if (!appended)
                {
                    SetStatus("Only adjacent cells can be appended to the path.");
                    return;
                }

                selection.Clear();
                RefreshBoard();
                UpdateAllTexts();
                SetStatus($"Pending path cells: {pathEditTool.PreviewCells.Count}");
            }

            if (Input.GetMouseButtonDown(1) && pathEditTool.CanFinalize)
            {
                FinalizePendingArrow();
            }
        }

        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPendingPath();
            }

            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            {
                RemoveSelectedArrow();
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                FinalizePendingArrow();
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                StartPlayTest();
            }
        }

        private void RefreshBoard()
        {
            if (boardView != null)
            {
                boardView.Rebuild(currentLevel, pathEditTool.PreviewCells, selection.SelectedArrowId);
            }
        }

        private bool DoesCellBelongToExistingArrow(Int2 cell)
        {
            if (currentLevel == null || currentLevel.arrows == null)
            {
                return false;
            }

            foreach (ArrowPathData arrow in currentLevel.arrows)
            {
                if (arrow == null || arrow.cells == null)
                {
                    continue;
                }

                for (int i = 0; i < arrow.cells.Count; i++)
                {
                    if (arrow.cells[i].Equals(cell))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool DoesPathOverlapExistingArrows(IReadOnlyList<Int2> previewCells)
        {
            if (previewCells == null)
            {
                return false;
            }

            for (int i = 0; i < previewCells.Count; i++)
            {
                if (DoesCellBelongToExistingArrow(previewCells[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private string GenerateNextArrowId()
        {
            string id = $"AR_{nextArrowIndex:000}";
            nextArrowIndex++;
            return id;
        }

        private int CalculateNextArrowIndex()
        {
            if (currentLevel == null || currentLevel.arrows == null)
            {
                return 1;
            }

            int max = 0;
            foreach (ArrowPathData arrow in currentLevel.arrows)
            {
                if (arrow == null || string.IsNullOrEmpty(arrow.id))
                {
                    continue;
                }

                if (arrow.id.StartsWith("AR_") && int.TryParse(arrow.id.Substring(3), out int parsed) && parsed > max)
                {
                    max = parsed;
                }
            }

            return max + 1;
        }

        private void UpdateAllTexts()
        {
            if (controlsText != null)
            {
                controlsText.text =
                    "Left Click: Start/extend path or select arrow\n" +
                    "Right Click / Enter: Finalize path\n" +
                    "Delete/Backspace: Remove selected\n" +
                    "Esc: Cancel pending path\n" +
                    "F5: Play test in gameplay scene";
            }

            if (selectionText != null)
            {
                selectionText.text =
                    $"Level: {(currentLevel != null ? currentLevel.levelId : "-")}\n" +
                    $"Selected: {(selection.HasSelection ? selection.SelectedArrowId : "(none)")}\n" +
                    $"Color: {currentArrowColor}\n" +
                    $"Head Dir: {currentHeadDirection}\n" +
                    $"Pending Cells: {pathEditTool.PreviewCells.Count}\n" +
                    $"Arrow Count: {(currentLevel != null && currentLevel.arrows != null ? currentLevel.arrows.Count : 0)}\n" +
                    $"Board: {boardWidth} x {boardHeight}";
            }
        }

        private void SetStatus(string message)
        {
            Debug.Log(message);
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private static bool IsPointerOverUi()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}
