using UnityEngine;

namespace KeepBallMoving
{
    public static class KeepBallBootstrapper
    {
        private const string DefaultGameManagerPrefabPath = "KeepBallMoving/KeepBallGameManager";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreatePrototypeIfNeeded()
        {
            if (Object.FindObjectOfType<KeepBallGameManager>() != null)
            {
                return;
            }

            KeepBallGameManager prefab = Resources.Load<KeepBallGameManager>(DefaultGameManagerPrefabPath);
            if (prefab != null)
            {
                Object.Instantiate(prefab);
                return;
            }

            GameObject gameObject = new GameObject("KeepBallGameManager");
            gameObject.AddComponent<KeepBallGameManager>();
        }
    }
}
