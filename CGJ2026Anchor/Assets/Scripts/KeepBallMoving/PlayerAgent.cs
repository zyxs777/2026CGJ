using System.Collections;
using UnityEngine;

namespace KeepBallMoving
{
    public sealed class PlayerAgent : MonoBehaviour
    {
        [SerializeField] private Team team;
        [SerializeField] private PlayerRole role;
        [SerializeField] private float catchRadius = 1.8f;
        [SerializeField] private float bodyRadius = 0.5f;

        private SpriteRenderer spriteRenderer;
        private CircleCollider2D bodyCollider;
        private SpriteRenderer controlRangeRenderer;
        private Transform holdPointPivot;
        private Transform holdPointTransform;
        private SpriteRenderer holdPointRenderer;
        private Color baseColor;
        private Vector2 spawnPosition;
        private float holdPointRadius;
        private float holdPointAngle;
        private float controlRangeMultiplier = 1f;
        private Coroutine knockbackRoutine;

        public Team Team => team;
        public PlayerRole Role => role;
        public float CatchRadius => catchRadius;
        public float BodyRadius => bodyRadius;
        public Vector2 Position => transform.position;
        public Vector2 HoldPointPosition => holdPointTransform != null ? holdPointTransform.position : transform.position;
        public bool IsKnockbackActive => knockbackRoutine != null;

        public void Initialize(Team playerTeam, PlayerRole playerRole, int index, Vector2 startPosition, Sprite sprite, Sprite controlRangeSprite, Sprite holdPointSprite, Color color, float radius, float catchRange, float pointRadius)
        {
            team = playerTeam;
            role = playerRole;
            bodyRadius = radius;
            catchRadius = catchRange;
            holdPointRadius = pointRadius;
            spawnPosition = startPosition;
            baseColor = color;
            holdPointAngle = Random.Range(0f, 360f);

            gameObject.name = $"{team}{role}_{index:00}";
            transform.position = startPosition;
            transform.localScale = Vector3.one * (bodyRadius * 2f);

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

            spriteRenderer.color = baseColor;
            spriteRenderer.sortingOrder = 20;

            bodyCollider = GetComponent<CircleCollider2D>();
            if (bodyCollider == null)
            {
                bodyCollider = gameObject.AddComponent<CircleCollider2D>();
            }

            bodyCollider.isTrigger = true;
            bodyCollider.radius = 0.5f;

            CreateControlRangeVisual(controlRangeSprite, color);
            CreateHoldPointVisual(holdPointSprite, color);
        }

        public void SetControlRangeMultiplier(float multiplier)
        {
            controlRangeMultiplier = Mathf.Max(0.01f, multiplier);
            UpdateControlRangeScale();
            UpdateHoldPointLayout();
        }

        public bool CanCatch(BallController ball)
        {
            return team == Team.Blue && IsBallInControlRange(ball);
        }

        public bool CanControlBall(BallController ball)
        {
            return IsBallInControlRange(ball);
        }

        public bool IsBallInControlRange(BallController ball)
        {
            return Vector2.Distance(Position, ball.Position) <= catchRadius + ball.Radius;
        }

        public bool IsHoldPointTouchingBall(BallController ball)
        {
            float controlDistance = holdPointRadius + ball.Radius;
            return Vector2.Distance(HoldPointPosition, ball.Position) <= controlDistance;
        }

        public void AlignHoldPointToBall(Vector2 ballPosition)
        {
            Vector2 direction = ballPosition - Position;
            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            holdPointAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (holdPointPivot != null)
            {
                holdPointPivot.localRotation = Quaternion.Euler(0f, 0f, holdPointAngle);
            }
        }

        public void TickHoldPoint(float deltaTime, float angularSpeed)
        {
            holdPointAngle += angularSpeed * deltaTime;
            if (holdPointPivot != null)
            {
                holdPointPivot.localRotation = Quaternion.Euler(0f, 0f, holdPointAngle);
            }
        }

        public void MoveTo(Vector2 position)
        {
            transform.position = position;
        }

        public void KnockbackTo(Vector2 targetPosition, float duration)
        {
            if (knockbackRoutine != null)
            {
                StopCoroutine(knockbackRoutine);
            }

            knockbackRoutine = StartCoroutine(KnockbackRoutine(targetPosition, Mathf.Max(0.01f, duration)));
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
            UpdateHoldPointLayout();

            if (holdPointRenderer != null)
            {
                holdPointRenderer.enabled = false;
            }
        }

        public void ResetToSpawn()
        {
            if (knockbackRoutine != null)
            {
                StopCoroutine(knockbackRoutine);
                knockbackRoutine = null;
            }

            transform.position = spawnPosition;
            SetCatchHighlighted(false);
        }

        private IEnumerator KnockbackRoutine(Vector2 targetPosition, float duration)
        {
            Vector2 startPosition = Position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.position = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            transform.position = targetPosition;
            knockbackRoutine = null;
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

        private void CreateHoldPointVisual(Sprite holdPointSprite, Color color)
        {
            GameObject pivotObject = new GameObject("HoldPointPivot");
            pivotObject.transform.SetParent(transform);
            pivotObject.transform.localPosition = Vector3.zero;
            pivotObject.transform.localRotation = Quaternion.Euler(0f, 0f, holdPointAngle);
            holdPointPivot = pivotObject.transform;

            GameObject pointObject = new GameObject("HoldPoint");
            pointObject.transform.SetParent(holdPointPivot);
            pointObject.transform.localRotation = Quaternion.identity;
            holdPointTransform = pointObject.transform;

            holdPointRenderer = pointObject.AddComponent<SpriteRenderer>();
            holdPointRenderer.sprite = holdPointSprite;
            holdPointRenderer.color = new Color(color.r, color.g, color.b, 1f);
            holdPointRenderer.sortingOrder = 28;
            holdPointRenderer.enabled = false;

            UpdateHoldPointLayout();
        }

        private void UpdateControlRangeScale()
        {
            if (controlRangeRenderer == null)
            {
                return;
            }

            float parentScale = Mathf.Max(0.001f, transform.localScale.x);
            float localDiameter = catchRadius * controlRangeMultiplier * 2f / parentScale;
            controlRangeRenderer.transform.localScale = Vector3.one * localDiameter;
        }

        private void UpdateHoldPointLayout()
        {
            if (holdPointTransform == null)
            {
                return;
            }

            float parentScale = Mathf.Max(0.001f, transform.localScale.x);
            holdPointTransform.localPosition = new Vector3(catchRadius * controlRangeMultiplier / parentScale, 0f, 0f);
            holdPointTransform.localScale = Vector3.one * (holdPointRadius * 2f / parentScale);
        }
    }
}
