using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct MoventObstacleSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (obstacleComponent, localTransform) in SystemAPI.Query<RefRW<ObstacleComponent>, RefRW<LocalTransform>>())
        {
            // Only update obstacles that have speed greater than 0
            if (obstacleComponent.ValueRO.speed > 0)
            {
                // Move obstacle horizontally
                float moveDirection = obstacleComponent.ValueRO.isMovingRight ? 1f : -1f;
                if(!obstacleComponent.ValueRW.isWaiting)
                    localTransform.ValueRW.Position.z += moveDirection * obstacleComponent.ValueRO.speed * SystemAPI.Time.DeltaTime;

                // Check if the obstacle has reached its target position
                if (math.abs(localTransform.ValueRW.Position.z - obstacleComponent.ValueRO.targetPosition.y) < 0.1f)
                {
                    // Stop and start waiting if it reaches the target position
                    obstacleComponent.ValueRW.isWaiting = true;
                   
                    // Start the wait timer if the obstacle hasn't already been waiting
                    if (obstacleComponent.ValueRO.waitTime <= 0f)
                    {
                        obstacleComponent.ValueRW.waitTime = 2f; // Wait for 2 seconds
                    }
                }

                // Handle waiting time
                if (obstacleComponent.ValueRO.isWaiting)
                {
                    obstacleComponent.ValueRW.waitTime -= SystemAPI.Time.DeltaTime;
                    
                    // If the waiting time is over, reverse direction
                    if (obstacleComponent.ValueRO.waitTime <= 0f)
                    {
                        // Reset wait time and reverse direction
                        obstacleComponent.ValueRW.isWaiting = false;
                        obstacleComponent.ValueRW.isMovingRight = !obstacleComponent.ValueRO.isMovingRight;

                        //Avoid overshooting the position
                        if(moveDirection == 1)
                            localTransform.ValueRW.Position = 
                                new float3(obstacleComponent.ValueRO.targetPosition.x, localTransform.ValueRW.Position.y, obstacleComponent.ValueRO.targetPosition.y);
                        else
                            localTransform.ValueRW.Position = 
                                new float3(obstacleComponent.ValueRO.targetPosition.x, localTransform.ValueRW.Position.y, obstacleComponent.ValueRO.targetPosition.y-1);

                    }
                }
            }
        }
    }
}
