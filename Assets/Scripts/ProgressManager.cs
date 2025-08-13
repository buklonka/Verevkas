using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    private const string STARS_KEY_PREFIX = "LevelStars_";

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
}
