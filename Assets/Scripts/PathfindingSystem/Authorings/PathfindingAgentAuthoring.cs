using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// Authoring component for pathfinding agents
public class PathfindingAgentAuthoring : MonoBehaviour
{
    public Transform target;
    public float speed = 5.0f;
    
    public class Baker : Baker<PathfindingAgentAuthoring>
    {
        public override void Bake(PathfindingAgentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            // Add tag
            AddComponent<PathfindingAgentTag>(entity);
            
            // Add data
            float3 targetPosition = float3.zero;
            if (authoring.target != null)
            {
                targetPosition = new float3(
                    authoring.target.position.x,
                    authoring.target.position.y,
                    authoring.target.position.z);
            }
            
            AddComponent(entity, new PathfindingData
            {
                TargetPosition = targetPosition,
                Speed = authoring.speed,
                NeedsPath = true
            });
        }
    }
}