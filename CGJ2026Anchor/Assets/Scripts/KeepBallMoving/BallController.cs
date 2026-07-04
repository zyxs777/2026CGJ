using UnityEngine;
using RGScript.ScriptsSkinSpecialDeals;

namespace KeepBallMoving
{
    public sealed class BallController : MonoBehaviour
    {
        [SerializeField] private float maxSpeed = 22f;
        [SerializeField] private float minSpeed = 5f;
        [SerializeField] private float releaseKickSpeedMultiplier = 1.35f;
        [SerializeField] private float freeBallDeceleration = 7f;
        [SerializeField] private float baseFastRotateSpeed = 360f;

        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private RotateAroundAxisBehaviour[] rotateBehaviours;
        private PlayerAgent holder;
        private float radius;
        private float holdRadius;
        private float holdAngularSpeed;
        private float holdAngle;
        private int phantomBounceCount;
        private int phantomMaxBounces = 3;
        private float curveRemainingTime;
        private float curveStrength;
        private float curveSign = 1f;
        private Vector2 lastFreeDirection = Vector2.right;

        public BallState State { get; private set; } = BallState.Free;
        public Vector2 Position => transform.position;
        public Vector2 Velocity => body != null ? body.velocity : Vector2.zero;
        public PlayerAgent Holder => holder;
        public float Radius => radius;
        public bool IsPhantom { get; private set; }
        public Team PhantomOwner { get; private set; }

        public void Initialize(Sprite sprite, PhysicsMaterial2D physicsMaterial, float radius, float maxBallSpeed)
        {
            this.radius = radius;
            maxSpeed = maxBallSpeed;

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

            spriteRenderer.color = Color.white;
            spriteRenderer.sortingOrder = 30;
            rotateBehaviours = GetComponentsInChildren<RotateAroundAxisBehaviour>(true);

            transform.localScale = Vector3.one * (radius * 2f);

            body = GetComponent<Rigidbody2D>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody2D>();
            }

            body.gravityScale = 0f;
            body.drag = 0f;
            body.angularDrag = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
            if (circleCollider == null)
            {
                circleCollider = gameObject.AddComponent<CircleCollider2D>();
            }

            circleCollider.radius = 0.5f;
            circleCollider.sharedMaterial = physicsMaterial;
        }

        private void FixedUpdate()
        {
            if (State != BallState.Free || body == null)
            {
                UpdateFastRotateSpeed(0f);
                return;
            }

            Vector2 velocity = body.velocity;
            float speed = velocity.magnitude;

            if (speed > 0.01f)
            {
                lastFreeDirection = velocity / speed;
            }

            if (speed > maxSpeed)
            {
                speed = maxSpeed;
            }
            else if (speed > minSpeed)
            {
                speed = Mathf.MoveTowards(speed, minSpeed, freeBallDeceleration * Time.fixedDeltaTime);
            }
            else if (speed < minSpeed)
            {
                speed = minSpeed;
            }

            body.velocity = lastFreeDirection * speed;
            UpdateFastRotateSpeed(speed);
            TickCurve(Time.fixedDeltaTime);
        }

        public void ResetBall(Vector2 position, Vector2 velocity)
        {
            State = BallState.Free;
            ClearHolderAnimation();
            holder = null;
            IsPhantom = false;
            phantomBounceCount = 0;
            curveRemainingTime = 0f;
            transform.position = position;

            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }

            body.bodyType = RigidbodyType2D.Dynamic;
            body.velocity = velocity;
            body.angularVelocity = 0f;

            if (velocity.sqrMagnitude > 0.001f)
            {
                lastFreeDirection = velocity.normalized;
            }

