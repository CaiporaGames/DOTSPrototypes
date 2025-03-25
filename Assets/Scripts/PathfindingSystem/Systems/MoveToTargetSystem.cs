using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
public partial struct MoveToTargetSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        // Require both the tag and data
        state.RequireForUpdate<PathfindingAgentTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        
        // Process only entities with the agent tag
        foreach (var (pathData, transform) in 
            SystemAPI.Query<RefRW<PathfindingData>, RefRW<LocalTransform>>().WithAll<PathfindingAgentTag>())
        {
            float3 direction = pathData.ValueRO.TargetPosition - transform.ValueRO.Position;
            float distance = math.length(direction);
            
            if (distance > 0.1f)
            {
                direction = math.normalize(direction);
                float3 movement = direction * pathData.ValueRO.Speed * deltaTime;
                
                if (math.length(movement) > distance)
                {
                    transform.ValueRW.Position = pathData.ValueRO.TargetPosition;
                }
                else
                {
                    transform.ValueRW.Position += movement;
                }
            }
        }
    }
}