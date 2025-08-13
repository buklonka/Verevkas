using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;
    public GameObject inGameHudPanel; // Made public for LevelButton
    [SerializeField] private GameObject levelCompletePanel;

    [Header("Level Select")]
    [SerializeField] private Transform levelButtonContainer;
    private GameObject levelButtonTemplate;

    [Header("Star Display")]
    [SerializeField] private GameObject[] starIcons;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button endlessModeButton;
    [SerializeField] private Button endlessModeHardButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button resetButton;

    private List<GameObject> allPanels;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        allPanels = new List<GameObject> { mainMenuPanel, levelSelectPanel, inGameHudPanel, levelCompletePanel };
    }

    void Start()
    {
        playButton?.onClick.AddListener(OnPlayClicked);
        endlessModeButton?.onClick.AddListener(OnEndlessModeClicked);
        endlessModeHardButton?.onClick.AddListener(OnEndlessModeHardClicked);
        nextLevelButton?.onClick.AddListener(OnNextLevelClicked);
        resetButton?.onClick.AddListener(OnResetClicked);

        CreateLevelButtonTemplate();

        ShowPanel(mainMenuPanel);
    }

    private void CreateLevelButtonTemplate()
    {
        // Programmatically create a template for the level buttons
        levelButtonTemplate = new GameObject("LevelButtonTemplate");
        levelButtonTemplate.AddComponent<RectTransform>();
        Image bgImage = levelButtonTemplate.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        levelButtonTemplate.AddComponent<Button>();

        // Add a child for the text
        GameObject textGO = new GameObject("LevelText");
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.SetParent(levelButtonTemplate.transform);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 48;
        text.color = Color.white;

        // Add the LevelButton script
        levelButtonTemplate.AddComponent<LevelButton>();

        levelButtonTemplate.SetActive(false); // Keep template inactive
    }

    public void PopulateLevelSelect()
    {
        // Clear old buttons
        foreach (Transform child in levelButtonContainer)
        {
            Destroy(child.gameObject);
        }

        if (LevelManager.Instance == null || LevelManager.Instance.levels == null) return;

        for (int i = 0; i < LevelManager.Instance.levels.Count; i++)
        {
            GameObject buttonGO = Instantiate(levelButtonTemplate, levelButtonContainer);
            buttonGO.name = "Level_" + (i + 1);

            int stars = ProgressManager.Instance.LoadStars(i);

            LevelButton levelButton = buttonGO.GetComponent<LevelButton>();
            levelButton.Initialize(i, stars);

            buttonGO.SetActive(true);
        }
    }

    public void ShowPanel(GameObject panelToShow)
    {
        foreach (var panel in allPanels)
        {
            if (panel != null)
            {
                panel.SetActive(panel == panelToShow);
            }
        }
    }

    public void ShowLevelCompletePanel()
    {
        ShowPanel(levelCompletePanel);
        SetStars(0);
    }

    public void ShowInGameHUD()
    {
        ShowPanel(inGameHudPanel);
    }

    public void SetStars(int starCount)
    {
        if (starIcons == null) return;
        for (int i = 0; i < starIcons.Length; i++)
        {
            starIcons[i]?.SetActive(i < starCount);
        }
    }

    private void OnPlayClicked()
    {
        SoundManager.Instance.PlayButtonClick();
        PopulateLevelSelect();
        ShowPanel(levelSelectPanel);
    }

    private void OnNextLevelClicked()
    {
        SoundManager.Instance.PlayButtonClick();
        LevelManager.Instance.LoadNextLevel();
        GameManager.Instance.ResetMoveCount();
        ShowPanel(inGameHudPanel);
    }

    private void OnResetClicked()
    {
        SoundManager.Instance.PlayButtonClick();
        GameManager.Instance.ResetGame();
    }

    private void OnEndlessModeClicked()
    {
        SoundManager.Instance.PlayButtonClick();
        // Load a random level with some default parameters (0 nails)
        LevelManager.Instance.LoadRandomLevel(8, 4, 0);
        GameManager.Instance.ResetMoveCount();
        ShowInGameHUD();
    }

    private void OnEndlessModeHardClicked()
    {
        SoundManager.Instance.PlayButtonClick();
        // Load a random level with more knots and some nails
        LevelManager.Instance.LoadRandomLevel(10, 5, 5);
        GameManager.Instance.ResetMoveCount();
        ShowInGameHUD();
    }
}
