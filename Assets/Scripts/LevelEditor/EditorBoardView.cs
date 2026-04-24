using System.Collections.Generic;
using ArrowPuzzle.Data;
using UnityEngine;

namespace ArrowPuzzle.LevelEditor
{
    public class EditorBoardView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Transform gridRoot;
        [SerializeField] private Transform arrowRoot;
        [SerializeField] private Transform previewRoot;

        [Header("Layout")]
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector2 origin = Vector2.zero;
        [SerializeField] private float zDepthStep = -0.05f;

        [Header("Visuals")]
        [SerializeField] private Color gridCellColor = new Color(0.11f, 0.14f, 0.18f, 0.75f);
        [SerializeField] private Color gridLineColor = new Color(0.25f, 0.30f, 0.37f, 0.75f);
        [SerializeField] private Color previewColor = new Color(1f, 0.89f, 0.28f, 0.95f);
        [SerializeField] private Color selectedTint = new Color(1f, 1f, 1f, 0.35f);

        private static Sprite cachedWhiteSprite;

        public float CellSize => cellSize;

        public void Initialize()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (gridRoot == null)
            {
                GameObject root = new GameObject("GridRoot");
                root.transform.SetParent(transform, false);
                gridRoot = root.transform;
            }

            if (arrowRoot == null)
            {
                GameObject root = new GameObject("ArrowRoot");
                root.transform.SetParent(transform, false);
                arrowRoot = root.transform;
            }

