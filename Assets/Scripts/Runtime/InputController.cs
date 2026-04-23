using UnityEngine;

namespace ArrowPuzzle.Runtime
{
    public class InputController : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private LayerMask inputLayerMask = Physics2D.DefaultRaycastLayers;
        [SerializeField] private bool allowMouse = true;
        [SerializeField] private bool allowTouch = true;

        private GameController gameController;

        public void Initialize(GameController owner)
        {
            gameController = owner;
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (gameController == null)
            {
                return;
            }

            if (allowMouse && Input.GetMouseButtonDown(0))
            {
                TryPickArrow(Input.mousePosition);
            }

            if (allowTouch && Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    TryPickArrow(touch.position);
                }
            }
        }

        private void TryPickArrow(Vector2 screenPosition)
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null)
                {
                    return;
                }
            }

            Vector3 worldPosition = targetCamera.ScreenToWorldPoint(screenPosition);
            Vector2 point = new Vector2(worldPosition.x, worldPosition.y);
            Collider2D hit = Physics2D.OverlapPoint(point, inputLayerMask);
            if (hit == null)
            {
                return;
            }

            ArrowView arrowView = hit.GetComponentInParent<ArrowView>();
            if (arrowView == null)
            {
                return;
            }

            gameController.HandleArrowClicked(arrowView.ArrowId);
        }
    }
}
