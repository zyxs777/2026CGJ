using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KeepBallMoving
{
    [DisallowMultipleComponent]
    public sealed class KeepBallMainMenuView : MonoBehaviour
    {
        [Header("Resources")]
        [SerializeField] private Sprite coverSprite;
        [SerializeField] private string coverSpriteResourcePath = "KeepBallMoving/Cover";

        [Header("References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasScaler canvasScaler;
        [SerializeField] private GraphicRaycaster graphicRaycaster;
        [SerializeField] private Image coverImage;
        [SerializeField] private Image overlayImage;
        [SerializeField] private Image panelImage;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text subtitleLabel;
        [SerializeField] private Button pveButton;
        [SerializeField] private Button pvpButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Text pveLabel;
        [SerializeField] private Text pvpLabel;
        [SerializeField] private Text exitLabel;

        public void Initialize(Action onPve, Action onPvp, Action onExit)
        {
            ResolveReferences();
            EnsureEventSystem();
            BindButton(pveButton, onPve);
            BindButton(pvpButton, onPvp);
            BindButton(exitButton, onExit);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
            }

            if (canvasScaler == null)
            {
                canvasScaler = GetComponent<CanvasScaler>();
            }

            if (graphicRaycaster == null)
            {
                graphicRaycaster = GetComponent<GraphicRaycaster>();
            }

            coverImage = ResolveChild(coverImage, "Cover");
            overlayImage = ResolveChild(overlayImage, "Overlay");
            panelImage = ResolveChild(panelImage, "MenuPanel");
            titleLabel = ResolveChild(titleLabel, "Title");
            subtitleLabel = ResolveChild(subtitleLabel, "Subtitle");
            pveButton = ResolveChild(pveButton, "PVEButton");
            pvpButton = ResolveChild(pvpButton, "PVPButton");
            exitButton = ResolveChild(exitButton, "ExitButton");
            pveLabel = ResolveLabel(pveButton, pveLabel);
            pvpLabel = ResolveLabel(pvpButton, pvpLabel);
            exitLabel = ResolveLabel(exitButton, exitLabel);

            LoadCoverSpriteIfNeeded();
            if (coverImage != null && coverImage.sprite == null && coverSprite != null)
            {
                coverImage.sprite = coverSprite;
            }
        }

        private void EnsureEventSystem()
        {
            if (!Application.isPlaying || EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystemObject.transform.SetParent(transform.root, false);
        }

        private void LoadCoverSpriteIfNeeded()
        {
            if (coverSprite != null || string.IsNullOrWhiteSpace(coverSpriteResourcePath))
            {
                return;
            }

            coverSprite = Resources.Load<Sprite>(coverSpriteResourcePath);
        }

        private T ResolveChild<T>(T current, string childName) where T : Component
        {
            return current != null ? current : FindChildComponent<T>(transform, childName);
        }

        private static Text ResolveLabel(Button button, Text current)
        {
            if (current != null || button == null)
            {
                return current;
            }

            Transform label = button.transform.Find("Label");
            return label != null ? label.GetComponent<Text>() : FindChildComponent<Text>(button.transform, "Label");
        }

        private static T FindChildComponent<T>(Transform root, string childName) where T : Component
        {
            if (root == null)
            {
                return null;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child.name == childName)
                {
                    T component = child.GetComponent<T>();
                    if (component != null)
                    {
                        return component;
                    }
                }

                T nested = FindChildComponent<T>(child, childName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }

        private static void BindButton(Button button, Action callback)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            if (callback != null)
            {
                button.onClick.AddListener(() => callback());
            }
        }
    }
}
