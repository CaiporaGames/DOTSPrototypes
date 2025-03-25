using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class TargetAuthoring : MonoBehaviour
{
    public int targetID; // Unique ID to match target with agent
    
    public class Baker : Baker<TargetAuthoring>
    {
        public override void Bake(TargetAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new TargetData
            {
                TargetID = authoring.targetID
            });
            
            // Add tag to identify this as a target
            AddComponent<TargetTag>(entity);
        }
    }
}

public struct TargetTag : IComponentData {}

public struct TargetData : IComponentData
{
    public int TargetID;
}