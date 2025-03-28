using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(WaypointConnectionSystem))]
partial struct EnemySpawnerSystem : ISystem
{
    float timer;
    float maxTimer;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        maxTimer = 2;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if(timer <= 0)
        {
            Entity enemyEntity = SystemAPI.GetSingleton<ReferencesComponent>().enemyEntity;
            timer = maxTimer;
            Entity enemyPrefab = state.EntityManager.Instantiate(enemyEntity);
            SystemAPI.SetComponent(enemyPrefab, LocalTransform.FromPosition(float3.zero));
        }

        timer -= SystemAPI.Time.DeltaTime;

    }
}
