using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

[UpdateAfter(typeof(GridInitializationSystem))]
public partial struct GridSystem : ISystem
{
    private NativeArray<bool> gridOccupied;  // Temporary array for grid states
    private int gridWidth;
    private int gridHeight;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GridComponent>();
        var gridEntity = SystemAPI.GetSingletonEntity<GridComponent>();
        gridWidth = SystemAPI.GetComponent<GridComponent>(gridEntity).width;
        gridHeight = SystemAPI.GetComponent<GridComponent>(gridEntity).height;
        // Initialize grid
        gridOccupied = new NativeArray<bool>(gridWidth * gridHeight, Allocator.Persistent);  // Example grid size (100x100)
    }

    public void OnUpdate(ref SystemState state)
    {
        // You can make this dynamically change based on game logic
        DrawGrid(gridWidth, gridHeight);  // Example size, 10x10 grid

        // Update grid system if necessary (e.g., for obstacles)
        // You can mark certain positions as occupied here.
    }

    // Function to draw grid with Debug.DrawLine
    private void DrawGrid(int width, int height)
    {
        float gridSize = 1f;  // Size of each grid cell in world space

        // Loop over the grid and draw horizontal and vertical lines
        for (int x = 0; x <= width; x++)
        {
            // Draw vertical lines
            Vector3 startPos = new Vector3(x * gridSize, 0, 0);
            Vector3 endPos = new Vector3(x * gridSize, 0, height * gridSize);
            Debug.DrawLine(startPos, endPos, Color.green);
        }

        for (int z = 0; z <= height; z++)
        {
            // Draw horizontal lines
            Vector3 startPos = new Vector3(0, 0, z * gridSize);
            Vector3 endPos = new Vector3(width * gridSize, 0, z * gridSize);
            Debug.DrawLine(startPos, endPos, Color.green);
        }
    }

    public void OnDestroy(ref SystemState state)
    {
        // Cleanup resources
        if (gridOccupied.IsCreated)
            gridOccupied.Dispose();
    }
}
