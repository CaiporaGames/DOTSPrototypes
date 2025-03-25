using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(PathFollowingSystem))]
public partial struct AgentCleanupSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PathfindingAgentTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        
        // Find all spawners to decrement their count when agents are destroyed
        var spawnerQuery = SystemAPI.QueryBuilder().WithAll<AgentSpawnerData, AgentSpawnerTag>().Build();
        var spawnerEntities = spawnerQuery.ToEntityArray(Allocator.Temp);
        
        // Check agents that have reached their destination
        foreach (var (pathData, transform, entity) in 
                 SystemAPI.Query<RefRO<PathfindingData>, RefRO<LocalTransform>>()
                 .WithAll<PathfindingAgentTag>()
                 .WithEntityAccess())
        {
            // If agent reached destination, destroy it
            float3 direction = pathData.ValueRO.TargetPosition - transform.ValueRO.Position;
            float distance = math.length(direction);
            
            if (distance < 0.5f)
            {
                ecb.DestroyEntity(entity);
                
                // Decrement agent count in all spawners
                foreach (var spawnerEntity in spawnerEntities)
                {
                    if (SystemAPI.HasComponent<AgentSpawnerData>(spawnerEntity))
                    {
                        var spawnerData = SystemAPI.GetComponent<AgentSpawnerData>(spawnerEntity);
                        spawnerData.CurrentAgentCount = math.max(0, spawnerData.CurrentAgentCount - 1);
                        ecb.SetComponent(spawnerEntity, spawnerData);
                    }
                }
            }
        }
        
        spawnerEntities.Dispose();
    }
}