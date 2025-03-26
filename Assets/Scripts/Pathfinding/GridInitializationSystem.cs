using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

[UpdateBefore(typeof(AStarPathfindingSystem ))]
public partial struct GridInitializationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        var entity = state.EntityManager.CreateEntity(typeof(GridComponent));

        state.EntityManager.SetComponentData(entity, new GridComponent
        {
          /*   gridSize = new int2(10, 10), */
            isOccupied = new NativeArray<bool>(100, Allocator.Persistent),
            width = 10
        });

        Debug.Log("GridComponent Entity Created!");
    }
}


