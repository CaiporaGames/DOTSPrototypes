using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(ObstacleSystem))]
public partial struct AStarPathfindingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Query for required components
        EntityQuery enemyQuery = SystemAPI.QueryBuilder().WithAll<EnemyComponent, LocalTransform>().Build();
        EntityQuery targetQuery = SystemAPI.QueryBuilder().WithAll<TargetComponent, LocalTransform>().Build();
        EntityQuery gridQuery = SystemAPI.QueryBuilder().WithAll<GridComponent>().Build();
        
        // Early exit if any required component is missing
        if (enemyQuery.IsEmpty || targetQuery.IsEmpty || gridQuery.IsEmpty) 
        {
            return;
        }

        // Get specific entities
        var enemyEntity = SystemAPI.GetSingletonEntity<EnemyComponent>();
        var targetEntity = SystemAPI.GetSingletonEntity<TargetComponent>();

        // Retrieve components
        var enemyTransform = SystemAPI.GetComponent<LocalTransform>(enemyEntity);
        var targetTransform = SystemAPI.GetComponent<LocalTransform>(targetEntity);
        var gridData = SystemAPI.GetSingleton<GridComponent>();

        // Convert world positions to grid coordinates
        int2 start = new int2((int)math.round(enemyTransform.Position.x), (int)math.round(enemyTransform.Position.z));
        int2 goal = new int2((int)math.round(targetTransform.Position.x), (int)math.round(targetTransform.Position.z));

        // Make sure grid dimensions are set
        int gridWidth = gridData.width > 0 ? gridData.width : 10;
        int gridHeight = gridData.height > 0 ? gridData.height : 10;

        // Check if positions are within grid bounds
        if (!IsInBounds(start, gridWidth, gridHeight) || !IsInBounds(goal, gridWidth, gridHeight))
        {
            Debug.LogWarning($"A* pathfinding: Start or goal is out of bounds. Start: {start}, Goal: {goal}, width:{gridWidth}, height:{gridHeight}");
            return;
        }

        // Allocate native collections for A* algorithm
        NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
        NativeHashMap<int2, int2> cameFrom = new NativeHashMap<int2, int2>(gridWidth * gridHeight, Allocator.Temp);
        NativeHashMap<int2, float> gScore = new NativeHashMap<int2, float>(gridWidth * gridHeight, Allocator.Temp);
        NativeHashMap<int2, float> fScore = new NativeHashMap<int2, float>(gridWidth * gridHeight, Allocator.Temp);

        // Find path using A* algorithm
        bool pathFound = AStar(start, goal, gridData.isOccupied, gridWidth, gridHeight, ref path, 
                              ref cameFrom, ref gScore, ref fScore);

        // If path found, move enemy toward next waypoint
        if (pathFound && path.Length > 1)
        {
            Debug.Log($"Path found with {path.Length} steps.");

            // Get next position in path
            int2 nextCell = path[1]; // Move to the next step in the path
            float3 nextPosition = new float3(nextCell.x, enemyTransform.Position.y, nextCell.y);

            // Move enemy toward next position
            float speed = SystemAPI.GetComponent<EnemyComponent>(enemyEntity).speed;
            float3 direction = math.normalize(nextPosition - enemyTransform.Position);
            
            // Ensure direction is valid before moving
            if (!math.any(math.isnan(direction)))
            {
                enemyTransform.Position += direction * speed * SystemAPI.Time.DeltaTime;
                SystemAPI.SetComponent(enemyEntity, enemyTransform);
            }
        }
        else if (!pathFound)
        {
            Debug.LogWarning("No path found to target.");
        }

        // Dispose NativeCollections to prevent memory leaks
        path.Dispose();
        cameFrom.Dispose();
        gScore.Dispose();
        fScore.Dispose();
    }

    private bool IsInBounds(int2 pos, int width, int height)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    private bool AStar(int2 start, int2 goal, NativeArray<bool> isOccupied, int gridWidth, int gridHeight,
                      ref NativeList<int2> path, ref NativeHashMap<int2, int2> cameFrom, 
                      ref NativeHashMap<int2, float> gScore, ref NativeHashMap<int2, float> fScore)
    {
        // Create open set for nodes to be evaluated
        NativeMinHeap<int2, float> openSet = new NativeMinHeap<int2, float>(gridWidth * gridHeight, Allocator.Temp);
        
        // Add start node to open set
        openSet.Insert(start, 0);

        // Initialize scores for start node
        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);

        // Limit iterations to prevent infinite loops
        int iterationLimit = gridWidth * gridHeight * 4; // Reasonable upper limit
        int iterationCount = 0;

        while (!openSet.IsEmpty)
        {
            // Safety check to prevent infinite loops
            if (++iterationCount > iterationLimit)
            {
                Debug.LogError($"A* aborted: Too many iterations! Limit: {iterationLimit}");
                openSet.Dispose();
                return false;
            }

            // Get node with lowest fScore from priority queue
            int2 current = openSet.ExtractMin();

            // Check if we've reached the goal
            if (current.Equals(goal))
            {
                ReconstructPath(cameFrom, current, ref path);
                openSet.Dispose();
                return true;
            }

            // Process each neighbor
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    // Skip diagonals and self
                    if ((dx == 0 && dy == 0) || (dx != 0 && dy != 0))
                        continue;

                    int2 neighbor = new int2(current.x + dx, current.y + dy);

                    // Check if neighbor is within bounds
                    if (!IsInBounds(neighbor, gridWidth, gridHeight))
                        continue;

                    // Check if neighbor is traversable (not occupied)
                    int index = neighbor.x + neighbor.y * gridWidth;
                    if (index < 0 || index >= isOccupied.Length || isOccupied[index])
                        continue;

                    // Calculate tentative g score
                    float tentativeGScore = gScore[current] + 1.0f;

                    // If this path is better than previous ones, record it
                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        float newFScore = tentativeGScore + Heuristic(neighbor, goal);
                        fScore[neighbor] = newFScore;

                        openSet.Insert(neighbor, newFScore);
                    }
                }
            }
        }

        // No path found
        Debug.LogWarning("A* pathfinding: No path found!");
        openSet.Dispose();
        return false;
    }

    private float Heuristic(int2 a, int2 b)
    {
        return math.abs(a.x - b.x) + math.abs(a.y - b.y); // Manhattan distance
    }

    private void ReconstructPath(NativeHashMap<int2, int2> cameFrom, int2 current, ref NativeList<int2> path)
    {
        // Create temporary path in reverse order
        NativeList<int2> tempPath = new NativeList<int2>(Allocator.Temp);
        
        // Add current node
        tempPath.Add(current);
        
        // Follow path backwards from goal to start
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            tempPath.Add(current);
        }

        // Reverse the path to get start-to-goal order
        for (int i = tempPath.Length - 1; i >= 0; i--)
        {
            path.Add(tempPath[i]);
        }
        
        tempPath.Dispose();
    }
}

