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

        private Button[] menuButtons;

        public void Initialize(Action onPve, Action onPvp, Action onExit)
        {
            ResolveReferences();
            EnsureEventSystem();
            BindButton(pveButton, onPve);
            BindButton(pvpButton, onPvp);
            BindButton(exitButton, onExit);
            RefreshButtonList();
            SelectOption(0);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
            if (visible)
            {
                ResolveReferences();
                RefreshButtonList();
                SelectOption(0);
            }
        }

        public int OptionCount
        {
            get
            {
                RefreshButtonList();
                return menuButtons != null ? menuButtons.Length : 0;
            }
        }

        public void SelectOption(int index)
        {
            RefreshButtonList();
            if (menuButtons == null || menuButtons.Length == 0)
            {
                return;
            }

            int safeIndex = Mathf.Clamp(index, 0, menuButtons.Length - 1);
            Button button = menuButtons[safeIndex];
            if (button == null)
            {
                return;
            }

            button.Select();
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(button.gameObject);
            }
        }

        public void ConfirmOption(int index)
        {
            RefreshButtonList();
            if (menuButtons == null || menuButtons.Length == 0)
            {
                return;
            }

            int safeIndex = Mathf.Clamp(index, 0, menuButtons.Length - 1);
            Button button = menuButtons[safeIndex];
            if (button != null && button.interactable)
            {
                button.onClick.Invoke();
            }
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
            RefreshButtonList();

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

        private void RefreshButtonList()
        {
            menuButtons = new[] { pveButton, pvpButton, exitButton };
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
