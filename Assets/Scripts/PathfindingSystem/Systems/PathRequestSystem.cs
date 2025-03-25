using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// System to check for agents that need path recalculation
public partial struct PathRequestSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PathfindingAgentTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        
        foreach (var (pathData, transform, entity) in 
                 SystemAPI.Query<RefRO<PathfindingData>, RefRO<LocalTransform>>()
                 .WithAll<PathfindingAgentTag>()
                 .WithEntityAccess())
        {
            // Check if we need to request a new path
            bool needsPath = false;
            
            // If no path exists yet, definitely need one
            if (!SystemAPI.HasComponent<PathFollowData>(entity))
            {
                needsPath = true;
            }
            else
            {
                var pathFollow = SystemAPI.GetComponent<PathFollowData>(entity);
                
                // If we have no path or are at the end of our path, we need a new one
                if (!pathFollow.HasPath || 
                    pathFollow.CurrentPathIndex >= pathFollow.Path.Value.Waypoints.Length)
                {
                    needsPath = true;
                }
                
                // Periodically recalculate path to adapt to changing environment
                // (You could add additional logic here, like a timer)
            }
            
            // If we need a path, set the flag
            if (needsPath && !pathData.ValueRO.NeedsPath)
            {
                var updatedPathData = pathData.ValueRO;
                updatedPathData.NeedsPath = true;
                ecb.SetComponent(entity, updatedPathData);
            }
        }
    }
}