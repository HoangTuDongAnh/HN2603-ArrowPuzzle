using System.Collections;
using UnityEngine;

namespace ArrowPuzzle.Runtime
{
    public class FeedbackController : MonoBehaviour
    {
        [Header("Runtime Feedback")]
        [SerializeField] private float markerDuration = 0.2f;
        [SerializeField] private float markerScale = 0.28f;

        public void PlayRemoveFeedback(Vector3 worldPosition)
        {
            StartCoroutine(SpawnMarkerRoutine("RemoveFx", worldPosition, new Color(0.35f, 1f, 0.45f, 0.9f)));
        }

        public void PlayBlockedFeedback(Vector3 worldPosition)
        {
            StartCoroutine(SpawnMarkerRoutine("BlockedFx", worldPosition, new Color(1f, 0.35f, 0.35f, 0.95f)));
        }

        public void PlayWinFeedback()
        {
            Debug.Log("WIN: All arrows removed.");
        }

        private IEnumerator SpawnMarkerRoutine(string markerName, Vector3 worldPosition, Color color)
        {
            GameObject marker = new GameObject(markerName);
            marker.transform.position = worldPosition;

            SpriteRenderer spriteRenderer = marker.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = ArrowRuntimeVisualUtility.GetWhiteSprite();
            spriteRenderer.color = color;
            marker.transform.localScale = Vector3.one * markerScale;

            float elapsed = 0f;
            while (elapsed < markerDuration)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / markerDuration);

                Color currentColor = spriteRenderer.color;
                currentColor.a = Mathf.Lerp(color.a, 0f, normalized);
                spriteRenderer.color = currentColor;

                marker.transform.localScale = Vector3.one * Mathf.Lerp(markerScale, markerScale * 1.8f, normalized);
                yield return null;
            }

            Destroy(marker);
        }
    }
}
