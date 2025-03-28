
using Unity.Entities;
using Unity.Mathematics;

public struct PathRequest : IComponentData, IEnableableComponent
{
    public float3 startPosition;
    public float3 targetPosition;
}