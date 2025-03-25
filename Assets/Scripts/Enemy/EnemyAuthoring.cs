using Unity.Entities;
using UnityEngine;

class EnemyAuthoring : MonoBehaviour
{
    [SerializeField] private float speed;
    class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnemyComponent
            {
                speed = authoring.speed
            });
        }
    }
}

public struct EnemyComponent : IComponentData
{
    public float speed;
}

