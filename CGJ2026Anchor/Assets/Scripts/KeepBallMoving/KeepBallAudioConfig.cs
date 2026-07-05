using UnityEngine;

namespace KeepBallMoving
{
    [CreateAssetMenu(fileName = "KeepBallAudioConfig", menuName = "Keep Ball Moving/Audio Config")]
    public sealed class KeepBallAudioConfig : ScriptableObject
    {
        [Header("Clips")]
        [SerializeField] private AudioClip kickClip;
        [SerializeField] private AudioClip waveClip;
        [SerializeField] private AudioClip goalClip;
        [SerializeField] private AudioClip roundStartClip;
        [SerializeField] private AudioClip gameplayBgmClip;

        [Header("Resource Fallbacks")]
        [SerializeField] private string kickClipResourcePath = "KeepBallMoving/kick";
        [SerializeField] private string waveClipResourcePath = "KeepBallMoving/wave";
        [SerializeField] private string goalClipResourcePath = "KeepBallMoving/happy";
        [SerializeField] private string roundStartClipResourcePath = "KeepBallMoving/start";
        [SerializeField] private string gameplayBgmResourcePath = "KeepBallMoving/bgm_play";

        [Header("Volumes")]
        [SerializeField, Range(0f, 1f)] private float kickVolume = 0.85f;
        [SerializeField, Range(0f, 1f)] private float waveVolume = 0.85f;
        [SerializeField, Range(0f, 1f)] private float goalVolume = 0.9f;
        [SerializeField, Range(0f, 1f)] private float roundStartVolume = 0.8f;
        [SerializeField, Range(0f, 1f)] private float gameplayBgmVolume = 0.45f;

        public AudioClip KickClip => LoadClipIfNeeded(ref kickClip, kickClipResourcePath);
        public AudioClip WaveClip => LoadClipIfNeeded(ref waveClip, waveClipResourcePath);
        public AudioClip GoalClip => LoadClipIfNeeded(ref goalClip, goalClipResourcePath);
        public AudioClip RoundStartClip => LoadClipIfNeeded(ref roundStartClip, roundStartClipResourcePath);
        public AudioClip GameplayBgmClip => LoadClipIfNeeded(ref gameplayBgmClip, gameplayBgmResourcePath);

        public float KickVolume => Mathf.Clamp01(kickVolume);
        public float WaveVolume => Mathf.Clamp01(waveVolume);
        public float GoalVolume => Mathf.Clamp01(goalVolume);
        public float RoundStartVolume => Mathf.Clamp01(roundStartVolume);
        public float GameplayBgmVolume => Mathf.Clamp01(gameplayBgmVolume);

        private static AudioClip LoadClipIfNeeded(ref AudioClip clip, string resourcePath)
        {
            if (clip == null && !string.IsNullOrWhiteSpace(resourcePath))
            {
                clip = Resources.Load<AudioClip>(resourcePath);
            }

            return clip;
        }
    }
}
