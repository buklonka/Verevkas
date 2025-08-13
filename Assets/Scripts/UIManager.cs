using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject[] starIcons;

    [Header("Buttons")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button resetButton;


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
    }

    void Start()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }

        // Add listeners for the buttons
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        }
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetClicked);
        }
    }

    public void ShowLevelCompletePanel()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            // Hide all stars initially
            SetStars(0);
        }
    }

    public void HideLevelCompletePanel()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }

    public void SetStars(int starCount)
    {
        if (starIcons == null) return;

        for (int i = 0; i < starIcons.Length; i++)
        {
            if (i < starCount)
            {
                starIcons[i].SetActive(true);
            }
            else
            {
                starIcons[i].SetActive(false);
            }
        }
    }

    private void OnNextLevelClicked()
    {
        LevelManager.Instance.LoadNextLevel();
        GameManager.Instance.ResetMoveCount();
        HideLevelCompletePanel();
    }

    private void OnResetClicked()
    {
        GameManager.Instance.ResetGame();
    }
}
