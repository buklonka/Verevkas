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
        CreateSampleLevels();
    }

    private void CreateNailTemplate()
    {
        nailTemplate = new GameObject("NailTemplate");
        nailTemplate.AddComponent<Nail>();

        var sr = nailTemplate.AddComponent<SpriteRenderer>();
        // For simplicity, we'll make it a grey circle. A proper sprite would be better.
        // I can't create a circle sprite, so I'll try to load one or use a default.
        // Let's assume the 'Usel' prefab has a circular sprite we can borrow.
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

        // 1. Instantiate Nails
        if (level.nailPositions != null)
        {
            foreach (Vector2 pos in level.nailPositions)
            {
                GameObject nailGO = Instantiate(nailTemplate, pos, Quaternion.identity);
                nailGO.SetActive(true);
                activeLevelObjects.Add(nailGO);
            }
        }

        // 2. Instantiate Knots
        List<Uzelok> spawnedUzeloks = new List<Uzelok>();
        foreach (Vector2 pos in level.knotPositions)
        {
            GameObject knotGO = Instantiate(uselPrefab, pos, Quaternion.identity);
            activeLevelObjects.Add(knotGO);
            spawnedUzeloks.Add(knotGO.GetComponent<Uzelok>());
        }

        // 3. Create Ropes
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

    private void CreateSampleLevels()
    {
        levels = new List<Level>();
        // ... (sample level data remains the same) ...
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

        // Level 3: Triangle with a nail in the middle
        levels.Add(new Level
        {
            levelName = "Triangle",
            knotPositions = new Vector2[]
            {
                new Vector2(0, 3), new Vector2(-3, -1.5f), new Vector2(3, -1.5f)
            },
            ropeConnections = new RopeConnection[]
            {
                new RopeConnection { knotIndexA = 0, knotIndexB = 1 },
                new RopeConnection { knotIndexA = 1, knotIndexB = 2 },
                new RopeConnection { knotIndexA = 2, knotIndexB = 0 }
            },
            threeStarMoves = 4,
            twoStarMoves = 6,
            nailPositions = new List<Vector2> { Vector2.zero }
        });

        // Level 4: Hourglass
        levels.Add(new Level
        {
            levelName = "Hourglass",
            knotPositions = new Vector2[]
            {
                new Vector2(-2, 3), new Vector2(2, 3),
                new Vector2(0.5f, 0), new Vector2(-0.5f, 0),
                new Vector2(-2, -3), new Vector2(2, -3)
            },
            ropeConnections = new RopeConnection[]
            {
                new RopeConnection { knotIndexA = 0, knotIndexB = 1 },
                new RopeConnection { knotIndexA = 0, knotIndexB = 3 },
                new RopeConnection { knotIndexA = 1, knotIndexB = 2 },
                new RopeConnection { knotIndexA = 2, knotIndexB = 5 },
                new RopeConnection { knotIndexA = 3, knotIndexB = 4 },
                new RopeConnection { knotIndexA = 4, knotIndexB = 5 }
            },
            threeStarMoves = 10,
            twoStarMoves = 15
        });

        // Level 5: Chain
        levels.Add(new Level
        {
            levelName = "Chain",
            knotPositions = new Vector2[]
            {
                new Vector2(-4, 0), new Vector2(-2, 0), new Vector2(0, 0), new Vector2(2,0), new Vector2(4,0)
            },
            ropeConnections = new RopeConnection[]
            {
                new RopeConnection { knotIndexA = 0, knotIndexB = 1 },
                new RopeConnection { knotIndexA = 1, knotIndexB = 2 },
                new RopeConnection { knotIndexA = 2, knotIndexB = 3 },
                new RopeConnection { knotIndexA = 3, knotIndexB = 4 }
            },
            threeStarMoves = 6,
            twoStarMoves = 10
        });
    }
}
