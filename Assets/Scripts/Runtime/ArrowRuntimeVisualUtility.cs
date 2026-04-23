using UnityEngine;

namespace ArrowPuzzle.Runtime
{
    public static class ArrowRuntimeVisualUtility
    {
        private static Sprite cachedWhiteSprite;

        public static Sprite GetWhiteSprite()
        {
            if (cachedWhiteSprite != null)
            {
                return cachedWhiteSprite;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.filterMode = FilterMode.Point;

            cachedWhiteSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);
            cachedWhiteSprite.name = "RuntimeWhiteSprite";
            return cachedWhiteSprite;
        }
    }
}
