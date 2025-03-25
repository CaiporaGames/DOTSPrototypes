using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;

// Component to store path data for an agent
public struct PathFollowData : IComponentData
{
    // Store the current path
    public BlobAssetReference<PathBlob> Path;
    public int CurrentPathIndex;
    public bool HasPath;
}

// Blob to store path data
public struct PathBlob
{
    public BlobArray<float3> Waypoints;
}

// System to calculate paths for agents
public partial struct PathfindingSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PathfindingGrid>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var grid = SystemAPI.GetSingleton<PathfindingGrid>();
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        
        // Process all agents that need a path
        foreach (var (pathData, transform, entity) in 
                 SystemAPI.Query<RefRO<PathfindingData>, RefRO<LocalTransform>>()
                 .WithAll<PathfindingAgentTag>()
                 .WithEntityAccess())
        {
            if (pathData.ValueRO.NeedsPath)
            {
                // Convert start and target positions to grid coordinates
                int2 startPos = WorldToGrid(transform.ValueRO.Position, grid.WorldOrigin, grid.CellSize);
                int2 targetPos = WorldToGrid(pathData.ValueRO.TargetPosition, grid.WorldOrigin, grid.CellSize);
                
                // Find path using A*
                NativeList<int2> path = FindPath(startPos, targetPos, grid, Allocator.Temp);
                
                if (path.Length > 0)
                {
                    // Convert the path to world positions and store in a blob
                    BlobBuilder builder = new BlobBuilder(Allocator.Temp);
                    ref PathBlob pathBlobRef = ref builder.ConstructRoot<PathBlob>();
                    
                    BlobBuilderArray<float3> waypointArray = 
                        builder.Allocate(ref pathBlobRef.Waypoints, path.Length);
                    
                    for (int i = 0; i < path.Length; i++)
                    {
                        waypointArray[i] = GridToWorld(path[i], grid.WorldOrigin, grid.CellSize);
                    }
                    
                    // Create the blob asset
                    var pathBlobAsset = builder.CreateBlobAssetReference<PathBlob>(Allocator.Persistent);
                    builder.Dispose();
                    
                    // Add or update path component
                    if (SystemAPI.HasComponent<PathFollowData>(entity))
                    {
                        // Dispose old path if exists
                        var oldPathData = SystemAPI.GetComponent<PathFollowData>(entity);
                        if (oldPathData.HasPath)
                        {
                            oldPathData.Path.Dispose();
                        }
                        
                        // Update with new path
                        ecb.SetComponent(entity, new PathFollowData
                        {
                            Path = pathBlobAsset,
                            CurrentPathIndex = 0,
                            HasPath = true
                        });
                    }
                    else
                    {
                        // Add path component
                        ecb.AddComponent(entity, new PathFollowData
                        {
                            Path = pathBlobAsset,
                            CurrentPathIndex = 0,
                            HasPath = true
                        });
                    }
                }
                
                path.Dispose();
                
                // Mark as no longer needing a path
                var updatedPathData = pathData.ValueRO;
                updatedPathData.NeedsPath = false;
                ecb.SetComponent(entity, updatedPathData);
            }
        }
    }
    
    // A* pathfinding algorithm
    private NativeList<int2> FindPath(int2 startPos, int2 targetPos, PathfindingGrid grid, Allocator allocator)
    {
        NativeList<int2> path = new NativeList<int2>(allocator);
        
        // If start or target is out of bounds or not walkable, return empty path
        if (!IsPositionValid(startPos, grid) || !IsPositionValid(targetPos, grid))
        {
            return path;
        }
        
        // Create nodes for A*
        int totalNodes = grid.GridSize.x * grid.GridSize.y;
        NativeArray<PathNode> nodes = new NativeArray<PathNode>(totalNodes, Allocator.Temp);
        
        // Initialize nodes
        for (int x = 0; x < grid.GridSize.x; x++)
        {
            for (int y = 0; y < grid.GridSize.y; y++)
            {
                int index = grid.GridData.Value.GetIndex(x, y, grid.GridSize.x);
                nodes[index] = new PathNode
                {
                    Position = new int2(x, y),
                    GCost = int.MaxValue,
                    HCost = CalculateHCost(new int2(x, y), targetPos),
                    ParentIndex = -1,
                    IsWalkable = grid.GridData.Value.Walkable[index],
                    IsProcessed = false
                };
            }
        }
        
        // Initialize start node
        int startIndex = grid.GridData.Value.GetIndex(startPos.x, startPos.y, grid.GridSize.x);
        nodes[startIndex] = new PathNode
        {
            Position = startPos,
            GCost = 0,
            HCost = CalculateHCost(startPos, targetPos),
            ParentIndex = -1,
            IsWalkable = nodes[startIndex].IsWalkable,
            IsProcessed = false
        };
        
        // Open set priority queue
        NativeList<int> openSet = new NativeList<int>(Allocator.Temp);
        openSet.Add(startIndex);
        
        // A* main loop
        bool foundPath = false;
        
        while (openSet.Length > 0)
        {
            // Get node with lowest F cost
            int currentIndex = openSet[0];
            for (int i = 1; i < openSet.Length; i++)
            {
                if (nodes[openSet[i]].FCost < nodes[currentIndex].FCost || 
                    nodes[openSet[i]].FCost == nodes[currentIndex].FCost && 
                    nodes[openSet[i]].HCost < nodes[currentIndex].HCost)
                {
                    currentIndex = openSet[i];
                }
            }
            
            // Remove current from open set
            for (int i = 0; i < openSet.Length; i++)
            {
                if (openSet[i] == currentIndex)
                {
                    openSet.RemoveAtSwapBack(i);
                    break;
                }
            }
            
            // Mark as processed
            PathNode currentNode = nodes[currentIndex];
            currentNode.IsProcessed = true;
            nodes[currentIndex] = currentNode;
            
            // Check if we reached the target
            if (currentNode.Position.Equals(targetPos))
            {
                foundPath = true;
                break;
            }
            
            // Process neighbors
            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                for (int yOffset = -1; yOffset <= 1; yOffset++)
                {
                    // Skip self
                    if (xOffset == 0 && yOffset == 0)
                        continue;
                    
                    // Skip diagonals (optional, can remove this if you want diagonal movement)
                    if (math.abs(xOffset) == 1 && math.abs(yOffset) == 1)
                        continue;
                    
                    int2 neighborPos = new int2(
                        currentNode.Position.x + xOffset,
                        currentNode.Position.y + yOffset
                    );
                    
                    // Skip if outside grid
                    if (neighborPos.x < 0 || neighborPos.x >= grid.GridSize.x ||
                        neighborPos.y < 0 || neighborPos.y >= grid.GridSize.y)
                        continue;
                    
                    int neighborIndex = grid.GridData.Value.GetIndex(
                        neighborPos.x, neighborPos.y, grid.GridSize.x);
                    
                    // Skip if processed or not walkable
                    if (nodes[neighborIndex].IsProcessed || !nodes[neighborIndex].IsWalkable)
                        continue;
                    
                    // Calculate new G cost (10 for cardinal, 14 for diagonal moves)
                    int moveCost = (math.abs(xOffset) + math.abs(yOffset) == 1) ? 10 : 14;
                    int newGCost = currentNode.GCost + moveCost;
                    
                    // If new path to neighbor is shorter or neighbor is not in open set
                    if (newGCost < nodes[neighborIndex].GCost)
                    {
                        // Update neighbor
                        PathNode neighborNode = nodes[neighborIndex];
                        neighborNode.GCost = newGCost;
                        neighborNode.ParentIndex = currentIndex;
                        nodes[neighborIndex] = neighborNode;
                        
                        // Add to open set if not there
                        bool inOpenSet = false;
                        for (int i = 0; i < openSet.Length; i++)
                        {
                            if (openSet[i] == neighborIndex)
                            {
                                inOpenSet = true;
                                break;
                            }
                        }
                        
                        if (!inOpenSet)
                        {
                            openSet.Add(neighborIndex);
                        }
                    }
                }
            }
        }
        
        // Reconstruct path if found
        if (foundPath)
        {
            // Start from target and work backwards
            int currentIdx = grid.GridData.Value.GetIndex(targetPos.x, targetPos.y, grid.GridSize.x);
            
            // Add target position first (will be reversed later)
            path.Add(targetPos);
            
            // Work backwards through parents
            while (nodes[currentIdx].ParentIndex != -1)
            {
                currentIdx = nodes[currentIdx].ParentIndex;
                path.Add(nodes[currentIdx].Position);
            }
            
            // Reverse path to get start-to-finish order
            for (int i = 0; i < path.Length / 2; i++)
            {
                int2 temp = path[i];
                path[i] = path[path.Length - 1 - i];
                path[path.Length - 1 - i] = temp;
            }
        }
        
        // Clean up
        nodes.Dispose();
        openSet.Dispose();
        
        return path;
    }
    
    // Calculate heuristic cost (Manhattan distance)
    private int CalculateHCost(int2 from, int2 to)
    {
        int xDist = math.abs(from.x - to.x);
        int yDist = math.abs(from.y - to.y);
        return (xDist + yDist) * 10; // Multiply by 10 to match G cost scale
    }
    
    // Check if position is valid (within grid and walkable)
    private bool IsPositionValid(int2 pos, PathfindingGrid grid)
    {
        if (pos.x < 0 || pos.x >= grid.GridSize.x || pos.y < 0 || pos.y >= grid.GridSize.y)
            return false;
        
        int index = grid.GridData.Value.GetIndex(pos.x, pos.y, grid.GridSize.x);
        return grid.GridData.Value.Walkable[index];
    }
    
    // Helper to convert world position to grid coordinates
    private int2 WorldToGrid(float3 worldPos, float3 origin, float cellSize)
    {
        float3 localPos = worldPos - origin;
        return new int2(
            (int)math.floor(localPos.x / cellSize),
            (int)math.floor(localPos.z / cellSize)
        );
    }
    
    // Helper to convert grid coordinates to world position
    private float3 GridToWorld(int2 gridPos, float3 origin, float cellSize)
    {
        return new float3(
            gridPos.x * cellSize + cellSize/2 + origin.x,
            0,
            gridPos.y * cellSize + cellSize/2 + origin.z
        );
    }
}