using UnityEngine;
using UnityEngine.UI;

namespace KeepBallMoving
{
    public sealed class KeepBallScoreboard : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text infoText;
        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private string backgroundSpriteResourcePath = "KeepBallMoving/point";
        [SerializeField] private Color scoreTextColor = Color.white;
        [SerializeField] private Color infoTextColor = Color.white;
        [SerializeField] private Vector2 size = new Vector2(520f, 118f);

        private RectTransform rectTransform;

        public RectTransform RectTransform
        {
            get
            {
                EnsureBuilt();
                return rectTransform;
            }
        }

        public void SetScore(int blueScore, int redScore, int targetScore, string matchMode, string kickoffLabel)
        {
            EnsureBuilt();
            scoreText.text = $"蓝方   {blueScore} - {redScore}   红方";
            infoText.text = $"目标：先到 {Mathf.Max(1, targetScore)} 球 / {matchMode} / {kickoffLabel}开球";
        }

        private void Awake()
        {
            EnsureBuilt();
        }

        private void EnsureBuilt()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            if (rectTransform.sizeDelta == Vector2.zero && size.x > 0f && size.y > 0f)
            {
                rectTransform.sizeDelta = size;
            }

            if (backgroundSprite == null && !string.IsNullOrEmpty(backgroundSpriteResourcePath))
            {
                backgroundSprite = Resources.Load<Sprite>(backgroundSpriteResourcePath);
            }

            background = EnsureImage("Background", background, Vector2.zero, Vector2.one, Vector2.one * 0.5f, Vector2.zero, Vector2.zero);
            if (background.sprite == null)
            {
                background.sprite = backgroundSprite;
            }

            background.raycastTarget = false;

            scoreText = EnsureText("Score", scoreText, new Vector2(0f, 0.42f), new Vector2(1f, 0.82f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, 32, FontStyle.Bold);
            infoText = EnsureText("Info", infoText, new Vector2(0f, 0.2f), new Vector2(1f, 0.44f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, 15, FontStyle.Bold);

            scoreText.color = scoreTextColor;
            infoText.color = infoTextColor;
        }

        private Image EnsureImage(string childName, Image current, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 offsetMin, Vector2 offsetMax)
        {
            bool applyDefaultLayout = current == null;

            if (current == null)
            {
                Transform child = transform.Find(childName);
                if (child == null)
                {
                    GameObject childObject = new GameObject(childName, typeof(RectTransform));
                    childObject.transform.SetParent(transform, false);
                    child = childObject.transform;
                }

                current = child.GetComponent<Image>();
                if (current == null)
                {
                    current = child.gameObject.AddComponent<Image>();
                }
            }

            if (applyDefaultLayout)
            {
                RectTransform childRect = current.rectTransform;
                childRect.anchorMin = anchorMin;
                childRect.anchorMax = anchorMax;
                childRect.pivot = pivot;
                childRect.offsetMin = offsetMin;
                childRect.offsetMax = offsetMax;
            }

            return current;
        }

        private Text EnsureText(string childName, Text current, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 offsetMin, Vector2 offsetMax, int fontSize, FontStyle fontStyle)
        {
            bool applyDefaults = current == null;

            if (current == null)
            {
                Transform child = transform.Find(childName);
                if (child == null)
                {
                    GameObject childObject = new GameObject(childName, typeof(RectTransform));
                    childObject.transform.SetParent(transform, false);
                    child = childObject.transform;
                }

                current = child.GetComponent<Text>();
                if (current == null)
                {
                    current = child.gameObject.AddComponent<Text>();
                }
            }

            if (applyDefaults)
            {
                RectTransform childRect = current.rectTransform;
                childRect.anchorMin = anchorMin;
                childRect.anchorMax = anchorMax;
                childRect.pivot = pivot;
                childRect.offsetMin = offsetMin;
                childRect.offsetMax = offsetMax;
            }

            if (current.font == null)
            {
                current.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            if (applyDefaults || current.fontSize <= 0)
            {
                current.fontSize = fontSize;
                current.fontStyle = fontStyle;
                current.alignment = TextAnchor.MiddleCenter;
                current.horizontalOverflow = HorizontalWrapMode.Overflow;
                current.verticalOverflow = VerticalWrapMode.Truncate;
            }

            current.raycastTarget = false;
            return current;
        }
    }
}
