using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct GridComponent : IComponentData
{
    public int width;
    public int height;
    public NativeArray<bool> isOccupied;
}
