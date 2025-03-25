using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct PathfindingSystemCreationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PathfindingSystemsTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Get world reference
        var world = state.EntityManager.World;
        
        // We need to check if our systems already exist before creating them
        bool systemsCreated = false;
        var entityQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<PathfindingSystemsCreatedTag>()
            .Build(state.EntityManager);
        
        systemsCreated = entityQuery.CalculateEntityCount() > 0;
        entityQuery.Dispose();
        
        if (!systemsCreated)
        {
            // Create our systems
            var gridUpdateSystem = world.CreateSystem<GridUpdateSystem>();
            var pathRequestSystem = world.CreateSystem<PathRequestSystem>();
            var pathfindingSystem = world.CreateSystem<PathfindingSystem>();
            var pathFollowingSystem = world.CreateSystem<PathFollowingSystem>();
            
            // Create the pathfinding group
            var pathfindingGroup = world.GetOrCreateSystemManaged<PathfindingSystemGroup>();
            
            // Add our systems to the simulation group in proper order
            var simulationGroup = world.GetOrCreateSystemManaged<SimulationSystemGroup>();
            simulationGroup.AddSystemToUpdateList(pathfindingGroup);
            
            // Make sure our systems run in the right order
            pathfindingGroup.AddSystemToUpdateList(gridUpdateSystem);
            pathfindingGroup.AddSystemToUpdateList(pathRequestSystem);
            pathfindingGroup.AddSystemToUpdateList(pathfindingSystem);
            pathfindingGroup.AddSystemToUpdateList(pathFollowingSystem);
            
            // Create marker entity to indicate systems are created
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var markerEntity = ecb.CreateEntity();
            ecb.AddComponent<PathfindingSystemsCreatedTag>(markerEntity);
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        
        // Disable this system after it runs once
        state.Enabled = false;
    }
}

// Tag to indicate systems have been created
public struct PathfindingSystemsCreatedTag : IComponentData {}

