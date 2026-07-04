using UnityEngine;


    public class IndicatorRenderer : MonoBehaviour {
        private SpriteRenderer _img;
        private float _angle;
        private float _radius;
        private int _angleHash;

        void Awake() {
            _img = GetComponentInChildren<SpriteRenderer>();
            _angleHash = Shader.PropertyToID("_Angle");
        }

        public void SetSize(float radius, float angle) {
            _angle = angle;
            _radius = radius;
            UpdateSize();
        }

        void Update() {
            UpdateSize();
        }


        void UpdateSize() {
            _img.transform.localScale = new Vector3(_radius, _radius, 1);
            _img.material.SetFloat(_angleHash, _angle * Mathf.Deg2Rad);
        }

        public void Show() {
            _img.enabled = true;
        }

        public void Hide() {
            _img.enabled = false;
        }
    }
