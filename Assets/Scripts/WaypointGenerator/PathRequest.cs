
using Unity.Entities;

public struct PathRequest : IComponentData, IEnableableComponent
{
    public Entity startWaypoint;
    public Entity endWaypoint;
}