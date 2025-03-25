using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

// Singleton component to store our grid data
public struct PathfindingGrid : IComponentData
{
    // Grid dimensions
    public int2 GridSize;
    public float CellSize;
    
    // Origin of the grid in world space
    public float3 WorldOrigin;
    
    // Reference to our grid data (stored in a blob asset)
    public BlobAssetReference<GridBlob> GridData;
}

// Blob to efficiently store our grid data
public struct GridBlob
{
    // Store whether each cell is walkable
    public BlobArray<bool> Walkable;
    
    // Get index into the 1D array from 2D coordinates
    public int GetIndex(int x, int z, int width)
    {
        return z * width + x;
    }
}

// Node for A* pathfinding
public struct PathNode
{
    public int2 Position;
    public int GCost; // Cost from start
    public int HCost; // Estimated cost to end
    public int FCost => GCost + HCost; // Total cost
    public int ParentIndex; // For reconstructing path
    public bool IsWalkable;
    public bool IsProcessed;
}