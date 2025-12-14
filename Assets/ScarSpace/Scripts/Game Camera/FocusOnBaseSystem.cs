using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

// Тег-метка, чтобы камера создавалась только один раз за сессию подключения
public struct LocalCameraCreatedTag : IComponentData { }

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class FocusOnBaseSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // 1. Ждем подключения (наличия NetworkId)
        if (!SystemAPI.TryGetSingleton<NetworkId>(out var myNetId)) 
            return;

        var networkIdEntity = SystemAPI.GetSingletonEntity<NetworkId>();

        // 2. Если камера уже была создана для этого подключения — выходим
        if (EntityManager.HasComponent<LocalCameraCreatedTag>(networkIdEntity)) 
            return;

        int myId = myNetId.Value;
        
        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(World.Unmanaged);

        // 3. Ищем базу, принадлежащую нам
        foreach (var (transform, owner) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<GhostOwner>>())
        {
            if (owner.ValueRO.NetworkId == myId)
            {
                // --- СОЗДАНИЕ КАМЕРЫ ---
                var cameraEntity = ecb.CreateEntity();
                
                // Конфигурация камеры
                ecb.AddComponent(cameraEntity, new RTSCameraConfig 
                { 
                    MoveSpeed = 30f, 
                    ZoomSpeed = 2f, 
                    Height = 25f 
                });
                
                // Стартовая позиция: над базой со смещением
                float3 basePos = transform.ValueRO.Position;
                float3 startPos = new float3(basePos.x, 25f, basePos.z - 15f);
                
                ecb.AddComponent(cameraEntity, LocalTransform.FromPosition(startPos)
                    .WithRotation(quaternion.RotateX(math.radians(60))));
                
                // Тег для трекера камеры (MonoBehaviour)
                ecb.AddComponent(cameraEntity, new ActiveCameraTag());

                // 4. Ставим метку "Камера создана", чтобы больше не искать
                ecb.AddComponent<LocalCameraCreatedTag>(networkIdEntity);
                
                return; // Работа сделана
            }
        }
    }
}
