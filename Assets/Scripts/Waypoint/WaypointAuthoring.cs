using Unity.Entities;
using UnityEngine;

public class WaypointAuthoring : MonoBehaviour
{
    class Baker : Baker<WaypointAuthoring>
    {
        public override void Bake(WaypointAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Waypoint());
            AddComponent(entity, new PathRequest());
            AddBuffer<PathResult>(entity);
        }
    }
}
