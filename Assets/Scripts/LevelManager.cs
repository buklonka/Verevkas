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

    [Header("Obstacles")]
    public List<Vector2> nailPositions;
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject uselPrefab; // The "Uselok" prefab
    private GameObject nailTemplate;

    [Header("Level Data")]
    public List<Level> levels; // Made public for UIManager
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
        CreateNailTemplate();
        CreateLevelCampaign(); // Changed from CreateSampleLevels
    }

    private void CreateNailTemplate()
    {
        nailTemplate = new GameObject("NailTemplate");
        nailTemplate.AddComponent<Nail>();

        var sr = nailTemplate.AddComponent<SpriteRenderer>();
        if (uselPrefab != null && uselPrefab.GetComponent<SpriteRenderer>() != null)
        {
            sr.sprite = uselPrefab.GetComponent<SpriteRenderer>().sprite;
        }
        sr.color = Color.grey;
        nailTemplate.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        var collider = nailTemplate.AddComponent<CircleCollider2D>();
        collider.radius = 0.5f;

        nailTemplate.SetActive(false);
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogError("Invalid level index: " + levelIndex);
            return;
        }
        CurrentLevelIndex = levelIndex;
        LoadLevel(levels[levelIndex]);
    }

    public void LoadLevel(Level level)
    {
        UIManager.Instance.HideLevelCompletePanel();
        ClearCurrentLevel();

        if (level.nailPositions != null)
        {
            foreach (Vector2 pos in level.nailPositions)
            {
                GameObject nailGO = Instantiate(nailTemplate, pos, Quaternion.identity);
                nailGO.SetActive(true);
                activeLevelObjects.Add(nailGO);
            }
        }

        List<Uzelok> spawnedUzeloks = new List<Uzelok>();
        foreach (Vector2 pos in level.knotPositions)
        {
            GameObject knotGO = Instantiate(uselPrefab, pos, Quaternion.identity);
            activeLevelObjects.Add(knotGO);
            spawnedUzeloks.Add(knotGO.GetComponent<Uzelok>());
        }

        foreach (RopeConnection conn in level.ropeConnections)
        {
            Uzelok uzelokA = spawnedUzeloks[conn.knotIndexA];
            Uzelok uzelokB = spawnedUzeloks[conn.knotIndexB];

            GameObject ropeGO = new GameObject("DynamicRope_" + conn.knotIndexA + "_" + conn.knotIndexB);
            ropeGO.transform.SetParent(transform);
            activeLevelObjects.Add(ropeGO);

            DynamicRope rope = ropeGO.AddComponent<DynamicRope>();
            rope.startPoint = uzelokA.transform;
            rope.endPoint = uzelokB.transform;
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

    public void LoadRandomLevel(int knotCount, int extraRopes, int nailCount)
    {
        CurrentLevelIndex = -1;
        Level generatedLevel = LevelGenerator.GenerateLevel(knotCount, extraRopes, nailCount);
        LoadLevel(generatedLevel);
    }

    private void CreateLevelCampaign()
    {
        levels = new List<Level>();
        int totalLevels = 50;

        for (int i = 0; i < totalLevels; i++)
        {
            int knotCount;
            int extraRopeCount;
            int nailCount;

            // Difficulty Curve Logic
            if (i < 10) // Levels 1-10: Easy
            {
                knotCount = Random.Range(4, 6); // 4-5 knots
                extraRopeCount = Random.Range(1, 3); // 1-2 extra ropes
                nailCount = 0;
            }
            else if (i < 20) // Levels 11-20: Medium
            {
                knotCount = Random.Range(6, 8); // 6-7 knots
                extraRopeCount = Random.Range(2, 4); // 2-3 extra ropes
                nailCount = Random.Range(0, 2); // 0-1 nails
            }
            else if (i < 30) // Levels 21-30: Hard
            {
                knotCount = Random.Range(8, 10); // 8-9 knots
                extraRopeCount = Random.Range(4, 6); // 4-5 extra ropes
                nailCount = Random.Range(2, 4); // 2-3 nails
            }
            else if (i < 40) // Levels 31-40: Very Hard
            {
                knotCount = Random.Range(10, 13); // 10-12 knots
                extraRopeCount = Random.Range(6, 8); // 6-7 extra ropes
                nailCount = Random.Range(4, 6); // 4-5 nails
            }
            else // Levels 41-50: Expert
            {
                knotCount = Random.Range(13, 16); // 13-15 knots
                extraRopeCount = Random.Range(8, 11); // 8-10 extra ropes
                nailCount = Random.Range(6, 9); // 6-8 nails
            }

            Level generatedLevel = LevelGenerator.GenerateLevel(knotCount, extraRopeCount, nailCount);
            generatedLevel.levelName = "Level " + (i + 1);
            levels.Add(generatedLevel);
        }
    }
}
