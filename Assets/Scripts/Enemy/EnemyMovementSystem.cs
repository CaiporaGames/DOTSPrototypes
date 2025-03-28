using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(WaypointGenerationSystem))]
partial struct EnemyMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach(var(LocalTransform, enemyComponent, pathResultBuffer) 
            in SystemAPI.Query<RefRW<LocalTransform>, RefRO<EnemyComponent>, DynamicBuffer<PathResult>>())
        {
            for(int i = 0; i < pathResultBuffer.Length; i++)
            {
                LocalTransform.ValueRW.Position = 
                    math.lerp(LocalTransform.ValueRO.Position, pathResultBuffer[i].position, enemyComponent.ValueRO.speed); 
            }
        }
    }
}
