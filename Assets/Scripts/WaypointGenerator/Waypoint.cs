using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct Waypoint : IComponentData
{
    public float3 position;//waipoint position in the world
    public FixedList128Bytes<Entity> Neighbors;
}
