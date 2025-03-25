using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;

// System to update the grid when obstacles change
public partial struct GridUpdateSystem : ISystem
{
    private EntityQuery obstacleQuery;

    public void OnCreate(ref SystemState state)
    {
        obstacleQuery = state.GetEntityQuery(ComponentType.ReadOnly<ObstacleTag>(), 
                                           ComponentType.ReadOnly<LocalTransform>());
        state.RequireForUpdate<PathfindingGrid>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Get the grid singleton
        var grid = SystemAPI.GetSingleton<PathfindingGrid>();
        int totalCells = grid.GridSize.x * grid.GridSize.y;
        
        // First, reset the grid (all walkable)
        for (int i = 0; i < totalCells; i++)
        {
            grid.GridData.Value.Walkable[i] = true;
        }
        
        // Then mark cells with obstacles as not walkable
        NativeArray<LocalTransform> obstacleTransforms = obstacleQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        
        foreach (var transform in obstacleTransforms)
        {
            // Convert world position to grid position
            int2 gridPos = WorldToGrid(transform.Position, grid.WorldOrigin, grid.CellSize);
            
            // Make sure position is within grid
            if (gridPos.x >= 0 && gridPos.x < grid.GridSize.x && 
                gridPos.y >= 0 && gridPos.y < grid.GridSize.y)
            {
                // Mark as obstacle (not walkable)
                int index = grid.GridData.Value.GetIndex(gridPos.x, gridPos.y, grid.GridSize.x);
                grid.GridData.Value.Walkable[index] = false;
                
                // Also mark cells surrounding the obstacle as not walkable
                // This creates a buffer around obstacles
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    for (int yOffset = -1; yOffset <= 1; yOffset++)
                    {
                        int2 neighborPos = new int2(gridPos.x + xOffset, gridPos.y + yOffset);
                        
                        if (neighborPos.x >= 0 && neighborPos.x < grid.GridSize.x && 
                            neighborPos.y >= 0 && neighborPos.y < grid.GridSize.y)
                        {
                            int neighborIndex = grid.GridData.Value.GetIndex(
                                neighborPos.x, neighborPos.y, grid.GridSize.x);
                            grid.GridData.Value.Walkable[neighborIndex] = false;
                        }
                    }
                }
            }
        }
        
        // Update the singleton with our modified grid
        SystemAPI.SetSingleton(grid);
        
        obstacleTransforms.Dispose();
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
}