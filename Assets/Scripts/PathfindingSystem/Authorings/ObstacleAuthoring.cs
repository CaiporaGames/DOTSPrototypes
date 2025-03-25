using Unity.Entities;
using UnityEngine;

// Authoring component for obstacles
public class ObstacleAuthoring : MonoBehaviour
{
    public class Baker : Baker<ObstacleAuthoring>
    {
        public override void Bake(ObstacleAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<ObstacleTag>(entity);
        }
    }
}