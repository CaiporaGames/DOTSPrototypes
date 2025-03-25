using Unity.Entities;
using UnityEngine;

public class PathfindingWorldAuthoring : MonoBehaviour
{
    // You can add configuration parameters here if needed
    public int gridSizeX = 100;
    public int gridSizeY = 100;
    public float cellSize = 1.0f;
    
    public class Baker : Baker<PathfindingWorldAuthoring>
    {
        public override void Bake(PathfindingWorldAuthoring authoring)
        {
            // Create a singleton entity for world configuration
            var entity = GetEntity(TransformUsageFlags.None);
            
            // Add a singleton tag to trigger our bootstrap system
            AddComponent<PathfindingWorldTag>(entity);
            
            // You can add configuration data if needed
            AddComponent(entity, new PathfindingWorldConfig
            {
                GridSizeX = authoring.gridSizeX,
                GridSizeY = authoring.gridSizeY,
                CellSize = authoring.cellSize
            });
        }
    }
}

// Tag to identify our world configuration entity
public struct PathfindingWorldTag : IComponentData {}

// Configuration data
public struct PathfindingWorldConfig : IComponentData
{
    public int GridSizeX;
    public int GridSizeY;
    public float CellSize;
}