using DG.Tweening;
using UnityEngine;

namespace Sludge.UI
{
    public enum UiPanel { MainMenu, LevelSelect, Game, BetweenRoundsMenu }

    public class UiPanels : MonoBehaviour
    {
        public static UiPanels Instance;

        const float ChangeTime = 0.1f;

        public GameObject PanelBackground;
        public GameObject PanelMainMenu;
        public GameObject PanelLevelSelect;
        public GameObject PanelGame;
        public GameObject PanelBetweenRoundsMenu;

        Vector2 panelLevelSelectHidePos;
        Vector2 panelLevelSelectShowPos = new Vector3(110, -46);
        Vector2 panelMainMenuHidePos;
        Vector2 panelMainMenuShowPos;
        Vector2 panelGameHidePos = new Vector2(0, 40);
        Vector2 panelGameShowPos = new Vector2(0, 0);
        Vector2 panelBetweenRoundsHidePos = new Vector2(0, 30);
        Vector2 panelBetweenRoundsShowPos = new Vector2(0, -40);

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            PanelBackground.SetActive(true);
            PanelMainMenu.SetActive(true);
            PanelLevelSelect.SetActive(true);
            PanelGame.SetActive(true);

            panelLevelSelectHidePos = PanelLevelSelect.GetComponent<RectTransform>().anchoredPosition;
            panelMainMenuShowPos = PanelMainMenu.GetComponent<RectTransform>().anchoredPosition;
            panelMainMenuHidePos = panelMainMenuShowPos + Vector2.left * 760;
        }

        public void ShowBackground() => PanelBackground.SetActive(true);
        public void HideBackground() => PanelBackground.SetActive(false);

        public YieldInstruction ShowPanel(UiPanel panel, bool instant = false) => ShowPanel(panel, show: true, instant);
        public YieldInstruction HidePanel(UiPanel panel, bool instant = false) => ShowPanel(panel, show: false, instant);

        YieldInstruction ShowPanel(UiPanel panel, bool show, bool instant = false)
        {
            float time = instant ? 0 : ChangeTime;

            switch (panel)
            {
                case UiPanel.MainMenu:
                    return PanelMainMenu.GetComponent<RectTransform>().
                        DOAnchorPos(show ? panelMainMenuShowPos : panelMainMenuHidePos, time).SetEase(Ease.OutCubic).WaitForCompletion();

                case UiPanel.LevelSelect:
                    return PanelLevelSelect.GetComponent<RectTransform>().
                        DOAnchorPos(show ? panelLevelSelectShowPos : panelLevelSelectHidePos, time).SetEase(Ease.OutCubic).WaitForCompletion();

                case UiPanel.Game:
                    return PanelGame.GetComponent<RectTransform>().
                        DOAnchorPos(show ? panelGameShowPos : panelGameHidePos, time).SetEase(Ease.OutCubic).WaitForCompletion();

                case UiPanel.BetweenRoundsMenu:
                    return PanelBetweenRoundsMenu.GetComponent<RectTransform>().
                        DOAnchorPos(show ? panelBetweenRoundsShowPos : panelBetweenRoundsHidePos, time).SetEase(Ease.OutCubic).WaitForCompletion();

                default:
                    Debug.LogError($"Unknown panel: {panel}");
                    return null;
            }
        }
    }
}
