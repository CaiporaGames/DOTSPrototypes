
using System;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
public partial struct WaypointGenerationSystem : ISystem
{
    int gridWidth;
    int gridDepth;
    float gridSpacing;
    float terrainOffset;

    public void OnCreate(ref SystemState state)
    {
        gridWidth = 30;
        gridDepth = 30;
        gridSpacing = 1;
        terrainOffset = 3f;
    }

    public void OnUpdate(ref SystemState state)
    {
        //Gets world physics
        var physicsWorldData = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        var physicsWorld = physicsWorldData.PhysicsWorld;

         // Get the waypoint prefab entity
        var waypointPrefabData = SystemAPI.GetSingleton<ReferencesComponent>().waypointEntity;

        //Generate waypoints
        for(int x = 0; x < gridWidth; x++)
        {
            for(int z = 0; z < gridDepth; z++)
            {
                float3 start = new float3(x * gridSpacing, 100f, z * gridSpacing);
                float3 end = new float3(x * gridSpacing, -100f, z * gridSpacing);
                //generate raycast for waypoint placement
                if(RaycastGround(ref physicsWorld, start, end, out float3 hitPosition))
                {   
                    hitPosition.y += terrainOffset;
                    
                    Entity waypointEntity = state.EntityManager.Instantiate(waypointPrefabData);
                    SystemAPI.SetComponent(waypointEntity, LocalTransform.FromPosition(hitPosition));
                }
            }
        }

        state.Enabled = false;
    }

    private bool RaycastGround(ref PhysicsWorld physicsWorld, float3 start, float3 end, out float3 hitPosition)
    {
        RaycastInput raycastInput = new RaycastInput
        {
            Start = start,
            End = end,
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << 0, // default layer is ground
                GroupIndex = 0
            }
        };

        if (physicsWorld.CastRay(raycastInput, out RaycastHit hit))
        {
            hitPosition = hit.Position;

            return true;
        }

        hitPosition = float3.zero;
        return false;
    }

}