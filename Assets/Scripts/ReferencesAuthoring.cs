using Unity.Entities;
using UnityEngine;

class ReferencesAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject waypointPrefab;
    [SerializeField] private GameObject enemyPrefab;
    class Baker : Baker<ReferencesAuthoring>
    {
        public override void Bake(ReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ReferencesComponent
            {
                waypointEntity = GetEntity(authoring.waypointPrefab, TransformUsageFlags.Dynamic),
                enemyEntity = GetEntity(authoring.enemyPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }

}

public struct ReferencesComponent : IComponentData
{
    public Entity waypointEntity;
    public Entity enemyEntity;
}