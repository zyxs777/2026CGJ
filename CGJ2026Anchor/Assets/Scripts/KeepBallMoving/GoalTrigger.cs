using UnityEngine;

namespace KeepBallMoving
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class GoalTrigger : MonoBehaviour
    {
        [SerializeField] private GoalSide side;

        private KeepBallGameManager gameManager;

        public void Initialize(KeepBallGameManager manager, GoalSide goalSide, Vector2 size)
        {
            gameManager = manager;
            side = goalSide;

            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            boxCollider.size = Vector2.one;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            BallController ball = other.GetComponent<BallController>();
            if (ball != null)
            {
                gameManager.OnGoal(side, ball);
            }
        }
    }
}
