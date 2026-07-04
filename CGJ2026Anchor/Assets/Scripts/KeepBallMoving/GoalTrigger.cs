using UnityEngine;

namespace KeepBallMoving
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class GoalTrigger : MonoBehaviour
    {
        [SerializeField] private GoalSide side;

        private KeepBallGameManager gameManager;
        private BoxCollider2D boxCollider;

        public void Initialize(KeepBallGameManager manager, GoalSide goalSide, Vector2 size)
        {
            gameManager = manager;
            side = goalSide;

            boxCollider = GetComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            boxCollider.size = Vector2.one;
        }

        public void SetTriggerEnabled(bool enabled)
        {
            if (boxCollider == null)
            {
                boxCollider = GetComponent<BoxCollider2D>();
            }

            if (boxCollider != null)
            {
                boxCollider.enabled = enabled;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            BallController ball = other.GetComponent<BallController>();
            if (ball != null && gameManager != null)
            {
                gameManager.OnGoal(side, ball);
            }
        }
    }
}
