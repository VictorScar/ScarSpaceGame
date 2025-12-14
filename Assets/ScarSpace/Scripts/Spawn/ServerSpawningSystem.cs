using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)] // Работает только на сервере
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct ServerSpawningSystem : ISystem
{
    // Компонент-метка, чтобы знать, что для этого соединения мы уже все создали
    public struct PlayerSpawnedTag : IComponentData
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingleton<GameConfig>(out var config)) return;

        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (netId, playerData, entity) in SystemAPI.Query<RefRO<NetworkId>,
                         RefRO<PlayerData>>()
                     .WithAll<NetworkStreamInGame>()
                     .WithNone<PlayerSpawnedTag>()
                     .WithEntityAccess())
        {
            int playerId = netId.ValueRO.Value;
            Debug.Log($"[Server] Игрок {playerId} заходит. Ищу точку спавна...");

            var spawnPointEntity = Entity.Null;
            var spawnPosition = float3.zero;
           
            var foundPoint = false;
            int pointIndex = 0;

            foreach (var (sp, spEntity) in SystemAPI.Query<RefRW<SpawnPoint>>().WithEntityAccess())
            {
                if (!sp.ValueRO.IsOccupied)
                {
                    sp.ValueRW.IsOccupied = true;
                    sp.ValueRW.OccupiedByNetworkID = playerId;

                    spawnPointEntity = spEntity;
                    spawnPosition = sp.ValueRO.Position;
                   foundPoint = true;

                    Debug.Log($"[Server] Точка найдена: {spawnPosition}");
                    break; // Берем первую попавшуюся и выходим
                }

                pointIndex++;
            }

            if (!foundPoint)
            {
                Debug.LogError($"[Server] НЕТ СВОБОДНЫХ ТОЧЕК СПАВНА для игрока {playerId}! Спавн отменен.");
                // Можно кикнуть игрока или отправить в наблюдатели
            }

            var towerEntity = ecb.Instantiate(config.TowerPrefab);
            ecb.SetComponent(towerEntity, LocalTransform.FromPosition(spawnPosition));
            ecb.SetComponent(towerEntity, new FractionID { Value = playerData.ValueRO.ColorID });
            ecb.AddComponent(towerEntity, new GhostOwner { NetworkId = netId.ValueRO.Value });

            ecb.AddComponent<PlayerSpawnedTag>(entity);
            ecb.AppendToBuffer(entity, new LinkedEntityGroup { Value = towerEntity });
        }
    }
}