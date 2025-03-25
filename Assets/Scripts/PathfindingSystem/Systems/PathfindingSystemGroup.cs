using Unity.Entities;
using Unity.Transforms;

// Define system ordering
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial class PathfindingSystemGroup : ComponentSystemGroup {}

// Place systems in the group with specific order
[UpdateInGroup(typeof(PathfindingSystemGroup))]
[UpdateBefore(typeof(PathRequestSystem))]
public partial struct GridUpdateSystem : ISystem {}

[UpdateInGroup(typeof(PathfindingSystemGroup))]
[UpdateAfter(typeof(GridUpdateSystem))]
[UpdateBefore(typeof(PathfindingSystem))]
public partial struct PathRequestSystem : ISystem {}

[UpdateInGroup(typeof(PathfindingSystemGroup))]
[UpdateAfter(typeof(PathRequestSystem))]
[UpdateBefore(typeof(PathFollowingSystem))]
public partial struct PathfindingSystem : ISystem {}

[UpdateInGroup(typeof(PathfindingSystemGroup))]
[UpdateAfter(typeof(PathfindingSystem))]
public partial struct PathFollowingSystem : ISystem {}