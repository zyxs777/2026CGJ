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
        [Header("Team Visuals")]
        [SerializeField] private Color blueColor = new Color(0.05f, 0.33f, 1f, 1f);
        [SerializeField] private Color redColor = new Color(0.95f, 0.12f, 0.11f, 1f);
        [SerializeField] private Color blueHighlightColor = new Color(0.52f, 0.78f, 1f, 1f);
        [SerializeField] private Color redHighlightColor = new Color(1f, 0.48f, 0.42f, 1f);

        private SpriteRenderer spriteRenderer;
        private CircleCollider2D bodyCollider;
        private SpriteRenderer controlRangeRenderer;
        private Transform holdPointPivot;
        private Transform holdPointTransform;
        private SpriteRenderer holdPointRenderer;
        private Animator animator;
        private Color baseColor;
        private Color highlightColor;
        private Vector2 spawnPosition;
        private Vector2 lastAnimationPosition;
        private float holdPointRadius;
        private float holdPointAngle;
        private float controlRangeMultiplier = 1f;
        private Coroutine knockbackRoutine;
        private bool animatorHasRun;
        private bool animatorHasHold;
        private bool isHoldingAnimation;

        private static readonly int RunAnimatorHash = Animator.StringToHash("run");
        private static readonly int HoldAnimatorHash = Animator.StringToHash("hold");
        private const float RunAnimationMoveThreshold = 0.0025f;
        private const float FacingMoveThreshold = 0.001f;
        private const int NormalSortingOrder = 30;
        private const int HoldingSortingOrder = 100;

        public Team Team => team;
        public PlayerRole Role => role;
        public float CatchRadius => catchRadius;
        public float BodyRadius => bodyRadius;
        public Vector2 Position => transform.position;
        public Vector2 HoldPointPosition => holdPointTransform != null ? holdPointTransform.position : transform.position;
        public bool IsKnockbackActive => knockbackRoutine != null;

        public void Initialize(Team playerTeam, PlayerRole playerRole, int index, Vector2 startPosition, Sprite sprite, Sprite controlRangeSprite, Sprite holdPointSprite, float radius, float catchRange, float pointRadius)
        {
            team = playerTeam;
            role = playerRole;
            bodyRadius = radius;
            catchRadius = catchRange;
            holdPointRadius = pointRadius;
            spawnPosition = startPosition;
            baseColor = GetConfiguredTeamColor(playerTeam);
            highlightColor = GetConfiguredHighlightColor(playerTeam);
            holdPointAngle = Random.Range(0f, 360f);

            gameObject.name = $"{team}{role}_{index:00}";
            transform.position = startPosition;
            transform.localScale = Vector3.one * (bodyRadius * 2f);
            lastAnimationPosition = startPosition;

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
            spriteRenderer.flipX = false;
            spriteRenderer.sortingOrder = NormalSortingOrder;

            bodyCollider = GetComponent<CircleCollider2D>();
            if (bodyCollider == null)
            {
                bodyCollider = gameObject.AddComponent<CircleCollider2D>();
            }

            bodyCollider.isTrigger = true;
            bodyCollider.radius = 0.5f;

            CreateControlRangeVisual(controlRangeSprite, baseColor);
            CreateHoldPointVisual(holdPointSprite, baseColor);
            CacheAnimator();
            SetHoldingAnimation(false);
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

        public void SetHoldingAnimation(bool holding)
        {
            isHoldingAnimation = holding;
            SetCatchHighlighted(holding);
            SetAnimatorBool(HoldAnimatorHash, animatorHasHold, holding);
            if (holding)
            {
                SetAnimatorBool(RunAnimatorHash, animatorHasRun, false);
            }
        }

        public void OverrideVisualColors(Color color, Color catchHighlightColor)
        {
            baseColor = color;
            highlightColor = catchHighlightColor;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = baseColor;
            }

            if (controlRangeRenderer != null)
            {
                controlRangeRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.18f);
            }

            if (holdPointRenderer != null)
            {
                holdPointRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
            }
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

            spriteRenderer.color = highlighted ? highlightColor : baseColor;
            spriteRenderer.sortingOrder = highlighted ? HoldingSortingOrder : NormalSortingOrder;
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
            ResetToPosition(spawnPosition);
        }

        public void ResetToPosition(Vector2 position)
        {
            if (knockbackRoutine != null)
            {
                StopCoroutine(knockbackRoutine);
                knockbackRoutine = null;
            }

            spawnPosition = position;
            transform.position = spawnPosition;
            lastAnimationPosition = spawnPosition;
            SetHoldingAnimation(false);
            SetCatchHighlighted(false);
        }

        private void LateUpdate()
        {
            CacheAnimator();

            Vector2 currentPosition = transform.position;
            Vector2 movement = currentPosition - lastAnimationPosition;
            UpdateSpriteFacing(movement);

            bool isRunning = !isHoldingAnimation && movement.sqrMagnitude > RunAnimationMoveThreshold * RunAnimationMoveThreshold;
            SetAnimatorBool(RunAnimatorHash, animatorHasRun, isRunning);
            SetAnimatorBool(HoldAnimatorHash, animatorHasHold, isHoldingAnimation);
            lastAnimationPosition = currentPosition;
        }

        private void UpdateSpriteFacing(Vector2 movement)
        {
            if (spriteRenderer == null || Mathf.Abs(movement.x) <= FacingMoveThreshold)
            {
                return;
            }

            spriteRenderer.flipX = movement.x < 0f;
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

        private void CacheAnimator()
        {
            if (animator != null)
            {
                return;
            }

            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (animator == null)
            {
                return;
            }

            animatorHasRun = HasAnimatorParameter("run", AnimatorControllerParameterType.Bool);
            animatorHasHold = HasAnimatorParameter("hold", AnimatorControllerParameterType.Bool);
        }

        private Color GetConfiguredTeamColor(Team playerTeam)
        {
            return playerTeam == Team.Blue ? blueColor : redColor;
        }

        private Color GetConfiguredHighlightColor(Team playerTeam)
        {
            return playerTeam == Team.Blue ? blueHighlightColor : redHighlightColor;
        }

        private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
        {
            if (animator == null)
            {
                return false;
            }

            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter.type == parameterType && parameter.name == parameterName)
                {
                    return true;
                }
            }

            return false;
        }

        private void SetAnimatorBool(int parameterHash, bool hasParameter, bool value)
        {
            if (animator == null || !hasParameter)
            {
                return;
            }

            animator.SetBool(parameterHash, value);
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
