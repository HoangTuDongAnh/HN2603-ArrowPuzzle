using System.Collections.Generic;
using ArrowPuzzle.Core;
using ArrowPuzzle.Data;
using UnityEngine;

namespace ArrowPuzzle.Runtime
{
    [RequireComponent(typeof(LineRenderer))]
    public class ArrowView : MonoBehaviour
    {
        [Header("Line Renderer")]
        [SerializeField] private float baseLineWidth = 0.22f;
        [SerializeField] private Material defaultLineMaterial;
        [SerializeField] private int lineSortingOrder = 2;
        [SerializeField] private bool useWorldSpace = false;

        [Header("Head Visual")]
        [SerializeField] private float headScale = 0.38f;
        [SerializeField] private Material defaultHeadMaterial;
        [SerializeField] private int headSortingOrder = 3;

        [Header("Input")]
        [SerializeField] private float clickColliderWidthMultiplier = 2.2f;

        private ArrowModel arrowModel;
        private BoardView boardView;
        private LineRenderer lineRenderer;
        private GameObject headObject;
        private SpriteRenderer headRenderer;
        private readonly List<BoxCollider2D> segmentColliders = new List<BoxCollider2D>();

        private bool isRemovable;
        private bool isHinted;
        private Color baseColor;
        private float zOffset;

        public string ArrowId => arrowModel != null ? arrowModel.Id : string.Empty;

        public void Initialize(ArrowModel model, BoardView ownerBoardView, float localZOffset)
        {
            arrowModel = model;
            boardView = ownerBoardView;
            zOffset = localZOffset;
            name = $"Arrow_{model.Id}";

            lineRenderer = GetComponent<LineRenderer>();
            ConfigureLineRenderer();
            BuildLine();
            BuildHead();
            BuildHitColliders();
            ApplyCurrentVisualState();
        }

        public void SetRemovableVisual(bool removable)
        {
            isRemovable = removable;
            ApplyCurrentVisualState();
        }

        public void SetHintVisual(bool hinted)
        {
            isHinted = hinted;
            ApplyCurrentVisualState();
        }

        public void PlayRemovedVisual()
        {
            SetAlpha(0.12f);
        }

        public Vector3 GetCenterWorld()
        {
            if (arrowModel == null || arrowModel.OccupiedCells.Count == 0)
            {
                return transform.position;
            }

            Vector3 sum = Vector3.zero;
            for (int i = 0; i < arrowModel.OccupiedCells.Count; i++)
            {
                sum += boardView.GridToWorld(arrowModel.OccupiedCells[i], zOffset);
            }

            return sum / arrowModel.OccupiedCells.Count;
        }

        private void ConfigureLineRenderer()
        {
            lineRenderer.useWorldSpace = useWorldSpace;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.textureMode = LineTextureMode.Stretch;
            lineRenderer.numCapVertices = 6;
            lineRenderer.numCornerVertices = 6;
            lineRenderer.sortingOrder = lineSortingOrder;
            lineRenderer.widthMultiplier = baseLineWidth;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;

            if (defaultLineMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    defaultLineMaterial = new Material(shader);
                }
            }

            if (defaultLineMaterial != null)
            {
                lineRenderer.material = defaultLineMaterial;
            }
        }

