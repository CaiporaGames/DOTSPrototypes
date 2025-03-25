using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// System to create agents at runtime
public partial class AgentSpawnerSystem : SystemBase
{
    private Random random;
    private float spawnTimer;
    private const float SpawnInterval = 1.0f; // Spawn every second
    private const int MaxAgents = 1000;       // Cap for demo purposes
    private int agentCount = 0;

    protected override void OnCreate()
    {
        random = Random.CreateFromIndex(1234); // Seed for reproducibility
    }

    protected override void OnUpdate()
    {
        // Spawn agents periodically
        spawnTimer += SystemAPI.Time.DeltaTime;
        
        if (spawnTimer >= SpawnInterval && agentCount < MaxAgents)
        {
            spawnTimer = 0;
            
            // Get ECB for entity creation
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(World.Unmanaged);
            
            // Create a new agent
            var entity = ecb.CreateEntity();
            
            // Random position at the edges
            float3 startPos = new float3(
                random.NextFloat(-45f, 45f), 
                0, 
                random.NextFloat(-45f, 45f));
            
            // Random target position
            float3 targetPos = new float3(
                random.NextFloat(-40f, 40f), 
                0, 
                random.NextFloat(-40f, 40f));
            
            // Add components
            ecb.AddComponent(entity, new PathfindingAgentTag());
            ecb.AddComponent(entity, new PathfindingData
            {
                TargetPosition = targetPos,
                Speed = random.NextFloat(3f, 8f),
                NeedsPath = true
            });
            
            ecb.AddComponent(entity, new LocalTransform
            {
                Position = startPos,
                Rotation = quaternion.identity,
                Scale = 1.0f
            });
            
            agentCount++;
        }
        
        // Handle agent destruction (when they reach targets)
        var ecbDestroy = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(World.Unmanaged);
            
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
                ecbDestroy.DestroyEntity(entity);
                agentCount--;
            }
        }
    }
}