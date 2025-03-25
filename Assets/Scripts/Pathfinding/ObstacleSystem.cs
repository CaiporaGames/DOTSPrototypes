using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
[UpdateAfter(typeof(GridSystem))]  // Ensures it runs after the GridSystem
public partial struct ObstacleSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Get the GridComponent
        foreach (var grid in SystemAPI.Query<RefRW<GridComponent>>())
        {
            // Get the occupied array
            NativeArray<bool> gridOccupied = grid.ValueRW.isOccupied;

            // Iterate through all obstacles and mark them in the grid
            foreach (var obstacle in SystemAPI.Query<RefRO<ObstacleComponent>>())
            {
                int index = obstacle.ValueRO.position.x + obstacle.ValueRO.position.y * grid.ValueRO.width;
                
                if (index >= 0 && index < gridOccupied.Length)
                {
                    gridOccupied[index] = true;
                }
            }
        }
    }
}
