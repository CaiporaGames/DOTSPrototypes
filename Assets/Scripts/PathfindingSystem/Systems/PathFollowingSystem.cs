using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// System to move agents along calculated paths
public partial struct PathFollowingSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        // Require components needed for this system
        state.RequireForUpdate<PathfindingAgentTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        
        foreach (var (pathFollow, transform, agent) in 
                 SystemAPI.Query<RefRW<PathFollowData>, RefRW<LocalTransform>, RefRO<PathfindingData>>()
                 .WithAll<PathfindingAgentTag>())
        {
            if (!pathFollow.ValueRO.HasPath)
                continue;
            
            // If we've reached the end of the path, target the final destination
            if (pathFollow.ValueRO.CurrentPathIndex >= pathFollow.ValueRO.Path.Value.Waypoints.Length)
            {
                // Move directly to target
                float3 direction = agent.ValueRO.TargetPosition - transform.ValueRO.Position;
                float distance = math.length(direction);
                
                if (distance > 0.1f)
                {
                    direction = math.normalize(direction);
                    float3 movement = direction * agent.ValueRO.Speed * deltaTime;
                    
                    if (math.length(movement) > distance)
                    {
                        transform.ValueRW.Position = agent.ValueRO.TargetPosition;
                    }
                    else
                    {
                        transform.ValueRW.Position += movement;
                    }
                }
                continue;
            }
            
            // Get current waypoint
            float3 currentWaypoint = pathFollow.ValueRO.Path.Value.Waypoints[pathFollow.ValueRO.CurrentPathIndex];
            
            // Move toward waypoint
            float3 dirToWaypoint = currentWaypoint - transform.ValueRO.Position;
            float distToWaypoint = math.length(dirToWaypoint);
            
            // If close enough to waypoint, move to next one
            if (distToWaypoint < 0.5f)
            {
                pathFollow.ValueRW.CurrentPathIndex++;
            }
            else
            {
                // Move toward current waypoint
                dirToWaypoint = math.normalize(dirToWaypoint);
                float3 movement = dirToWaypoint * agent.ValueRO.Speed * deltaTime;
                
                transform.ValueRW.Position += movement;
                
                // Make entity face movement direction
                if (math.lengthsq(dirToWaypoint) > 0.001f)
                {
                    transform.ValueRW.Rotation = quaternion.LookRotation(dirToWaypoint, new float3(0, 1, 0));
                }
            }
        }
    }
}