// A minimal priority queue implementation to improve A* performance
public struct NativeMinHeap<TItem, TPriority> : System.IDisposable where TItem : unmanaged, System.IEquatable<TItem> where TPriority : unmanaged, System.IComparable<TPriority>
{
    private struct HeapNode
    {
        public TItem Item;
        public TPriority Priority;
    }

    private NativeList<HeapNode> _heap;
    private NativeHashMap<TItem, int> _indices;

    public NativeMinHeap(int capacity, Allocator allocator)
    {
        _heap = new NativeList<HeapNode>(capacity, allocator);
        _indices = new NativeHashMap<TItem, int>(capacity, allocator);
    }

    public bool IsEmpty => _heap.Length == 0;

    public void Insert(TItem item, TPriority priority)
    {
        if (_indices.ContainsKey(item))
        {
            // Update priority if item exists
            int index = _indices[item];
            if (priority.CompareTo(_heap[index].Priority) < 0)
            {
                _heap[index] = new HeapNode { Item = item, Priority = priority };
                SiftUp(index);
            }
            return;
        }

        // Add new item
        _heap.Add(new HeapNode { Item = item, Priority = priority });
        int newIndex = _heap.Length - 1;
        _indices[item] = newIndex;
        SiftUp(newIndex);
    }

    public TItem ExtractMin()
    {
        if (_heap.Length == 0)
            throw new System.InvalidOperationException("Heap is empty");

        TItem min = _heap[0].Item;
        _indices.Remove(min);

        if (_heap.Length > 1)
        {
            _heap[0] = _heap[_heap.Length - 1];
            _indices[_heap[0].Item] = 0;
            _heap.RemoveAt(_heap.Length - 1);
            SiftDown(0);
        }
        else
        {
            _heap.Clear();
        }

        return min;
    }

    private void SiftUp(int index)
    {
        int parent = (index - 1) / 2;
        while (index > 0 && _heap[index].Priority.CompareTo(_heap[parent].Priority) < 0)
        {
            Swap(index, parent);
            index = parent;
            parent = (index - 1) / 2;
        }
    }

    private void SiftDown(int index)
    {
        int minIndex = index;
        int leftChild = 2 * index + 1;
        int rightChild = 2 * index + 2;

        if (leftChild < _heap.Length && _heap[leftChild].Priority.CompareTo(_heap[minIndex].Priority) < 0)
            minIndex = leftChild;

        if (rightChild < _heap.Length && _heap[rightChild].Priority.CompareTo(_heap[minIndex].Priority) < 0)
            minIndex = rightChild;

        if (index != minIndex)
        {
            Swap(index, minIndex);
            SiftDown(minIndex);
        }
    }

    private void Swap(int i, int j)
    {
        HeapNode temp = _heap[i];
        _heap[i] = _heap[j];
        _heap[j] = temp;

        _indices[_heap[i].Item] = i;
        _indices[_heap[j].Item] = j;
    }

    public void Dispose()
    {
        if (_heap.IsCreated) _heap.Dispose();
        if (_indices.IsCreated) _indices.Dispose();
    }
}