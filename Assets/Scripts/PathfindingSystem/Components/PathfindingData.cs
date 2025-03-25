using Unity.Entities;
using Unity.Mathematics;

// Component for entities that need pathfinding
public struct PathfindingData : IComponentData
{
    // Current destination
    public float3 TargetPosition;
    
    // Movement speed
    public float Speed;
    
    // Whether a new path needs to be calculated
    public bool NeedsPath;
}