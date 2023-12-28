using DG.Tweening;
using UnityEngine;

namespace Sludge.UI
{
    public enum UiPanel { MainMenu, LevelSelect, Game, BetweenRoundsMenu, Settings }

    public class UiPanels : MonoBehaviour
    {
        public static UiPanels Instance;

        const float ChangeTime = 0.1f;

        public GameObject PanelBackground;
        public GameObject PanelMainMenu;
        public GameObject PanelLevelSelect;
        public GameObject PanelGame;
        public GameObject PanelBetweenRoundsMenu;
        public GameObject PanelSettings;

        Vector2 panelLevelSelectHidePos;
        Vector2 panelLevelSelectShowPos = new Vector3(110, -46);
        Vector2 panelMainMenuHidePos;
        Vector2 panelMainMenuShowPos;
        Vector2 panelGameHidePos = new Vector2(0, 40);
        Vector2 panelGameShowPos = new Vector2(0, 0);
        Vector2 panelBetweenRoundsHidePos = new Vector2(0, -320);
        Vector2 panelBetweenRoundsShowPos = new Vector2(0, -283);
        Vector2 panelSettingsHidePos;
        Vector2 panelSettingsShowPos = new Vector3(110, -46);

        private void Awake()
        {
            Instance = this;
        }

        public void SetAllActive(bool active)
        {
            PanelBackground.SetActive(active);
            PanelMainMenu.SetActive(active);
            PanelLevelSelect.SetActive(active);
            PanelGame.SetActive(active);
            PanelSettings.SetActive(active);
        }

        public void Init()
        {
            SetAllActive(true);
            panelLevelSelectHidePos = PanelLevelSelect.GetComponent<RectTransform>().anchoredPosition;
            panelMainMenuShowPos = PanelMainMenu.GetComponent<RectTransform>().anchoredPosition;
            panelMainMenuHidePos = panelMainMenuShowPos + Vector2.left * 760;
            panelSettingsHidePos = PanelSettings.GetComponent<RectTransform>().anchoredPosition;
        }

        public void ShowBackground() => PanelBackground.SetActive(true);
        public void HideBackground() => PanelBackground.SetActive(false);

        public YieldInstruction ShowPanel(UiPanel panel, bool instant = false) => ShowPanel(panel, show: true, instant);
        public YieldInstruction HidePanel(UiPanel panel, bool instant = false) => ShowPanel(panel, show: false, instant);

        GameObject GetPanelSettings(UiPanel panel, out Vector2 hidePos, out Vector2 showPos)
        {
            switch (panel)
            {
                case UiPanel.MainMenu:
                    hidePos = panelMainMenuHidePos;
                    showPos = panelMainMenuShowPos;
                    return PanelMainMenu;

                case UiPanel.LevelSelect:
                    hidePos = panelLevelSelectHidePos;
                    showPos = panelLevelSelectShowPos;
                    return PanelLevelSelect;

                case UiPanel.Game:
                    hidePos = panelGameHidePos;
                    showPos = panelGameShowPos;
                    return PanelGame;

                case UiPanel.BetweenRoundsMenu:
                    hidePos = panelBetweenRoundsHidePos;
                    showPos = panelBetweenRoundsShowPos;
                    return PanelBetweenRoundsMenu;

                case UiPanel.Settings:
                    hidePos = panelSettingsHidePos;
                    showPos = panelSettingsShowPos;
                    return PanelSettings;

                default:
                    Debug.LogError($"Unknown panel: {panel}");
                    hidePos = Vector2.zero;
                    showPos = Vector2.zero;
                    return null;
            }
        }

        YieldInstruction ShowPanel(UiPanel panel, bool show, bool instant = false)
        {
            float time = instant ? 0 : ChangeTime;
            var go = GetPanelSettings(panel, out var hidePos, out var showPos);
            if (show)
                go.SetActive(true);

            return go.GetComponent<RectTransform>().DOAnchorPos(show ? showPos : hidePos, time).
                SetEase(Ease.InOutCubic).OnComplete(() => go.SetActive(show)).
                WaitForCompletion();
        }
    }
}
