using UnityEngine;
using System.Collections.Generic;

// Define the data structures for our levels.
[System.Serializable]
public class RopeConnection
{
    public int knotIndexA;
    public int knotIndexB;
}

[System.Serializable]
public class Level
{
    public string levelName;
    public Vector2[] knotPositions;
    public RopeConnection[] ropeConnections;

    [Header("Star Rating Thresholds")]
    public int threeStarMoves;
    public int twoStarMoves;
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject uselPrefab; // The "Uselok" prefab

    [Header("Level Data")]
    [SerializeField] private List<Level> levels;
    public int CurrentLevelIndex { get; private set; } = 0;

    private List<GameObject> activeLevelObjects = new List<GameObject>();

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
        // For now, we create the level data in code.
        CreateSampleLevels();
        LoadLevel(CurrentLevelIndex);
    }

    public void LoadLevel(int levelIndex)
    {
        UIManager.Instance.HideLevelCompletePanel();

        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogError("Invalid level index: " + levelIndex);
            return;
        }

        CurrentLevelIndex = levelIndex;
        ClearCurrentLevel();

        Level level = levels[levelIndex];
        List<Uzelok> spawnedUzeloks = new List<Uzelok>();

        // 1. Instantiate all the knots (Uzeloks)
        foreach (Vector2 pos in level.knotPositions)
        {
            GameObject knotGO = Instantiate(uselPrefab, pos, Quaternion.identity);
            activeLevelObjects.Add(knotGO);
            spawnedUzeloks.Add(knotGO.GetComponent<Uzelok>());
        }

        // 2. Create the ropes (Svyaznoy components)
        foreach (RopeConnection conn in level.ropeConnections)
        {
            Uzelok uzelokA = spawnedUzeloks[conn.knotIndexA];
            Uzelok uzelokB = spawnedUzeloks[conn.knotIndexB];

            // Add the Svyaznoy component to one knot and connect it to the other
            Svyaznoy rope = uzelokA.gameObject.AddComponent<Svyaznoy>();
            rope.Initialize(uzelokB);
        }
    }

    private void ClearCurrentLevel()
    {
        foreach (GameObject obj in activeLevelObjects)
        {
            Destroy(obj);
        }
        activeLevelObjects.Clear();
    }

    public void LoadNextLevel()
    {
        int nextLevel = (CurrentLevelIndex + 1) % levels.Count;
        LoadLevel(nextLevel);
    }

    public void ReloadCurrentLevel()
    {
        LoadLevel(CurrentLevelIndex);
    }

    public Level GetCurrentLevel()
    {
        if (CurrentLevelIndex >= 0 && CurrentLevelIndex < levels.Count)
        {
            return levels[CurrentLevelIndex];
        }
        return null;
    }

    private void CreateSampleLevels()
    {
        levels = new List<Level>();

        // Level 1: Simple square
        levels.Add(new Level
        {
            levelName = "Square One",
            knotPositions = new Vector2[]
            {
                new Vector2(-2, 2), new Vector2(2, 2),
                new Vector2(-2, -2), new Vector2(2, -2)
            },
            ropeConnections = new RopeConnection[]
            {
                new RopeConnection { knotIndexA = 0, knotIndexB = 1 },
                new RopeConnection { knotIndexA = 1, knotIndexB = 3 },
                new RopeConnection { knotIndexA = 3, knotIndexB = 2 },
                new RopeConnection { knotIndexA = 2, knotIndexB = 0 }
            },
            threeStarMoves = 5,
            twoStarMoves = 8
        });

        // Level 2: A simple crossing pattern
        levels.Add(new Level
        {
            levelName = "Cross",
            knotPositions = new Vector2[]
            {
                new Vector2(-2, 2), new Vector2(2, -2),
                new Vector2(2, 2), new Vector2(-2, -2)
            },
            ropeConnections = new RopeConnection[]
            {
                new RopeConnection { knotIndexA = 0, knotIndexB = 1 },
                new RopeConnection { knotIndexA = 2, knotIndexB = 3 }
            },
            threeStarMoves = 2,
            twoStarMoves = 4
        });
    }
}
