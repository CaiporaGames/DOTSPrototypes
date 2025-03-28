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
            //Reset timer
            timer = maxTimer;

            //Get all necessaries references
            Entity referencesEntity = SystemAPI.GetSingletonEntity<ReferencesComponent>();
            
            Entity enemyEntity = SystemAPI.GetComponent<ReferencesComponent>(referencesEntity).enemyEntity;

            Entity enemyPrefab = state.EntityManager.Instantiate(enemyEntity);
            var startWaypointEntity = SystemAPI.GetComponent<ReferencesComponent>(referencesEntity).enemySpawnPointEntity;
            var spawnPosition = SystemAPI.GetComponent<LocalTransform>(startWaypointEntity);
            
            var startPosition = SystemAPI.GetComponent<LocalToWorld>(startWaypointEntity);
            SystemAPI.SetComponent(enemyPrefab, spawnPosition);

            var targetEntity = SystemAPI.GetSingletonEntity<TargetComponent>();
            var targetPosition = SystemAPI.GetComponent<LocalToWorld>(targetEntity);

            //Setup path request
            SystemAPI.SetComponent(enemyPrefab, new PathRequest{
                startPosition = startPosition.Position,
                targetPosition = targetPosition.Position
            });

        }

        timer -= SystemAPI.Time.DeltaTime;

    }
}
