using ArrowPuzzle.Data;
using UnityEngine;

namespace ArrowPuzzle.Runtime
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class ArrowSegmentView : MonoBehaviour
    {
        private static Sprite cachedWhiteSprite;

        private SpriteRenderer spriteRenderer;
        private BoxCollider2D boxCollider;
        private ArrowView ownerArrowView;
        private Vector3 baseScale = Vector3.one;
        private bool initialized;

        public ArrowView OwnerArrowView => ownerArrowView;

        public void Initialize(
            ArrowView owner,
            Int2 cell,
            bool isHead,
            Direction headDirection,
            Color color,
            Vector3 worldPosition,
            float cellSize)
        {
            ownerArrowView = owner;
            transform.position = worldPosition;

            spriteRenderer = GetComponent<SpriteRenderer>();
            boxCollider = GetComponent<BoxCollider2D>();

            spriteRenderer.sprite = GetWhiteSprite();
            spriteRenderer.color = isHead ? Brighten(color, 0.16f) : color;
            spriteRenderer.sortingOrder = isHead ? 2 : 1;

            float size = isHead ? cellSize * 0.9f : cellSize * 0.82f;
            baseScale = new Vector3(size, size, 1f);
            transform.localScale = baseScale;
            boxCollider.size = Vector2.one;
            boxCollider.isTrigger = true;

            if (isHead)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, DirectionToZRotation(headDirection));
            }

            initialized = true;
        }

        public void SetAlpha(float alpha)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }

        public void SetScaleMultiplier(float multiplier)
        {
            if (!initialized)
            {
                return;
            }

            transform.localScale = baseScale * multiplier;
        }

        public void SetOutlineActive(bool active)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            spriteRenderer.material = null;
            spriteRenderer.drawMode = SpriteDrawMode.Simple;

            Color color = spriteRenderer.color;
            color.a = color.a <= 0f ? 1f : color.a;
            spriteRenderer.color = color;

            if (active)
            {
                transform.localScale = baseScale * 1.04f;
            }
        }

        private static float DirectionToZRotation(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return 0f;
                case Direction.Right:
                    return -90f;
                case Direction.Down:
                    return 180f;
                case Direction.Left:
                    return 90f;
                default:
                    return 0f;
            }
        }

        private static Color Brighten(Color color, float amount)
        {
            return new Color(
                Mathf.Clamp01(color.r + amount),
                Mathf.Clamp01(color.g + amount),
                Mathf.Clamp01(color.b + amount),
                color.a);
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
            cachedWhiteSprite.name = "RuntimeWhiteSprite";
            return cachedWhiteSprite;
        }
    }
}
