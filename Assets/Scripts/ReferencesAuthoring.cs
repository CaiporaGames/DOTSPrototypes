using Unity.Entities;
using UnityEngine;

class ReferencesAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject waypointPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform targetPosition;
    class Baker : Baker<ReferencesAuthoring>
    {
        public override void Bake(ReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ReferencesComponent
            {
                waypointEntity = GetEntity(authoring.waypointPrefab, TransformUsageFlags.None),
                enemyEntity = GetEntity(authoring.enemyPrefab, TransformUsageFlags.Dynamic),
                targetPositionEntity = GetEntity(authoring.targetPosition, TransformUsageFlags.None)
            });
        }
    }

}

public struct ReferencesComponent : IComponentData
{
    public Entity waypointEntity;
    public Entity enemyEntity;
    public Entity enemySpawnPointEntity;
    public Entity targetPositionEntity;
}