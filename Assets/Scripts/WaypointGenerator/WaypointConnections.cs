using Unity.Entities;

[InternalBufferCapacity(8)]//Grid is rectangular so 8 points around the waypoint
public struct WaypointConnections : IBufferElementData
{
    public Entity connectedWaypoint;
}