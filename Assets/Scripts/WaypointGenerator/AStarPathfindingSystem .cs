
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;

[UpdateAfter(typeof(WaypointGenerationSystem))]
public partial struct AStarPathfindingSystem : ISystem
{
    ComponentLookup<Waypoint> waypointLookup;
    public void OnCreate(ref SystemState state)
    {
        waypointLookup = state.GetComponentLookup<Waypoint>(true);
    }

    public void OnUpdate(ref SystemState state)
    {
        waypointLookup.Update(ref state);
        
        //query enabled path request entities
        foreach(var(pathRequest, entity) in SystemAPI.Query<RefRO<PathRequest>>().WithEntityAccess())
        {
            //Find the nearest waypoits to start and target
            Entity startWaypoint = FindNearestWaypoint(ref state, pathRequest.ValueRO.startPosition);
            Entity targetWaypoint = FindNearestWaypoint(ref state, pathRequest.ValueRO.targetPosition);

            if(!waypointLookup.HasComponent(startWaypoint) || !waypointLookup.HasComponent(targetWaypoint))
            {
                UnityEngine.Debug.LogError("Invalid waypoints in A* algorithm.");
                return;
            }

            var pathBuffer = state.EntityManager.GetBuffer<PathResult>(entity);
            UnityEngine.Debug.Log($"Start Waypoint: {startWaypoint}, Target Waypoint: {targetWaypoint}");

            AStar(startWaypoint, targetWaypoint, waypointLookup, pathBuffer);
            SystemAPI.SetComponentEnabled<PathRequest>(entity, false);
        }
    }

    private Entity FindNearestWaypoint(ref SystemState state, float3 targetPosition)
    {
        Entity closestWaypoint = Entity.Null;
        float minDistance = float.MaxValue;

        // Update component lookup
        var waypointLookup = SystemAPI.GetComponentLookup<Waypoint>(true);
        waypointLookup.Update(ref state);

        foreach(var(waypoint, entity) in SystemAPI.Query<Waypoint>().WithEntityAccess())
        {
            float distance = math.distance(targetPosition, waypoint.position);
            if(distance <= minDistance)
            {
                minDistance = distance;
                closestWaypoint = entity;
            }
        }
        return closestWaypoint;
    }

    private void AStar(Entity startWaypoint, Entity endWaypoint, ComponentLookup<Waypoint> waypointLookup, DynamicBuffer<PathResult> pathBuffer)
    {
        NativeHashMap<Entity, Entity> cameFrom = new(100, Allocator.Temp);
        NativeHashMap<Entity, float> costSoFar = new NativeHashMap<Entity, float>(100, Allocator.Temp);
        NativePriorityQueue<PriorityQueueNode> openSet = new(Allocator.Temp);

        openSet.Enqueue(new PriorityQueueNode(startWaypoint, 0));
        cameFrom[startWaypoint] = startWaypoint;
        costSoFar[startWaypoint] = 0;

        while(openSet.Count > 0)
        {
            PriorityQueueNode current = openSet.Dequeue();
            Entity currentEntity = current.Node;

            if(currentEntity == endWaypoint) break; //Reach the path ending

            Waypoint currentWaypoint = waypointLookup[currentEntity];  

            foreach(Entity neighbor in currentWaypoint.Neighbors)
            {
                if (!waypointLookup.HasComponent(neighbor))
                {
                    UnityEngine.Debug.LogWarning($"Skipping neighbor {neighbor} - No Waypoint Component.");
                    continue;
                }

                float newCost = costSoFar[currentEntity] + math.distance(currentWaypoint.position, waypointLookup[neighbor].position);

                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;
                    float priority = newCost + math.distance(waypointLookup[neighbor].position, waypointLookup[endWaypoint].position);
                    openSet.Enqueue(new PriorityQueueNode(neighbor, priority));
                    cameFrom[neighbor] = currentEntity;
                }
            }       
        }

        //Reconstruct path
        if(cameFrom.ContainsKey(endWaypoint))
        {
            Entity step = endWaypoint;
            int count = 0;
            while(step != startWaypoint)
            {
                 var waypointPos = waypointLookup[step].position;
                pathBuffer.Insert(0, new PathResult {position = waypointLookup[step].position});
                step = cameFrom[step];
                count++;
            }

            pathBuffer.Insert(0, new PathResult {position = waypointLookup[startWaypoint].position});
        }
        else
        {
            UnityEngine.Debug.LogError("A* failed: No path found from startWaypoint to endWaypoint!");
        }

        cameFrom.Dispose();
        costSoFar.Dispose();
        openSet.Dispose();
    }
}

public struct NativePriorityQueue<T> : System.IDisposable where T : unmanaged, System.IComparable<T>
{
    private NativeList<T> _heap;
    
    public NativePriorityQueue(Allocator allocator)
    {
        _heap = new NativeList<T>(allocator);
    }

    public int Count => _heap.Length;

    public void Enqueue(T item)
    {
        _heap.Add(item);
        int index = _heap.Length - 1;
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            if (_heap[index].CompareTo(_heap[parentIndex]) >= 0) break;

            (_heap[index], _heap[parentIndex]) = (_heap[parentIndex], _heap[index]);
            index = parentIndex;
        }
    }

    public T Dequeue()
    {
        if (_heap.Length == 0) return default;

        T root = _heap[0];
        _heap[0] = _heap[_heap.Length - 1];
        _heap.RemoveAt(_heap.Length - 1);

        int index = 0;
        while (true)
        {
            int leftChild = 2 * index + 1;
            int rightChild = 2 * index + 2;
            int smallest = index;

            if (leftChild < _heap.Length && _heap[leftChild].CompareTo(_heap[smallest]) < 0)
                smallest = leftChild;
            if (rightChild < _heap.Length && _heap[rightChild].CompareTo(_heap[smallest]) < 0)
                smallest = rightChild;

            if (smallest == index) break;

            (_heap[index], _heap[smallest]) = (_heap[smallest], _heap[index]);
            index = smallest;
        }

        return root;
    }

    public void Dispose()
    {
        if (_heap.IsCreated) _heap.Dispose();
    }
}

public struct PriorityQueueNode : System.IComparable<PriorityQueueNode>
{
    public Entity Node;
    public float Cost;

    public PriorityQueueNode(Entity node, float cost)
    {
        Node = node;
        Cost = cost;
    }

    public int CompareTo(PriorityQueueNode other)
    {
        return Cost.CompareTo(other.Cost);
    }
}