using UnityEngine;

namespace KeepBallMoving
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class BallController : MonoBehaviour
    {
        [SerializeField] private float maxSpeed = 22f;
        [SerializeField] private float minSpeed = 5f;

        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private PlayerAgent holder;
        private float radius;
        private float holdRadius;
        private float holdAngularSpeed;
        private float holdAngle;
        private Vector2 lastFreeDirection = Vector2.right;

        public BallState State { get; private set; } = BallState.Free;
        public Vector2 Position => transform.position;
        public Vector2 Velocity => body != null ? body.velocity : Vector2.zero;
        public PlayerAgent Holder => holder;
        public float Radius => radius;

        public void Initialize(Sprite sprite, PhysicsMaterial2D physicsMaterial, float radius, float maxBallSpeed)
        {
            this.radius = radius;
            maxSpeed = maxBallSpeed;

            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = Color.white;
            spriteRenderer.sortingOrder = 30;

            transform.localScale = Vector3.one * (radius * 2f);

            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.drag = 0.12f;
            body.angularDrag = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
            circleCollider.radius = 0.5f;
            circleCollider.sharedMaterial = physicsMaterial;
        }

        private void FixedUpdate()
        {
            if (State != BallState.Free || body == null)
            {
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
                body.velocity = lastFreeDirection * maxSpeed;
            }
            else if (speed < minSpeed)
            {
                body.velocity = lastFreeDirection * minSpeed;
            }
        }

        public void ResetBall(Vector2 position, Vector2 velocity)
        {
            State = BallState.Free;
            holder = null;
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
        }

        public void BeginHold(PlayerAgent newHolder, float minHoldRadius, float angularSpeed)
        {
            holder = newHolder;
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
        }

        private Vector2 GetHeldPosition()
        {
            float radians = holdAngle * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * holdRadius;
            return holder.Position + offset;
        }

        public void Release(float speed)
        {
            if (State != BallState.Held || holder == null)
            {
                return;
            }

            Vector2 direction = (Position - holder.Position).normalized;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector2.right;
            }

            ReleaseInDirection(direction, speed);
        }

        public void ReleaseToward(Vector2 targetPosition, float speed)
        {
            if (State != BallState.Held || holder == null)
            {
                return;
            }

            Vector2 direction = (targetPosition - Position).normalized;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = holder.Team == Team.Red ? Vector2.left : Vector2.right;
            }

            ReleaseInDirection(direction, speed);
        }

        private void ReleaseInDirection(Vector2 direction, float speed)
        {
            State = BallState.Free;
            holder = null;
            body.bodyType = RigidbodyType2D.Dynamic;
            lastFreeDirection = direction;
            body.velocity = direction * Mathf.Clamp(speed, minSpeed, maxSpeed);
            body.angularVelocity = 0f;
        }
    }
}
