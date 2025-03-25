using Unity.Entities;
using UnityEngine;

public class AgentSpawnerAuthoring : MonoBehaviour
{
    public GameObject agentPrefab;
    public GameObject[] targetPrefabs;
    public int maxAgents = 100;
    public float spawnInterval = 1.0f;
    
    public class Baker : Baker<AgentSpawnerAuthoring>
    {
        public override void Bake(AgentSpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            
            // Convert prefabs to entities
            var agentPrefabEntity = GetEntity(authoring.agentPrefab, TransformUsageFlags.Dynamic);
            
            // Create target entities array
            var targets = new DynamicBuffer<SpawnerTargetElement>();
            foreach (var targetPrefab in authoring.targetPrefabs)
            {
                if (targetPrefab != null)
                {
                    var targetEntity = GetEntity(targetPrefab, TransformUsageFlags.Dynamic);
                    targets.Add(new SpawnerTargetElement { TargetEntity = targetEntity });
                }
            }
            
            // Add spawner data
            AddComponent(entity, new AgentSpawnerData
            {
                AgentPrefab = agentPrefabEntity,
                MaxAgents = authoring.maxAgents,
                SpawnInterval = authoring.spawnInterval,
                CurrentAgentCount = 0,
                Timer = 0
            });
            
            // Add target buffer
            AddBuffer<SpawnerTargetElement>(entity);
            
            // Copy target entities to buffer
            var targetBuffer = AddBuffer<SpawnerTargetElement>(entity);
            foreach (var target in targets)
            {
                targetBuffer.Add(target);
            }
            
            // Add tag
            AddComponent<AgentSpawnerTag>(entity);
        }
    }
}

public struct AgentSpawnerTag : IComponentData {}

public struct AgentSpawnerData : IComponentData
{
    public Entity AgentPrefab;
    public int MaxAgents;
    public float SpawnInterval;
    public int CurrentAgentCount;
    public float Timer;
}

public struct SpawnerTargetElement : IBufferElementData
{
    public Entity TargetEntity;
}