using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
partial struct MoveAlongPathSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach(var(pathResult, enemyComponent, localTransform) 
            in SystemAPI.Query<DynamicBuffer<PathResult>, RefRO<EnemyComponent>, RefRW<LocalTransform>>())
        {
            if(pathResult.Length > 0)
            {
                float3 target = pathResult[0].position;

                localTransform.ValueRW.Position = math.lerp(localTransform.ValueRO.Position, target, enemyComponent.ValueRO.speed);

                if(math.distance(localTransform.ValueRO.Position, target) < 0.1f) pathResult.RemoveAt(0);
            }
        }
    }
}
