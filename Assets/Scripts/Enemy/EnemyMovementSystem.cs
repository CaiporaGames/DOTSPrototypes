using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

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
        /* var target = SystemAPI.GetSingletonEntity<TargetComponent>();
        var targetPosition = SystemAPI.GetComponent<LocalTransform>(target);
        foreach(var(enemyComponent, localTransform) in SystemAPI.Query<RefRO<EnemyComponent>, RefRW<LocalTransform>>())
        {
            localTransform.ValueRW.Position.z += enemyComponent.ValueRO.speed * SystemAPI.Time.DeltaTime;
        } */
    }
}
