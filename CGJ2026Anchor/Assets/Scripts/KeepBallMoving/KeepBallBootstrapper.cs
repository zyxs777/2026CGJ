using UnityEngine;

namespace KeepBallMoving
{
    public static class KeepBallBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreatePrototypeIfNeeded()
        {
            if (Object.FindObjectOfType<KeepBallGameManager>() != null)
            {
                return;
            }

            GameObject gameObject = new GameObject("KeepBallGameManager");
            gameObject.AddComponent<KeepBallGameManager>();
        }
    }
}
