using UnityEngine;
using System.Collections.Generic;

public static class LevelGenerator
{
    public static Level GenerateLevel(int knotCount, int extraRopeCount, int nailCount)
    {
        Level newLevel = new Level();
        newLevel.levelName = "Procedural Level";

        // Step 1: Place knots
        newLevel.knotPositions = PlaceKnots(knotCount);

        // Step 2: Place nails
        newLevel.nailPositions = PlaceNails(nailCount, newLevel.knotPositions);

        // Step 3: Connect ropes
        newLevel.ropeConnections = ConnectRopes(knotCount, extraRopeCount);

        // Step 4: Set star thresholds (can be dynamic based on complexity)
        newLevel.threeStarMoves = knotCount * 2;
        newLevel.twoStarMoves = knotCount * 3;

        Debug.Log($"Generated a level with {knotCount} knots.");

        return newLevel;
    }

    private static Vector2[] PlaceKnots(int knotCount)
    {
        List<Vector2> knotPositions = new List<Vector2>();
        Rect spawnArea = new Rect(-4, -4, 8, 8);
        float minDistance = 1.5f;
        int maxRetries = 100;

        for (int i = 0; i < knotCount; i++)
        {
            int retries = 0;
            while (retries < maxRetries)
            {
                Vector2 randomPos = new Vector2(
                    Random.Range(spawnArea.xMin, spawnArea.xMax),
                    Random.Range(spawnArea.yMin, spawnArea.yMax)
                );

                bool isTooClose = false;
                foreach (Vector2 pos in knotPositions)
                {
                    if (Vector2.Distance(pos, randomPos) < minDistance)
                    {
                        isTooClose = true;
                        break;
                    }
                }

                if (!isTooClose)
                {
                    knotPositions.Add(randomPos);
                    break; // Found a valid position, move to the next knot
                }

                retries++;
            }

            if (retries >= maxRetries)
            {
                Debug.LogWarning("Could not find a valid position for a knot after max retries. The level might be too crowded.");
            }
        }

        return knotPositions.ToArray();
    }

    private static RopeConnection[] ConnectRopes(int knotCount, int extraRopeCount)
    {
        if (knotCount < 2) return new RopeConnection[0];

        List<RopeConnection> connections = new List<RopeConnection>();
        HashSet<string> existingConnections = new HashSet<string>();

        // Phase 1: Generate a spanning tree to ensure all knots are connected
        List<int> connectedKnots = new List<int> { 0 };
        List<int> unconnectedKnots = new List<int>();
        for (int i = 1; i < knotCount; i++)
        {
            unconnectedKnots.Add(i);
        }

        while (unconnectedKnots.Count > 0)
        {
            int randomUnconnected = unconnectedKnots[Random.Range(0, unconnectedKnots.Count)];
            int randomConnected = connectedKnots[Random.Range(0, connectedKnots.Count)];

            // Add connection
            connections.Add(new RopeConnection { knotIndexA = randomConnected, knotIndexB = randomUnconnected });
            string connKey = randomConnected < randomUnconnected ? $"{randomConnected}-{randomUnconnected}" : $"{randomUnconnected}-{randomConnected}";
            existingConnections.Add(connKey);

            // Move knot to connected list
            unconnectedKnots.Remove(randomUnconnected);
            connectedKnots.Add(randomUnconnected);
        }

        // Phase 2: Add extra ropes to create crossings
        int maxPossibleConnections = (knotCount * (knotCount - 1)) / 2;
        int potentialExtraConns = maxPossibleConnections - connections.Count;
        int ropesToAdd = Mathf.Min(extraRopeCount, potentialExtraConns);

        for (int i = 0; i < ropesToAdd; i++)
        {
            int knotA = Random.Range(0, knotCount);
            int knotB = Random.Range(0, knotCount);

            if (knotA == knotB) continue; // No self-connections

            // Ensure consistent key format
            string connKey = knotA < knotB ? $"{knotA}-{knotB}" : $"{knotB}-{knotA}";
            if (existingConnections.Contains(connKey))
            {
                // This connection already exists, try again
                i--;
                continue;
            }

            connections.Add(new RopeConnection { knotIndexA = knotA, knotIndexB = knotB });
            existingConnections.Add(connKey);
        }

        return connections.ToArray();
    }

    private static List<Vector2> PlaceNails(int nailCount, Vector2[] knotPositions)
    {
        List<Vector2> nailPositions = new List<Vector2>();
        if (nailCount <= 0) return nailPositions;

        Rect spawnArea = new Rect(-4, -4, 8, 8);
        float minDistance = 1.0f; // Nails can be a bit closer
        int maxRetries = 100;

        for (int i = 0; i < nailCount; i++)
        {
            int retries = 0;
            while (retries < maxRetries)
            {
                Vector2 randomPos = new Vector2(
                    Random.Range(spawnArea.xMin, spawnArea.xMax),
                    Random.Range(spawnArea.yMin, spawnArea.yMax)
                );

                bool isTooClose = false;
                // Check against other nails
                foreach (Vector2 pos in nailPositions)
                {
                    if (Vector2.Distance(pos, randomPos) < minDistance)
                    {
                        isTooClose = true;
                        break;
                    }
                }
                if (isTooClose) { retries++; continue; }

                // Check against knots
                foreach (Vector2 pos in knotPositions)
                {
                    if (Vector2.Distance(pos, randomPos) < minDistance)
                    {
                        isTooClose = true;
                        break;
                    }
                }

                if (!isTooClose)
                {
                    nailPositions.Add(randomPos);
                    break;
                }

                retries++;
            }
             if (retries >= maxRetries)
            {
                Debug.LogWarning("Could not find a valid position for a nail after max retries.");
            }
        }
        return nailPositions;
    }
}
