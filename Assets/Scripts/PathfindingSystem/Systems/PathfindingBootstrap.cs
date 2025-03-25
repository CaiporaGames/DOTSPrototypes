using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct PathfindingBootstrap : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        // This system only runs when we have world configuration
        state.RequireForUpdate<PathfindingWorldTag>();
        state.RequireForUpdate<PathfindingWorldConfig>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Get configuration data
        var config = SystemAPI.GetSingleton<PathfindingWorldConfig>();
        
        // Create a grid entity with our grid component
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        
        // Define grid parameters from config
        int2 gridSize = new int2(config.GridSizeX, config.GridSizeY);
        float cellSize = config.CellSize;
        float3 worldOrigin = new float3(
            -config.GridSizeX * config.CellSize / 2, 
            0, 
            -config.GridSizeY * config.CellSize / 2); // Center grid
        
        // Create a blob builder to store our grid data
        BlobBuilder builder = new BlobBuilder(Allocator.Temp);
        ref GridBlob gridBlobRef = ref builder.ConstructRoot<GridBlob>();
        
        // Allocate space for the grid
        int totalCells = gridSize.x * gridSize.y;
        BlobBuilderArray<bool> walkableArray = builder.Allocate(ref gridBlobRef.Walkable, totalCells);
        
        // Initialize all cells as walkable
        for (int i = 0; i < totalCells; i++)
        {
            walkableArray[i] = true;
        }
        
        // Create the blob asset
        var gridBlobAsset = builder.CreateBlobAssetReference<GridBlob>(Allocator.Persistent);
        builder.Dispose();
        
        // Create the entity that will store our grid
        var gridEntity = ecb.CreateEntity();
        ecb.AddComponent(gridEntity, new PathfindingGrid
        {
            GridSize = gridSize,
            CellSize = cellSize,
            WorldOrigin = worldOrigin,
            GridData = gridBlobAsset
        });
        
        // Play the command buffer
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        
        // Disable this system after initialization
        state.Enabled = false;
    }

    public void OnDestroy(ref SystemState state)
    {
        // No cleanup needed here
    }
}