            if (previewRoot == null)
            {
                GameObject root = new GameObject("PreviewRoot");
                root.transform.SetParent(transform, false);
                previewRoot = root.transform;
            }
        }

        public void Rebuild(LevelData levelData, IReadOnlyList<Int2> previewCells, string selectedArrowId)
        {
            Initialize();
            ClearChildren(gridRoot);
            ClearChildren(arrowRoot);
            ClearChildren(previewRoot);

            if (levelData == null)
            {
                return;
            }

            BuildGrid(levelData.width, levelData.height);
            BuildArrows(levelData, selectedArrowId);
            BuildPreview(previewCells);
        }

        public bool TryGetGridCellFromScreen(Vector2 screenPosition, LevelData levelData, out Int2 cell)
        {
            cell = default;

            if (levelData == null)
            {
                return false;
            }

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null)
                {
                    return false;
                }
            }

            Vector3 world = targetCamera.ScreenToWorldPoint(screenPosition);
            int x = Mathf.FloorToInt((world.x - origin.x) / cellSize + 0.5f);
            int y = Mathf.FloorToInt((world.y - origin.y) / cellSize + 0.5f);

            cell = new Int2(x, y);
            return IsInBounds(levelData, cell);
        }

        public bool IsInBounds(LevelData levelData, Int2 cell)
        {
            return cell.x >= 0 && cell.x < levelData.width && cell.y >= 0 && cell.y < levelData.height;
        }

        public Vector3 GridToWorld(Int2 cell, float zOffset = 0f)
        {
            return new Vector3(origin.x + cell.x * cellSize, origin.y + cell.y * cellSize, zOffset);
        }

        public string GetArrowIdAtCell(LevelData levelData, Int2 cell)
        {
            if (levelData == null || levelData.arrows == null)
            {
                return string.Empty;
            }

            for (int i = 0; i < levelData.arrows.Count; i++)
            {
                ArrowPathData arrow = levelData.arrows[i];
                if (arrow == null || arrow.cells == null)
                {
                    continue;
                }

                for (int j = 0; j < arrow.cells.Count; j++)
                {
                    if (arrow.cells[j].Equals(cell))
                    {
                        return arrow.id;
                    }
                }
            }

            return string.Empty;
        }

        private void BuildGrid(int width, int height)
        {
            Sprite sprite = GetWhiteSprite();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    GameObject cellGo = new GameObject($"GridCell_{x}_{y}");
                    cellGo.transform.SetParent(gridRoot, false);
                    cellGo.transform.position = GridToWorld(new Int2(x, y), 0.20f);

                    SpriteRenderer sr = cellGo.AddComponent<SpriteRenderer>();
                    sr.sprite = sprite;
                    sr.color = gridCellColor;
                    sr.sortingOrder = -10;
                    cellGo.transform.localScale = Vector3.one * cellSize * 0.92f;
                }
            }

            for (int x = 0; x <= width; x++)
            {
                CreateGridLine(
                    new Vector3(origin.x + (x - 0.5f) * cellSize, origin.y - 0.5f * cellSize, 0.15f),
                    new Vector3(origin.x + (x - 0.5f) * cellSize, origin.y + (height - 0.5f) * cellSize, 0.15f));
            }

            for (int y = 0; y <= height; y++)
            {
                CreateGridLine(
                    new Vector3(origin.x - 0.5f * cellSize, origin.y + (y - 0.5f) * cellSize, 0.15f),
                    new Vector3(origin.x + (width - 0.5f) * cellSize, origin.y + (y - 0.5f) * cellSize, 0.15f));
            }
        }

        private void CreateGridLine(Vector3 a, Vector3 b)
        {
            GameObject lineGo = new GameObject("GridLine");
            lineGo.transform.SetParent(gridRoot, false);

            LineRenderer lr = lineGo.AddComponent<LineRenderer>();
            SetupLineRenderer(lr, gridLineColor, 0.03f, -9);
            lr.positionCount = 2;
            lr.SetPosition(0, a);
            lr.SetPosition(1, b);
        }

        private void BuildArrows(LevelData levelData, string selectedArrowId)
        {
            if (levelData.arrows == null)
            {
                return;
            }

            for (int i = 0; i < levelData.arrows.Count; i++)
            {
                ArrowPathData arrow = levelData.arrows[i];
                if (arrow == null || arrow.cells == null || arrow.cells.Count == 0)
                {
                    continue;
                }

                GameObject arrowGo = new GameObject($"Arrow_{arrow.id}");
                arrowGo.transform.SetParent(arrowRoot, false);

                LineRenderer lr = arrowGo.AddComponent<LineRenderer>();
                Color arrowColor = ResolveColor(arrow.color);
                if (selectedArrowId == arrow.id)
                {
                    arrowColor = Color.Lerp(arrowColor, selectedTint, 0.35f);
                }

                SetupLineRenderer(lr, arrowColor, selectedArrowId == arrow.id ? 0.26f : 0.18f, 2);
                lr.positionCount = arrow.cells.Count;

                for (int p = 0; p < arrow.cells.Count; p++)
                {
                    lr.SetPosition(p, GridToWorld(arrow.cells[p], i * zDepthStep));
                }

                CreateHeadMarker(arrowGo.transform, arrow, arrowColor, selectedArrowId == arrow.id);
            }
        }

        private void BuildPreview(IReadOnlyList<Int2> previewCells)
        {
            if (previewCells == null || previewCells.Count == 0)
            {
                return;
            }

            GameObject previewGo = new GameObject("PreviewPath");
            previewGo.transform.SetParent(previewRoot, false);

            LineRenderer lr = previewGo.AddComponent<LineRenderer>();
            SetupLineRenderer(lr, previewColor, 0.16f, 5);
            lr.positionCount = previewCells.Count;

            for (int i = 0; i < previewCells.Count; i++)
            {
                Vector3 world = GridToWorld(previewCells[i], -0.3f);
                lr.SetPosition(i, world);

                GameObject marker = new GameObject($"PreviewCell_{previewCells[i].x}_{previewCells[i].y}");
                marker.transform.SetParent(previewRoot, false);
                marker.transform.position = world;

                SpriteRenderer sr = marker.AddComponent<SpriteRenderer>();
                sr.sprite = GetWhiteSprite();
                sr.color = new Color(previewColor.r, previewColor.g, previewColor.b, i == previewCells.Count - 1 ? 1f : 0.65f);
                sr.sortingOrder = 6;
                marker.transform.localScale = Vector3.one * cellSize * (i == previewCells.Count - 1 ? 0.34f : 0.22f);
            }
        }

        private void CreateHeadMarker(Transform parent, ArrowPathData arrow, Color color, bool selected)
        {
            Int2 headCell = arrow.cells[arrow.cells.Count - 1];
            GameObject headGo = new GameObject("Head");
            headGo.transform.SetParent(parent, false);
            headGo.transform.position = GridToWorld(headCell, -0.02f);

            SpriteRenderer sr = headGo.AddComponent<SpriteRenderer>();
            sr.sprite = GetWhiteSprite();
            sr.color = Color.Lerp(color, Color.white, 0.22f);
            sr.sortingOrder = 3;
            headGo.transform.localScale = Vector3.one * cellSize * (selected ? 0.44f : 0.38f);
        }

        private void SetupLineRenderer(LineRenderer lr, Color color, float width, int sortingOrder)
        {
            lr.useWorldSpace = true;
            lr.alignment = LineAlignment.View;
            lr.textureMode = LineTextureMode.Stretch;
            lr.numCapVertices = 8;
            lr.numCornerVertices = 8;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.startColor = color;
            lr.endColor = color;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.sortingOrder = sortingOrder;
        }

        private void ClearChildren(Transform root)
        {
            if (root == null)
            {
                return;
            }

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Destroy(root.GetChild(i).gameObject);
            }
        }

        private static Color ResolveColor(ArrowColor arrowColor)
        {
            switch (arrowColor)
            {
                case ArrowColor.Red:
                    return new Color(0.93f, 0.35f, 0.35f, 1f);
                case ArrowColor.Blue:
                    return new Color(0.29f, 0.58f, 0.96f, 1f);
                case ArrowColor.Green:
                    return new Color(0.35f, 0.82f, 0.46f, 1f);
                case ArrowColor.Yellow:
                    return new Color(0.96f, 0.82f, 0.28f, 1f);
                case ArrowColor.Purple:
                    return new Color(0.71f, 0.46f, 0.89f, 1f);
                case ArrowColor.Orange:
                    return new Color(0.97f, 0.57f, 0.25f, 1f);
                default:
                    return Color.white;
            }
        }

        private static Sprite GetWhiteSprite()
        {
            if (cachedWhiteSprite != null)
            {
                return cachedWhiteSprite;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.filterMode = FilterMode.Point;

            cachedWhiteSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            cachedWhiteSprite.name = "EditorWhiteSprite";
            return cachedWhiteSprite;
        }
    }
}