            UpdateFastRotateSpeed(velocity.magnitude);
        }

        public void BeginHold(PlayerAgent newHolder, float minHoldRadius, float angularSpeed)
        {
            if (holder != null && holder != newHolder)
            {
                holder.SetHoldingAnimation(false);
            }

            holder = newHolder;
            holder.SetHoldingAnimation(true);
            Vector2 offset = Position - holder.Position;
            if (offset.sqrMagnitude < 0.001f)
            {
                offset = holder.Team == Team.Red ? Vector2.left : Vector2.right;
            }

            holdRadius = Mathf.Max(offset.magnitude, minHoldRadius);
            holdAngularSpeed = angularSpeed;
            holdAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            State = BallState.Held;

            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            UpdateFastRotateSpeed(0f);

            Vector2 heldPosition = GetHeldPosition();
            body.position = heldPosition;
            transform.position = heldPosition;
        }

        public void TickHold(float deltaTime)
        {
            if (State != BallState.Held || holder == null)
            {
                return;
            }

            holdAngle += holdAngularSpeed * deltaTime;
            body.MovePosition(GetHeldPosition());
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
            UpdateFastRotateSpeed(0f);
        }

        private Vector2 GetHeldPosition()
        {
            float radians = holdAngle * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * holdRadius;
            return holder.Position + offset;
        }

        public Vector2 Release(float speed)
        {
            if (State != BallState.Held || holder == null)
            {
                return Vector2.zero;
            }

            Vector2 direction = (Position - holder.Position).normalized;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector2.right;
            }

            ReleaseInDirection(direction, speed);
            return direction;
        }

        public Vector2 ReleaseToward(Vector2 targetPosition, float speed)
        {
            if (State != BallState.Held || holder == null)
            {
                return Vector2.zero;
            }

            Vector2 direction = (targetPosition - Position).normalized;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = holder.Team == Team.Red ? Vector2.left : Vector2.right;
            }

            ReleaseInDirection(direction, speed);
            return direction;
        }

        public Vector2 ReleaseFromHolderPosition(float speed)
        {
            if (State != BallState.Held || holder == null)
            {
                return Vector2.zero;
            }

            Vector2 holderPosition = holder.Position;
            Vector2 direction = (Position - holderPosition).normalized;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = holder.Team == Team.Red ? Vector2.left : Vector2.right;
            }

            ReleaseInDirectionAt(holderPosition, direction, speed);
            return direction;
        }

        public void ConfigurePhantom(Team owner, int maxBounces, Color color)
        {
            IsPhantom = true;
            PhantomOwner = owner;
            phantomBounceCount = 0;
            phantomMaxBounces = Mathf.Max(1, maxBounces);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
                spriteRenderer.sortingOrder = 29;
            }
        }

        public void LaunchPhantom(Vector2 position, Vector2 direction, float speed)
        {
            State = BallState.Free;
            ClearHolderAnimation();
            holder = null;
            transform.position = position;

            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }

            body.bodyType = RigidbodyType2D.Dynamic;
            body.velocity = direction.normalized * GetKickedSpeed(speed);
            body.angularVelocity = 0f;
            lastFreeDirection = direction.normalized;
            UpdateFastRotateSpeed(body.velocity.magnitude);
        }

        public void FreezeForGoalPresentation(Vector2 position)
        {
            State = BallState.Free;
            ClearHolderAnimation();
            holder = null;
            curveRemainingTime = 0f;
            transform.position = position;

            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }

            body.bodyType = RigidbodyType2D.Kinematic;
            body.position = position;
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
            UpdateFastRotateSpeed(0f);
        }

        public void ApplyCurve(float strength, float duration, float sign)
        {
            curveStrength = strength;
            curveRemainingTime = Mathf.Max(0f, duration);
            curveSign = Mathf.Sign(sign);
            if (Mathf.Abs(curveSign) < 0.001f)
            {
                curveSign = 1f;
            }
        }

        private void ReleaseInDirection(Vector2 direction, float speed)
        {
            ReleaseInDirectionAt(Position, direction, speed);
        }

        private void ReleaseInDirectionAt(Vector2 position, Vector2 direction, float speed)
        {
            State = BallState.Free;
            ClearHolderAnimation();
            holder = null;
            body.position = position;
            transform.position = position;
            body.bodyType = RigidbodyType2D.Dynamic;
            lastFreeDirection = direction;
            body.velocity = direction * GetKickedSpeed(speed);
            body.angularVelocity = 0f;
            UpdateFastRotateSpeed(body.velocity.magnitude);
        }

        private float GetKickedSpeed(float requestedSpeed)
        {
            return Mathf.Clamp(requestedSpeed * Mathf.Max(0.01f, releaseKickSpeedMultiplier), minSpeed, maxSpeed);
        }

        private void UpdateFastRotateSpeed(float speed)
        {
            if (rotateBehaviours == null || rotateBehaviours.Length == 0)
            {
                return;
            }

            float speedRatio = Mathf.Max(1f, speed / Mathf.Max(0.001f, minSpeed));
            float rotateSpeed = baseFastRotateSpeed * speedRatio;
            for (int i = 0; i < rotateBehaviours.Length; i++)
            {
                if (rotateBehaviours[i] != null)
                {
                    rotateBehaviours[i].fastRotateSpeed = rotateSpeed;
                }
            }
        }

        private void ClearHolderAnimation()
        {
            if (holder != null)
            {
                holder.SetHoldingAnimation(false);
            }
        }

        private void TickCurve(float deltaTime)
        {
            if (curveRemainingTime <= 0f || body == null)
            {
                return;
            }

            Vector2 velocity = body.velocity;
            if (velocity.sqrMagnitude < 0.01f)
            {
                curveRemainingTime = 0f;
                return;
            }

            Vector2 perpendicular = new Vector2(-velocity.y, velocity.x).normalized * curveSign;
            body.AddForce(perpendicular * curveStrength, ForceMode2D.Force);
            curveRemainingTime -= deltaTime;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsPhantom)
            {
                return;
            }

            if (collision.collider.GetComponent<BallController>() != null)
            {
                return;
            }

            phantomBounceCount++;
            if (phantomBounceCount >= phantomMaxBounces)
            {
                Destroy(gameObject);
            }
        }
    }
}
