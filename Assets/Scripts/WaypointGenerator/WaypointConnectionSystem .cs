

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
[UpdateAfter(typeof(WaypointGenerationSystem))]
public partial struct WaypointConnectionSystem : ISystem
{
   public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        // NativeHashMap for quick lookup by position
        NativeHashMap<int2, Entity> waypointMap = new NativeHashMap<int2, Entity>(1024, Allocator.Temp);

        // First pass: Store waypoints in a hashmap using grid coordinates
        foreach (var (waypoint, entity) in SystemAPI.Query<Waypoint>().WithEntityAccess())
        {
            int2 gridPos = new int2((int)math.round(waypoint.position.x), (int)math.round(waypoint.position.z));
            waypointMap.TryAdd(gridPos, entity);
        }

        // Second pass: Connect waypoints
        foreach (var (waypoint, entity) in SystemAPI.Query<Waypoint>().WithEntityAccess())
        {
            int2 gridPos = new int2((int)math.round(waypoint.position.x), (int)math.round(waypoint.position.z));
            var buffer = ecb.AddBuffer<WaypointConnections>(entity);

            // Define possible neighbor offsets
            int2[] neighborOffsets = new int2[]
            {
                new int2(1, 0), new int2(-1, 0), // Left/Right
                new int2(0, 1), new int2(0, -1), // Up/Down
                new int2(1, 1), new int2(-1, -1), // Diagonal (optional)
                new int2(1, -1), new int2(-1, 1)
            };

            foreach (int2 offset in neighborOffsets)
            {
                if (waypointMap.TryGetValue(gridPos + offset, out Entity neighbor))
                {
                    buffer.Add(new WaypointConnections { connectedWaypoint = neighbor });
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        waypointMap.Dispose();
    }
}