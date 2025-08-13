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
        levelButtonTemplate = new GameObject("LevelButtonTemplate");
        levelButtonTemplate.AddComponent<RectTransform>();
        Image bgImage = levelButtonTemplate.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        levelButtonTemplate.AddComponent<Button>();

        // Add a child for the text
        GameObject textGO = new GameObject("LevelText");
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.SetParent(levelButtonTemplate.transform, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI levelText = textGO.AddComponent<TextMeshProUGUI>();
        levelText.alignment = TextAlignmentOptions.Center;
        levelText.fontSize = 48;
        levelText.color = Color.white;

        // Add a child for the lock icon
        GameObject lockGO = new GameObject("LockIcon");
        RectTransform lockRect = lockGO.AddComponent<RectTransform>();
        lockRect.SetParent(levelButtonTemplate.transform, false);
        lockRect.anchorMin = new Vector2(0.5f, 0.5f);
        lockRect.anchorMax = new Vector2(0.5f, 0.5f);
        lockRect.sizeDelta = new Vector2(50, 50);
        Image lockImage = lockGO.AddComponent<Image>();
        lockImage.color = Color.white; // Assume a lock sprite is assigned elsewhere or use a placeholder color
        lockGO.SetActive(false);

        // Add star icons (placeholders)
        GameObject starsContainer = new GameObject("StarsContainer");
        RectTransform starsRect = starsContainer.AddComponent<RectTransform>();
        starsRect.SetParent(levelButtonTemplate.transform, false);
        starsRect.anchorMin = new Vector2(0.5f, 0.2f);
        starsRect.anchorMax = new Vector2(0.5f, 0.2f);
        starsRect.sizeDelta = new Vector2(100, 20);
        HorizontalLayoutGroup starLayout = starsContainer.AddComponent<HorizontalLayoutGroup>();
        starLayout.spacing = 5;
        GameObject[] starGOs = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject starGO = new GameObject("Star_" + i);
            starGO.transform.SetParent(starsContainer.transform, false);
            starGO.AddComponent<Image>().color = Color.yellow;
            starGOs[i] = starGO;
        }

        // Add the LevelButton script and assign references
        LevelButton lbScript = levelButtonTemplate.AddComponent<LevelButton>();
        lbScript.levelText = levelText;
        lbScript.lockIcon = lockGO;
        lbScript.stars = starGOs;

        levelButtonTemplate.SetActive(false);
    }

    public void PopulateLevelSelect()
    {
        foreach (Transform child in levelButtonContainer)
        {
            Destroy(child.gameObject);
        }

        if (LevelManager.Instance == null || LevelManager.Instance.levels == null) return;

        int highestUnlockedLevel = ProgressManager.Instance.GetHighestUnlockedLevel();

        for (int i = 0; i < LevelManager.Instance.levels.Count; i++)
        {
            GameObject buttonGO = Instantiate(levelButtonTemplate, levelButtonContainer);
            buttonGO.name = "Level_" + (i + 1);

            LevelButton levelButton = buttonGO.GetComponent<LevelButton>();

            if (i <= highestUnlockedLevel)
            {
                int stars = ProgressManager.Instance.LoadStars(i);
                levelButton.Initialize(i, stars);
            }
            else
            {
                levelButton.SetLockedState();
            }

            buttonGO.SetActive(true);
        }
    }

    public void ShowPanel(GameObject panelToShow)
    {
        foreach (var panel in allPanels)
        {
            panel?.SetActive(panel == panelToShow);
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
        LevelManager.Instance.LoadRandomLevel(8, 4, 0);
        GameManager.Instance.ResetMoveCount();
        ShowInGameHUD();
    }

    private void OnEndlessModeHardClicked()
    {
        SoundManager.Instance.PlayButtonClick();
        LevelManager.Instance.LoadRandomLevel(10, 5, 5);
        GameManager.Instance.ResetMoveCount();
        ShowInGameHUD();
    }
}
