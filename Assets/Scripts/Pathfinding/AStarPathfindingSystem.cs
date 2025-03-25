using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct AStarPathfindingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityQuery enemyQuery = SystemAPI.QueryBuilder().WithAll<EnemyComponent, LocalTransform>().Build();
        EntityQuery targetQuery = SystemAPI.QueryBuilder().WithAll<TargetComponent, LocalTransform>().Build();
        EntityQuery gridQuery = SystemAPI.QueryBuilder().WithAll<GridComponent>().Build();
 if (enemyQuery.IsEmpty)
        {
            Debug.Log("Nenhum inimigo encontrado!");
            return;
        }
        if (targetQuery.IsEmpty)
        {
            Debug.Log("Nenhum alvo encontrado!");
            return;
        }
        if (gridQuery.IsEmpty)
        {
            Debug.Log("Nenhum grid encontrado!");
            return;
        }
        if (enemyQuery.IsEmpty || targetQuery.IsEmpty || gridQuery.IsEmpty) return;

        // First, get the specific entity for the enemy and target
        var enemyEntity = SystemAPI.GetSingletonEntity<EnemyComponent>();
        var targetEntity = SystemAPI.GetSingletonEntity<TargetComponent>();

        // Now, retrieve the LocalTransform component for each entity
        var enemyTransform = SystemAPI.GetComponent<LocalTransform>(enemyEntity);
        var targetTransform = SystemAPI.GetComponent<LocalTransform>(targetEntity);
        var gridData = SystemAPI.GetSingleton<GridComponent>();

        int2 start = new int2((int)math.round(enemyTransform.Position.x), (int)math.round(enemyTransform.Position.z));
        int2 goal = new int2((int)math.round(targetTransform.Position.x), (int)math.round(targetTransform.Position.z));

        // Allocate native lists for A* search
        NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
        NativeHashMap<int2, int2> cameFrom = new NativeHashMap<int2, int2>(100, Allocator.Temp);
        NativeHashMap<int2, float> gScore = new NativeHashMap<int2, float>(100, Allocator.Temp);
        NativeHashMap<int2, float> fScore = new NativeHashMap<int2, float>(100, Allocator.Temp);

       

        bool pathFound = AStar(start, goal, gridData.isOccupied, ref path, ref cameFrom, ref gScore, ref fScore);

        if (pathFound && path.Length > 1)
        {
        Debug.Log("A* System Running...");

            int2 nextCell = path[1]; // Move to the next step in the path
            float3 nextPosition = new float3(nextCell.x, enemyTransform.Position.y, nextCell.y);

            float speed = SystemAPI.GetSingleton<EnemyComponent>().speed;
            float3 direction = math.normalize(nextPosition - enemyTransform.Position);
            enemyTransform.Position += direction * speed * SystemAPI.Time.DeltaTime;

            SystemAPI.SetSingleton(enemyTransform);
        }

        // Dispose NativeLists
        path.Dispose();
        cameFrom.Dispose();
        gScore.Dispose();
        fScore.Dispose();
    }

    private bool AStar(int2 start, int2 goal, NativeArray<bool> isOccupied, ref NativeList<int2> path,
                       ref NativeHashMap<int2, int2> cameFrom, ref NativeHashMap<int2, float> gScore, ref NativeHashMap<int2, float> fScore)
    {
        Debug.Log($"A* started: Start = {start}, Goal = {goal}");
        NativeList<int2> openSet = new NativeList<int2>(Allocator.Temp);
        openSet.Add(start);

        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);

        while (openSet.Length > 0)
        {
            // Get node with lowest fScore
            int bestIndex = 0;
            for (int i = 1; i < openSet.Length; i++)
            {
                if (fScore[openSet[i]] < fScore[openSet[bestIndex]])
                    bestIndex = i;
            }
            int2 current = openSet[bestIndex];

            if (current.Equals(goal))
            {
                ReconstructPath(cameFrom, current, ref path);
                openSet.Dispose();
                return true;
            }

            openSet.RemoveAtSwapBack(bestIndex);

            foreach (int2 neighbor in GetNeighbors(current, isOccupied))
            {
                float tentativeGScore = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Heuristic(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
 Debug.Log("No path found!");
        openSet.Dispose();
        return false; // No path found
    }

    private float Heuristic(int2 a, int2 b)
    {
        return math.abs(a.x - b.x) + math.abs(a.y - b.y); // Manhattan distance
    }

    private NativeList<int2> GetNeighbors(int2 cell, NativeArray<bool> isOccupied)
    {
        NativeList<int2> neighbors = new NativeList<int2>(Allocator.Temp);
        int2[] offsets = { new int2(1, 0), new int2(-1, 0), new int2(0, 1), new int2(0, -1) };

        foreach (var offset in offsets)
        {
            int2 neighbor = cell + offset;
            int index = neighbor.x + neighbor.y * 10; // Assuming grid width = 10

            if (index >= 0 && index < isOccupied.Length && !isOccupied[index])
                neighbors.Add(neighbor);
        }

        return neighbors;
    }

    private void ReconstructPath(NativeHashMap<int2, int2> cameFrom, int2 current, ref NativeList<int2> path)
    {
        NativeList<int2> tempPath = new NativeList<int2>(Allocator.Temp); // Temporary storage

        while (cameFrom.ContainsKey(current))
        {
            tempPath.Add(current);
            current = cameFrom[current];
        }
        tempPath.Add(current); // Add the last node

        // Reverse order into the final path
        for (int i = tempPath.Length - 1; i >= 0; i--)
        {
            path.Add(tempPath[i]);
        }

        tempPath.Dispose(); // Free memory
    }

}
