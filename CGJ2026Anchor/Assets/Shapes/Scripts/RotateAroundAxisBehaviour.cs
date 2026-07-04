using UnityEngine;

namespace RGScript.ScriptsSkinSpecialDeals
{
    public class RotateAroundAxisBehaviour : MonoBehaviour
    {
        public enum RotateSpeed {
            Stopped,
            Slow,
            Fast
        }

        public enum RotateAxis {
            X,Y,Z
        }
    
        public RotateSpeed speed = RotateSpeed.Fast;
        public float slowRotateSpeed = 5;
        public float fastRotateSpeed = 20;
        public RotateAxis rotateAroundAxis = RotateAxis.Z;
        public Space relativeTo = Space.Self;
        public bool reverse;
        public bool useUnscaledTime = false;

        private void Update() {
            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            Rotate(deltaTime);
        }

        private void Rotate(float deltaTime) {
            Vector3 rotateSpeed = GetRotateSpeed();
            if (reverse) {
                rotateSpeed *= -1;
            }

            transform.Rotate(rotateSpeed * deltaTime, relativeTo);
        }

        private Vector3 GetRotateSpeed() {
            if (speed.Equals(RotateSpeed.Stopped)) {
                return Vector3.zero;
            }

            var speedValue = speed switch {
                RotateSpeed.Fast => fastRotateSpeed,
                RotateSpeed.Slow => slowRotateSpeed,
                _ => 0f
            };

            return rotateAroundAxis switch {
                RotateAxis.X => new Vector3(1, 0, 0) * speedValue,
                RotateAxis.Y => new Vector3(0, 1, 0) * speedValue,
                RotateAxis.Z => new Vector3(0, 0, 1) * speedValue,
                _ => Vector3.zero
            };
        }
    }
}
