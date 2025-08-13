using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    private const string STARS_KEY_PREFIX = "LevelStars_";
    private const string UNLOCKED_LEVEL_KEY = "HighestLevelUnlocked";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this manager across scene loads
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveStars(int levelIndex, int stars)
    {
        string key = STARS_KEY_PREFIX + levelIndex;
        int currentBest = LoadStars(levelIndex);

        if (stars > currentBest)
        {
            PlayerPrefs.SetInt(key, stars);
            PlayerPrefs.Save();
            Debug.Log($"New best for level {levelIndex}: {stars} stars!");
        }
    }

    public int LoadStars(int levelIndex)
    {
        string key = STARS_KEY_PREFIX + levelIndex;
        return PlayerPrefs.GetInt(key, 0); // Default to 0 stars if not found
    }

    // Optional: A method to reset all progress
    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("All player progress has been reset.");
    }

    public void UnlockLevel(int levelIndex)
    {
        int highestLevel = GetHighestUnlockedLevel();
        if (levelIndex > highestLevel)
        {
            PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, levelIndex);
            PlayerPrefs.Save();
        }
    }

    public int GetHighestUnlockedLevel()
    {
        // Level 0 is always unlocked.
        return PlayerPrefs.GetInt(UNLOCKED_LEVEL_KEY, 0);
    }
}
