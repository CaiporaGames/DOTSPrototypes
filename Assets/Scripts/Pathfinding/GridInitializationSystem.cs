using Unity.Entities;
using Unity.Collections;

[UpdateBefore(typeof(AStarPathfindingSystem ))]
public partial struct GridInitializationSystem : ISystem
{
    private int gridWidth;
    private int gridHeight;
    public void OnCreate(ref SystemState state)
    {
        gridWidth = 30;
        gridHeight = 30;
        var entity = state.EntityManager.CreateEntity(typeof(GridComponent));

        state.EntityManager.SetComponentData(entity, new GridComponent
        {
            width = gridWidth,
            height = gridHeight,
            isOccupied = new NativeArray<bool>(gridWidth * gridHeight, Allocator.Persistent),
        });

    }
}


