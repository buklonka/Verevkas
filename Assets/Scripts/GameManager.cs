using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Text moveCounterText;

    private int moveCount = 0;

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

    void OnEnable()
    {
        DynamicRope.OnAllRopesUncrossed += OnLevelComplete;
    }

    void OnDisable()
    {
        DynamicRope.OnAllRopesUncrossed -= OnLevelComplete;
    }

    void Start()
    {
        UpdateMoveCounterText();
    }

    void Update()
    {
        // This is not the most optimal place, but it will work for now.
        // A better approach might be to only check after a rope has stopped moving.
        DynamicRope.CheckForWinCondition();
    }

    private void OnLevelComplete()
    {
        Level currentLevel = LevelManager.Instance.GetCurrentLevel();
        if (currentLevel == null) return;

        int stars = 1;
        if (moveCount <= currentLevel.threeStarMoves)
        {
            stars = 3;
        }
        else if (moveCount <= currentLevel.twoStarMoves)
        {
            stars = 2;
        }

        Debug.Log("Level Complete! Stars earned: " + stars);

        ProgressManager.Instance.SaveStars(LevelManager.Instance.CurrentLevelIndex, stars);

        SoundManager.Instance.PlayLevelWin();
        UIManager.Instance.ShowLevelCompletePanel();
        UIManager.Instance.SetStars(stars);
    }

    public void IncrementMoveCount()
    {
        moveCount++;
        UpdateMoveCounterText();
    }

    public void ResetMoveCount()
    {
        moveCount = 0;
        UpdateMoveCounterText();
    }

    private void UpdateMoveCounterText()
    {
        if (moveCounterText != null)
        {
            moveCounterText.text = "Moves: " + moveCount;
        }
    }

    public void ResetGame()
    {
        ResetMoveCount();
        LevelManager.Instance.ReloadCurrentLevel();
    }
}
