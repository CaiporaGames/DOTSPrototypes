using Unity.Entities;
using UnityEngine;

// This component signals that we want to create our pathfinding systems
public class PathfindingSystemsAuthoring : MonoBehaviour
{
    public class Baker : Baker<PathfindingSystemsAuthoring>
    {
        public override void Bake(PathfindingSystemsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent<PathfindingSystemsTag>(entity);
        }
    }
}

// Tag to trigger system creation
public struct PathfindingSystemsTag : IComponentData {}