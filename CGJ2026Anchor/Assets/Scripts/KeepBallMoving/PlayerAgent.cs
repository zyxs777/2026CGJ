using UnityEngine;

namespace KeepBallMoving
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class PlayerAgent : MonoBehaviour
    {
        [SerializeField] private Team team;
        [SerializeField] private float catchRadius = 1.8f;
        [SerializeField] private float bodyRadius = 0.5f;

        private SpriteRenderer spriteRenderer;
        private CircleCollider2D bodyCollider;
        private SpriteRenderer controlRangeRenderer;
        private Color baseColor;
        private Vector2 spawnPosition;

        public Team Team => team;
        public float CatchRadius => catchRadius;
        public float BodyRadius => bodyRadius;
        public Vector2 Position => transform.position;

        public void Initialize(Team playerTeam, int index, Vector2 startPosition, Sprite sprite, Sprite controlRangeSprite, Color color, float radius, float catchRange)
        {
            team = playerTeam;
            bodyRadius = radius;
            catchRadius = catchRange;
            spawnPosition = startPosition;
            baseColor = color;

            gameObject.name = $"{team}Player_{index:00}";
            transform.position = startPosition;
            transform.localScale = Vector3.one * (bodyRadius * 2f);

            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = baseColor;
            spriteRenderer.sortingOrder = 20;

            bodyCollider = GetComponent<CircleCollider2D>();
            bodyCollider.isTrigger = true;
            bodyCollider.radius = 0.5f;

            CreateControlRangeVisual(controlRangeSprite, color);
        }

        public bool CanCatch(BallController ball)
        {
            return team == Team.Blue && Vector2.Distance(Position, ball.Position) <= catchRadius;
        }

        public bool CanControlBall(BallController ball)
        {
            return Vector2.Distance(Position, ball.Position) <= catchRadius;
        }

        public void MoveTo(Vector2 position)
        {
            transform.position = position;
        }

        public void SetCatchHighlighted(bool highlighted)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.color = highlighted ? Color.Lerp(baseColor, Color.white, 0.35f) : baseColor;
            transform.localScale = Vector3.one * (bodyRadius * 2f * (highlighted ? 1.12f : 1f));
            UpdateControlRangeScale();
        }

        public void ResetToSpawn()
        {
            transform.position = spawnPosition;
            SetCatchHighlighted(false);
        }

        private void CreateControlRangeVisual(Sprite controlRangeSprite, Color color)
        {
            GameObject rangeObject = new GameObject("ControlRange");
            rangeObject.transform.SetParent(transform);
            rangeObject.transform.localPosition = Vector3.zero;
            rangeObject.transform.localRotation = Quaternion.identity;

            controlRangeRenderer = rangeObject.AddComponent<SpriteRenderer>();
            controlRangeRenderer.sprite = controlRangeSprite;
            controlRangeRenderer.color = new Color(color.r, color.g, color.b, 0.18f);
            controlRangeRenderer.sortingOrder = 10;

            UpdateControlRangeScale();
        }

        private void UpdateControlRangeScale()
        {
            if (controlRangeRenderer == null)
            {
                return;
            }

            float parentScale = Mathf.Max(0.001f, transform.localScale.x);
            float localDiameter = catchRadius * 2f / parentScale;
            controlRangeRenderer.transform.localScale = Vector3.one * localDiameter;
        }
    }
}
