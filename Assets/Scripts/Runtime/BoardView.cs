using System.Collections.Generic;
using ArrowPuzzle.Core;
using ArrowPuzzle.Data;
using UnityEngine;

namespace ArrowPuzzle.Runtime
{
    public class BoardView : MonoBehaviour
    {
        [Header("Optional Prefabs")]
        [SerializeField] private ArrowView arrowViewPrefab;

        [Header("Layout")]
        [SerializeField] private Transform boardRoot;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector2 origin = Vector2.zero;
        [SerializeField] private float zDepthStep = -0.05f;

        private readonly Dictionary<string, ArrowView> arrowViewsById = new Dictionary<string, ArrowView>();
        private readonly HashSet<string> removedArrowIds = new HashSet<string>();

        public float CellSize => cellSize;

        public void Build(LevelRuntimeState runtimeState)
        {
            ClearBoard();

            if (runtimeState == null)
            {
                return;
            }

            if (boardRoot == null)
            {
                boardRoot = transform;
            }

            int index = 0;
            foreach (ArrowModel arrow in runtimeState.BoardState.Arrows)
            {
                ArrowView arrowView = CreateArrowView(arrow.Id);
                arrowView.transform.SetParent(boardRoot, false);
                arrowView.Initialize(arrow, this, index * zDepthStep);
                arrowViewsById[arrow.Id] = arrowView;
                index++;
            }
        }

        public void RemoveArrow(string arrowId)
        {
            removedArrowIds.Add(arrowId);

            if (arrowViewsById.TryGetValue(arrowId, out ArrowView arrowView) && arrowView != null)
            {
                arrowView.PlayRemovedVisual();
                arrowView.gameObject.SetActive(false);
            }
        }

        public void SetRemovableState(IEnumerable<string> removableArrowIds)
        {
            HashSet<string> removableSet = removableArrowIds != null
                ? new HashSet<string>(removableArrowIds)
                : new HashSet<string>();

            foreach (KeyValuePair<string, ArrowView> pair in arrowViewsById)
            {
                if (pair.Value == null || removedArrowIds.Contains(pair.Key))
                {
                    continue;
                }

                pair.Value.SetRemovableVisual(removableSet.Contains(pair.Key));
            }
        }

        public void SetHintArrow(string arrowId)
        {
            foreach (KeyValuePair<string, ArrowView> pair in arrowViewsById)
            {
                if (pair.Value == null || removedArrowIds.Contains(pair.Key))
                {
                    continue;
                }

                pair.Value.SetHintVisual(pair.Key == arrowId);
            }
        }

        public bool TryGetArrowCenterWorld(string arrowId, out Vector3 centerWorld)
        {
            centerWorld = Vector3.zero;
            if (!arrowViewsById.TryGetValue(arrowId, out ArrowView arrowView) || arrowView == null)
            {
                return false;
            }

            centerWorld = arrowView.GetCenterWorld();
            return true;
        }

        public Vector3 GridToWorld(Int2 cell, float zOffset = 0f)
        {
            return new Vector3(
                origin.x + (cell.x * cellSize),
                origin.y + (cell.y * cellSize),
                zOffset);
        }

        private ArrowView CreateArrowView(string arrowId)
        {
            if (arrowViewPrefab != null)
            {
                return Instantiate(arrowViewPrefab);
            }

            GameObject go = new GameObject($"ArrowView_{arrowId}");
            return go.AddComponent<ArrowView>();
        }

        private void ClearBoard()
        {
            removedArrowIds.Clear();
            arrowViewsById.Clear();

            Transform root = boardRoot != null ? boardRoot : transform;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Destroy(root.GetChild(i).gameObject);
            }
        }
    }
}
