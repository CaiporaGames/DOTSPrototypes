using Unity.Entities;
using UnityEngine;

public class WaypointAuthoring : MonoBehaviour
{
    class Baker : Baker<WaypointAuthoring>
    {
        public override void Bake(WaypointAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

         /*    // Store the prefab entity reference
            AddComponent(entity, new WaypointComponent
            {
                waypointEntity = GetEntity(authoring.waypointPrefab, TransformUsageFlags.Dynamic),
            }); */

            // Ensure the prefab has the Waypoint component
            AddComponent(entity, new Waypoint());
        }
    }
}