        private void BuildLine()
        {
            baseColor = ResolveColor(arrowModel.Color);
            lineRenderer.positionCount = arrowModel.OccupiedCells.Count;

            for (int i = 0; i < arrowModel.OccupiedCells.Count; i++)
            {
                Vector3 world = boardView.GridToWorld(arrowModel.OccupiedCells[i], zOffset);
                Vector3 local = useWorldSpace ? world : transform.InverseTransformPoint(world);
                lineRenderer.SetPosition(i, local);
            }

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(baseColor, 0f),
                    new GradientColorKey(baseColor, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                });
            lineRenderer.colorGradient = gradient;
        }

        private void BuildHead()
        {
            headObject = new GameObject("Head");
            headObject.transform.SetParent(transform, false);
            headRenderer = headObject.AddComponent<SpriteRenderer>();
            headRenderer.sprite = ArrowRuntimeVisualUtility.GetWhiteSprite();
            headRenderer.sortingOrder = headSortingOrder;

            if (defaultHeadMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    defaultHeadMaterial = new Material(shader);
                }
            }

            if (defaultHeadMaterial != null)
            {
                headRenderer.material = defaultHeadMaterial;
            }

            Vector3 headWorld = boardView.GridToWorld(arrowModel.HeadCell, zOffset - 0.001f);
            if (useWorldSpace)
            {
                headObject.transform.position = headWorld;
            }
            else
            {
                headObject.transform.localPosition = transform.InverseTransformPoint(headWorld);
            }

            headObject.transform.localScale = new Vector3(headScale, headScale, 1f);
            headObject.transform.localRotation = Quaternion.Euler(0f, 0f, DirectionToZRotation(arrowModel.HeadDirection));
        }

        private void BuildHitColliders()
        {
            foreach (BoxCollider2D collider in segmentColliders)
            {
                if (collider != null)
                {
                    Destroy(collider);
                }
            }
            segmentColliders.Clear();

            for (int i = 0; i < arrowModel.OccupiedCells.Count; i++)
            {
                Int2 cell = arrowModel.OccupiedCells[i];
                GameObject hitNode = new GameObject($"Hit_{cell.x}_{cell.y}");
                hitNode.transform.SetParent(transform, false);

                Vector3 world = boardView.GridToWorld(cell, zOffset);
                if (useWorldSpace)
                {
                    hitNode.transform.position = world;
                }
                else
                {
                    hitNode.transform.localPosition = transform.InverseTransformPoint(world);
                }

                BoxCollider2D box = hitNode.AddComponent<BoxCollider2D>();
                box.isTrigger = true;
                box.size = Vector2.one * (boardView.CellSize * clickColliderWidthMultiplier);
                segmentColliders.Add(box);
            }
        }

        private void ApplyCurrentVisualState()
        {
            if (lineRenderer == null)
            {
                return;
            }

            float alpha = isRemovable ? 1f : 0.5f;
            float width = baseLineWidth;

            if (isRemovable)
            {
                width *= 1.08f;
            }

            if (isHinted)
            {
                width *= 1.18f;
            }

            lineRenderer.widthMultiplier = width;
            SetAlpha(alpha);

            if (headObject != null)
            {
                float scale = headScale * (isHinted ? 1.16f : 1f);
                headObject.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        private void SetAlpha(float alpha)
        {
            Color lineColor = baseColor;
            if (isHinted)
            {
                lineColor = Color.Lerp(baseColor, Color.white, 0.18f);
            }

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(lineColor, 0f),
                    new GradientColorKey(lineColor, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(alpha, 0f),
                    new GradientAlphaKey(alpha, 1f)
                });
            lineRenderer.colorGradient = gradient;

            if (headRenderer != null)
            {
                Color headColor = Color.Lerp(lineColor, Color.white, 0.2f);
                headColor.a = alpha;
                headRenderer.color = headColor;
            }
        }

        private static float DirectionToZRotation(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return 0f;
                case Direction.Right: return -90f;
                case Direction.Down: return 180f;
                case Direction.Left: return 90f;
                default: return 0f;
            }
        }

        private static Color ResolveColor(ArrowColor arrowColor)
        {
            switch (arrowColor)
            {
                case ArrowColor.Red: return new Color(0.93f, 0.35f, 0.35f, 1f);
                case ArrowColor.Blue: return new Color(0.29f, 0.58f, 0.96f, 1f);
                case ArrowColor.Green: return new Color(0.35f, 0.82f, 0.46f, 1f);
                case ArrowColor.Yellow: return new Color(0.96f, 0.82f, 0.28f, 1f);
                case ArrowColor.Purple: return new Color(0.71f, 0.46f, 0.89f, 1f);
                case ArrowColor.Orange: return new Color(0.97f, 0.57f, 0.25f, 1f);
                default: return Color.white;
            }
        }
    }
}
