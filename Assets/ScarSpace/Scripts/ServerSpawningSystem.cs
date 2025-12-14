using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)] // Работает только на сервере
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct ServerSpawningSystem : ISystem
{
    // Компонент-метка, чтобы знать, что для этого соединения мы уже все создали
    public struct PlayerSpawnedTag : IComponentData { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // DEBUG 1: Проверка конфига
        if (!SystemAPI.TryGetSingleton<GameConfig>(out var config))
        {
            // Если этот лог спамит - значит проблема в SubScene или Baker
            UnityEngine.Debug.LogWarning("ServerSpawningSystem: GameConfig не найден! Проверьте SubScene."); 
            return;
        }

        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        // DEBUG 2: Проверка цикла
        int newConnectionsCount = 0;

        foreach (var (netId, entity) in SystemAPI.Query<RefRO<NetworkId>>()
                     .WithAll<NetworkStreamInGame>()
                     .WithNone<PlayerSpawnedTag>()
                     .WithEntityAccess())
        {
            newConnectionsCount++;
            UnityEngine.Debug.Log($"Спавним башню для игрока ID: {netId.ValueRO.Value}");

            var towerEntity = ecb.Instantiate(config.TowerPrefab);
            float3 spawnPosition = new float3(netId.ValueRO.Value * 5f, 0, 0); 
            ecb.SetComponent(towerEntity, LocalTransform.FromPosition(spawnPosition));
            ecb.SetComponent(towerEntity, new FractionID { Value = netId.ValueRO.Value });
            ecb.AddComponent(towerEntity, new GhostOwner { NetworkId = netId.ValueRO.Value });
            ecb.AddComponent<PlayerSpawnedTag>(entity);
            ecb.AppendToBuffer(entity, new LinkedEntityGroup { Value = towerEntity });
        }
        
        // Если конфиг есть, но спавна нет - значит Query пустой
    }
}
