using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

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
                int index = (int)(obstacle.ValueRO.position.x + obstacle.ValueRO.position.y * grid.ValueRO.width);
                
                if (index >= 0 && index < gridOccupied.Length)
                {
                    UnityEngine.Debug.Log($"index: {index}, x: {obstacle.ValueRO.position.x}, y: {obstacle.ValueRO.position.y}, width: {grid.ValueRO.width}");
                    gridOccupied[index] = true;
                }
            }
        }
    }
}
