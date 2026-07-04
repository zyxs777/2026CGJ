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
        [Header("Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private GameObject tiredEffectPrefab;

        private SpriteRenderer spriteRenderer;
        private CircleCollider2D bodyCollider;
        private SpriteRenderer controlRangeRenderer;
        private Transform controlRangeTransform;
        private Vector3 controlRangeBaseLocalScale = Vector3.one;
        private Transform holdPointPivot;
        private Transform holdPointTransform;
        private SpriteRenderer holdPointRenderer;
        private Transform targetIndicator;
        private GameObject tiredEffectInstance;
        private Animator animator;
        private Color baseColor;
        private Color highlightColor;
        private Vector2 spawnPosition;
        private Vector2 lastAnimationPosition;
        private float holdPointRadius;
        private float holdPointAngle;
        private float controlRangeMultiplier = 1f;
        private float bodyRadiusMultiplier = 1f;
        private Coroutine knockbackRoutine;
        private bool animatorHasRun;
        private bool animatorHasHold;
        private bool isHoldingAnimation;
        private bool hasFacingOverride;
        private bool staminaExhausted;
        private Vector2 facingOverrideTarget;
        private float currentStamina;
        private int lastStaminaRecoveryFrame = -1;

        private static readonly int RunAnimatorHash = Animator.StringToHash("run");
        private static readonly int HoldAnimatorHash = Animator.StringToHash("hold");
        private const string DefaultTiredEffectPrefabPath = "KeepBallMoving/TiredEffect";
        private const float RunAnimationMoveThreshold = 0.0025f;
        private const float FacingMoveThreshold = 0.001f;
        private const int NormalSortingOrder = 30;
        private const int HoldingSortingOrder = 100;

        public Team Team => team;
        public PlayerRole Role => role;
        public float CatchRadius => catchRadius;
        public float BodyRadius => GetEffectiveBodyRadius();
        public Vector2 Position => transform.position;
        public Vector2 HoldPointPosition => holdPointTransform != null ? holdPointTransform.position : transform.position;
        public bool IsKnockbackActive => knockbackRoutine != null;
        public bool IsStaminaExhausted => staminaExhausted;
        public bool HasUsableStamina => !staminaExhausted && currentStamina > 0f;
        public float Stamina01 => Mathf.Clamp01(currentStamina / Mathf.Max(0.001f, maxStamina));

        public void Initialize(Team playerTeam, PlayerRole playerRole, int index, Vector2 startPosition, Sprite sprite, Sprite controlRangeSprite, GameObject controlRangePrefab, Sprite holdPointSprite, float radius, float catchRange, float pointRadius)
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

            CreateControlRangeVisual(controlRangeSprite, controlRangePrefab, baseColor);
            CreateHoldPointVisual(holdPointSprite, baseColor);
            CacheTargetIndicator();
            CacheAnimator();
            ResetStaminaFull();
            SetHoldingAnimation(false);
        }

        public void SetControlRangeMultiplier(float multiplier)
        {
            controlRangeMultiplier = Mathf.Max(0.01f, multiplier);
            UpdateControlRangeScale();
            UpdateHoldPointLayout();
        }

        public void SetBodyRadiusMultiplier(float multiplier)
        {
            bodyRadiusMultiplier = Mathf.Max(0.01f, multiplier);
            UpdateBodyScale();
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
            return Vector2.Distance(Position, ball.Position) <= catchRadius * controlRangeMultiplier + ball.Radius;
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

        public void TickStamina(float deltaTime, float drainPerSecond, float recoverPerSecond)
        {
            if (maxStamina <= 0f)
            {
                currentStamina = 0f;
                staminaExhausted = false;
                SetTiredEffectVisible(false);
                return;
            }

            if (staminaExhausted)
            {
                RecoverStamina(deltaTime, recoverPerSecond);
                return;
            }

            float drain = Mathf.Max(0f, drainPerSecond) * Mathf.Max(0f, deltaTime);
            if (drain > 0f)
            {
                ConsumeStamina(drain);
                return;
            }

            RecoverStamina(deltaTime, recoverPerSecond);
        }

        public void ConsumeStamina(float amount)
        {
            if (maxStamina <= 0f || staminaExhausted)
            {
                return;
            }

            currentStamina = Mathf.Max(0f, currentStamina - Mathf.Max(0f, amount));
            if (currentStamina <= 0f)
            {
                SetStaminaExhausted(true);
            }
        }

        public void ResetStaminaFull()
        {
            currentStamina = Mathf.Max(0f, maxStamina);
            SetStaminaExhausted(false);
            lastStaminaRecoveryFrame = -1;
        }

        public void SetHoldingAnimation(bool holding)
        {
            isHoldingAnimation = holding;
            if (holding)
            {
                SetFacingOverride(Vector2.zero, false);
            }

            SetCatchHighlighted(holding);
            SetTargetIndicatorVisible(holding);
            SetAnimatorBool(HoldAnimatorHash, animatorHasHold, holding);
            if (holding)
            {
                SetAnimatorBool(RunAnimatorHash, animatorHasRun, false);
            }
        }

        public void SetFacingOverride(Vector2 targetPosition, bool enabled)
        {
            hasFacingOverride = enabled;
            facingOverrideTarget = targetPosition;
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
            UpdateBodyScale();
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
            ResetStaminaFull();
            SetFacingOverride(Vector2.zero, false);
            SetHoldingAnimation(false);
            SetCatchHighlighted(false);
        }

        private void RecoverStamina(float deltaTime, float recoverPerSecond)
        {
            if (lastStaminaRecoveryFrame == Time.frameCount)
            {
                return;
            }

            lastStaminaRecoveryFrame = Time.frameCount;
            currentStamina = Mathf.Min(Mathf.Max(0f, maxStamina), currentStamina + Mathf.Max(0f, recoverPerSecond) * Mathf.Max(0f, deltaTime));
            if (staminaExhausted && currentStamina >= Mathf.Max(0f, maxStamina))
            {
                SetStaminaExhausted(false);
            }
        }

        private void SetStaminaExhausted(bool exhausted)
        {
            if (staminaExhausted == exhausted)
            {
                return;
            }

            staminaExhausted = exhausted;
            SetTiredEffectVisible(exhausted);
        }

        private void SetTiredEffectVisible(bool visible)
        {
            if (visible)
            {
                EnsureTiredEffect();
            }

            if (tiredEffectInstance == null)
            {
                return;
            }

            if (tiredEffectInstance.activeSelf != visible)
            {
                tiredEffectInstance.SetActive(visible);
            }

            ParticleSystem[] particleSystems = tiredEffectInstance.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                if (visible)
                {
                    ParticleSystem.MainModule main = particleSystems[i].main;
                    main.loop = true;
                    particleSystems[i].Play(true);
                }
                else
                {
                    particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }

        private void EnsureTiredEffect()
        {
            if (tiredEffectInstance != null)
            {
                return;
            }

            if (tiredEffectPrefab == null)
            {
                tiredEffectPrefab = Resources.Load<GameObject>(DefaultTiredEffectPrefabPath);
            }

            if (tiredEffectPrefab == null)
            {
                return;
            }

            tiredEffectInstance = Instantiate(tiredEffectPrefab, transform);
            tiredEffectInstance.name = "TiredEffect";
            tiredEffectInstance.transform.localPosition = Vector3.zero;
            tiredEffectInstance.transform.localRotation = Quaternion.identity;
            tiredEffectInstance.transform.localScale = Vector3.one;
        }

        private void LateUpdate()
        {
            CacheAnimator();

            Vector2 currentPosition = transform.position;
            Vector2 movement = currentPosition - lastAnimationPosition;
            UpdateSpriteFacing(hasFacingOverride ? facingOverrideTarget - currentPosition : movement);

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

        private void CacheTargetIndicator()
        {
            targetIndicator = FindChildRecursive(transform, "target");
            SetTargetIndicatorVisible(false);
        }

        private Transform FindChildRecursive(Transform parent, string childName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (string.Equals(child.name, childName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return child;
                }

                Transform match = FindChildRecursive(child, childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private void SetTargetIndicatorVisible(bool visible)
        {
            if (targetIndicator != null)
            {
                targetIndicator.gameObject.SetActive(visible);
            }
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

        private void CreateControlRangeVisual(Sprite controlRangeSprite, GameObject controlRangePrefab, Color color)
        {
            GameObject rangeObject;
            if (controlRangePrefab != null)
            {
                rangeObject = Instantiate(controlRangePrefab, transform);
                rangeObject.name = "ControlRange";
                rangeObject.transform.localPosition = Vector3.zero;
                rangeObject.transform.localRotation = Quaternion.identity;
            }
            else
            {
                rangeObject = new GameObject("ControlRange");
                rangeObject.transform.SetParent(transform);
                rangeObject.transform.localPosition = Vector3.zero;
                rangeObject.transform.localRotation = Quaternion.identity;

                controlRangeRenderer = rangeObject.AddComponent<SpriteRenderer>();
                controlRangeRenderer.sprite = controlRangeSprite;
                controlRangeRenderer.color = new Color(color.r, color.g, color.b, 0.18f);
                controlRangeRenderer.sortingOrder = 10;
            }

            controlRangeTransform = rangeObject.transform;
            controlRangeBaseLocalScale = controlRangeTransform.localScale;
            if (controlRangeRenderer == null)
            {
                controlRangeRenderer = rangeObject.GetComponentInChildren<SpriteRenderer>();
            }

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
            if (controlRangeTransform == null)
            {
                return;
            }

            float desiredDiameter = catchRadius * controlRangeMultiplier * 2f;
            controlRangeTransform.localScale = controlRangeBaseLocalScale;
            float baseDiameter = GetControlRangeWorldDiameter();
            float scale = desiredDiameter / Mathf.Max(0.001f, baseDiameter);
            controlRangeTransform.localScale = controlRangeBaseLocalScale * scale;
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

        private void UpdateBodyScale()
        {
            float highlightMultiplier = isHoldingAnimation ? 1.12f : 1f;
            transform.localScale = Vector3.one * (GetEffectiveBodyRadius() * 2f * highlightMultiplier);
        }

        private float GetEffectiveBodyRadius()
        {
            return bodyRadius * bodyRadiusMultiplier;
        }

        private float GetControlRangeWorldDiameter()
        {
            SpriteRenderer[] renderers = controlRangeTransform.GetComponentsInChildren<SpriteRenderer>();
            if (renderers.Length == 0)
            {
                return Mathf.Max(0.001f, transform.lossyScale.x);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return Mathf.Max(bounds.size.x, bounds.size.y);
        }
    }
}
