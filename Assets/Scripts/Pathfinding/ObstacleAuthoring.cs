using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class ObstacleAuthoring : MonoBehaviour
{
    [SerializeField] private float speed;
    class Baker : Baker<ObstacleAuthoring>
    {
        public override void Bake(ObstacleAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ObstacleComponent
            {
                speed = authoring.speed,
                position = new int2 (
                                    (int)math.round(authoring.transform.position.x),
                                    (int)math.round(authoring.transform.position.z)
                                    ),
                targetPosition =  new int2 (
                                    (int)math.round(authoring.transform.position.x),
                                    (int)math.round(authoring.transform.position.z + 1)
                                    ),
                isMovingRight = true
            });
        }
    }
}

public struct ObstacleComponent : IComponentData
{
    public float speed; // Speed of the obstacle
    public float2 position; // Current position
    public float2 targetPosition; // Target position to move to (e.g., for left/right movement)
    public bool isMovingRight; // Whether the obstacle is moving right or left
    public bool isWaiting; // Whether the obstacle is currently waiting at its target
    public float waitTime; // Time spent waiting before reversing direction
}


