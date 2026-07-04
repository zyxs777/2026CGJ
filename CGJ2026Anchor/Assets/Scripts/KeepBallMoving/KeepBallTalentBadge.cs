using UnityEngine;
using UnityEngine.UI;

namespace KeepBallMoving
{
    public sealed class KeepBallTalentBadge : MonoBehaviour
    {
        private const float BadgeWidth = 188f;
        private const float BadgeHeight = 44f;
        private const float IconSize = 30f;

        [SerializeField] private Image background;
        [SerializeField] private Image accentBar;
        [SerializeField] private Image iconFrame;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Text countText;

        private RectTransform rectTransform;

        public RectTransform RectTransform
        {
            get
            {
                EnsureBuilt();
                return rectTransform;
            }
        }

        public void SetData(Sprite panelSprite, Sprite iconSprite, string talentName, int count, int maxCount, Color accentColor, Team team)
        {
            EnsureBuilt();

            bool alignRight = team == Team.Red;
            ConfigureForTeam(alignRight);

            Color backgroundColor = team == Team.Blue
                ? new Color(0.035f, 0.09f, 0.18f, 0.82f)
                : new Color(0.18f, 0.045f, 0.04f, 0.82f);

            background.sprite = panelSprite;
            background.color = backgroundColor;
            accentBar.sprite = panelSprite;
            accentBar.color = accentColor;
            iconFrame.sprite = panelSprite;
            iconFrame.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.28f);
            iconImage.sprite = iconSprite;
            iconImage.color = Color.white;

            nameText.text = talentName;
            nameText.alignment = alignRight ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            nameText.color = Color.white;

            countText.text = $"x{count}/{maxCount}";
            countText.alignment = alignRight ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            countText.color = new Color(0.86f, 0.9f, 1f, 1f);
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

            rectTransform.sizeDelta = new Vector2(BadgeWidth, BadgeHeight);

            background = EnsureImage("Background", background, Vector2.zero, Vector2.one, Vector2.one * 0.5f, Vector2.zero, Vector2.zero);
            accentBar = EnsureImage("AccentBar", accentBar, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(4f, 0f));
            iconFrame = EnsureImage("IconFrame", iconFrame, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.one * 0.5f, new Vector2(24f, 0f), new Vector2(IconSize + 8f, IconSize + 8f));
            iconImage = EnsureImage("Icon", iconImage, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.one * 0.5f, new Vector2(24f, 0f), new Vector2(IconSize, IconSize));
            nameText = EnsureText("Name", nameText, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(48f, 0f), new Vector2(-76f, 0f), 17, FontStyle.Bold);
            countText = EnsureText("Count", countText, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-70f, 0f), new Vector2(-8f, 0f), 15, FontStyle.Bold);
        }

        private void ConfigureForTeam(bool alignRight)
        {
            RectTransform accentRect = accentBar.rectTransform;
            RectTransform iconFrameRect = iconFrame.rectTransform;
            RectTransform iconRect = iconImage.rectTransform;
            RectTransform nameRect = nameText.rectTransform;
            RectTransform countRect = countText.rectTransform;

            if (alignRight)
            {
                accentRect.anchorMin = new Vector2(1f, 0f);
                accentRect.anchorMax = new Vector2(1f, 1f);
                accentRect.pivot = new Vector2(1f, 0.5f);
                accentRect.anchoredPosition = Vector2.zero;
                accentRect.sizeDelta = new Vector2(4f, 0f);

                iconFrameRect.anchorMin = iconFrameRect.anchorMax = new Vector2(1f, 0.5f);
                iconFrameRect.pivot = Vector2.one * 0.5f;
                iconFrameRect.anchoredPosition = new Vector2(-24f, 0f);

                iconRect.anchorMin = iconRect.anchorMax = new Vector2(1f, 0.5f);
                iconRect.pivot = Vector2.one * 0.5f;
                iconRect.anchoredPosition = new Vector2(-24f, 0f);

                nameRect.anchorMin = new Vector2(0f, 0f);
                nameRect.anchorMax = new Vector2(1f, 1f);
                nameRect.pivot = new Vector2(1f, 0.5f);
                nameRect.offsetMin = new Vector2(76f, 0f);
                nameRect.offsetMax = new Vector2(-48f, 0f);

                countRect.anchorMin = new Vector2(0f, 0f);
                countRect.anchorMax = new Vector2(0f, 1f);
                countRect.pivot = new Vector2(0f, 0.5f);
                countRect.offsetMin = new Vector2(8f, 0f);
                countRect.offsetMax = new Vector2(70f, 0f);
            }
            else
            {
                accentRect.anchorMin = new Vector2(0f, 0f);
                accentRect.anchorMax = new Vector2(0f, 1f);
                accentRect.pivot = new Vector2(0f, 0.5f);
                accentRect.anchoredPosition = Vector2.zero;
                accentRect.sizeDelta = new Vector2(4f, 0f);

                iconFrameRect.anchorMin = iconFrameRect.anchorMax = new Vector2(0f, 0.5f);
                iconFrameRect.pivot = Vector2.one * 0.5f;
                iconFrameRect.anchoredPosition = new Vector2(24f, 0f);

                iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
                iconRect.pivot = Vector2.one * 0.5f;
                iconRect.anchoredPosition = new Vector2(24f, 0f);

                nameRect.anchorMin = new Vector2(0f, 0f);
                nameRect.anchorMax = new Vector2(1f, 1f);
                nameRect.pivot = new Vector2(0f, 0.5f);
                nameRect.offsetMin = new Vector2(48f, 0f);
                nameRect.offsetMax = new Vector2(-76f, 0f);

                countRect.anchorMin = new Vector2(1f, 0f);
                countRect.anchorMax = new Vector2(1f, 1f);
                countRect.pivot = new Vector2(1f, 0.5f);
                countRect.offsetMin = new Vector2(-70f, 0f);
                countRect.offsetMax = new Vector2(-8f, 0f);
            }
        }

        private Image EnsureImage(string childName, Image current, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
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

            RectTransform childRect = current.rectTransform;
            childRect.anchorMin = anchorMin;
            childRect.anchorMax = anchorMax;
            childRect.pivot = pivot;
            childRect.anchoredPosition = anchoredPosition;
            childRect.sizeDelta = sizeDelta;
            return current;
        }

        private Text EnsureText(string childName, Text current, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 offsetMin, Vector2 offsetMax, int fontSize, FontStyle fontStyle)
        {
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

            RectTransform childRect = current.rectTransform;
            childRect.anchorMin = anchorMin;
            childRect.anchorMax = anchorMax;
            childRect.pivot = pivot;
            childRect.offsetMin = offsetMin;
            childRect.offsetMax = offsetMax;

            current.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            current.fontSize = fontSize;
            current.fontStyle = fontStyle;
            current.raycastTarget = false;
            current.horizontalOverflow = HorizontalWrapMode.Overflow;
            current.verticalOverflow = VerticalWrapMode.Truncate;
            return current;
        }
    }
}
