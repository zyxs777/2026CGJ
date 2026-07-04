using UnityEngine;

namespace KeepBallMoving
{
    public sealed class ShockwaveEffect : MonoBehaviour
    {
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float startScaleMultiplier = 0.35f;
        [SerializeField] private float endScaleMultiplier = 1f;

        private SpriteRenderer spriteRenderer;
        private Color startColor;
        private float elapsed;
        private float targetDiameter;

        public void Initialize(Sprite sprite, float radius, float effectDuration)
        {
            duration = Mathf.Max(0.01f, effectDuration);
            targetDiameter = Mathf.Max(0.01f, radius * 2f);
            elapsed = 0f;

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = sprite;
            }

            spriteRenderer.sortingOrder = 32;
            startColor = spriteRenderer.color;
            transform.localScale = Vector3.one * (targetDiameter * startScaleMultiplier);
        }

        private void Update()
        {
            if (duration <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float scale = Mathf.Lerp(startScaleMultiplier, endScaleMultiplier, t) * targetDiameter;
            transform.localScale = Vector3.one * scale;

            if (spriteRenderer != null)
            {
                Color color = startColor;
                color.a = Mathf.Lerp(startColor.a, 0f, t);
                spriteRenderer.color = color;
            }

            if (elapsed >= duration)
            {
                Destroy(gameObject);
            }
        }
    }
}
