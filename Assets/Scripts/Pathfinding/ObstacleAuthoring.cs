using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class ObstacleAuthoring : MonoBehaviour
{
    class Baker : Baker<ObstacleAuthoring>
    {
        public override void Bake(ObstacleAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ObstacleComponent
            {
                position = new int2 (
                                    (int)math.round(authoring.transform.position.x),
                                    (int)math.round(authoring.transform.position.z) // Assuming Y is up
                                    )
            });
        }
    }
}

public struct ObstacleComponent : IComponentData
{
    public int2  position;
}

