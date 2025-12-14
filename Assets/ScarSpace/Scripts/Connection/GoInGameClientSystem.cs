using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine; // Для Debug.Log

public struct GoInGameRequest : IRpcCommand
{
    // Пустая структура, данные не нужны
}

// 1. Клиентская часть
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
[BurstCompile]
public partial struct GoInGameClientSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RpcCollection>();
        state.RequireForUpdate<NetworkId>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        // Ищем подключение без тега InGame
        foreach (var (id, entity) in SystemAPI.Query<RefRO<NetworkId>>()
                     .WithNone<NetworkStreamInGame>()
                     .WithEntityAccess())
        {
            // ЛОГ!
            Debug.Log($"[Client] Подключение обнаружено (ID: {id.ValueRO.Value}). Отправляю запрос GoInGame...");

            ecb.AddComponent<NetworkStreamInGame>(entity);
            
            var req = ecb.CreateEntity();
            ecb.AddComponent<GoInGameRequest>(req);
            ecb.AddComponent(req, new SendRpcCommandRequest { TargetConnection = entity });
        }
    }
}

// 2. Серверная часть
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct GoInGameServerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        // Ищем входящие запросы RPC
        foreach (var (req, sourceConn, entity) in SystemAPI.Query<RefRO<GoInGameRequest>, RefRO<ReceiveRpcCommandRequest>>()
                     .WithEntityAccess())
        {
            // ЛОГ!
            Debug.Log("[Server] Получен запрос GoInGame! Добавляю игрока в игру.");

            var connectionEntity = sourceConn.ValueRO.SourceConnection;
            ecb.AddComponent<NetworkStreamInGame>(connectionEntity);
            ecb.DestroyEntity(entity);
        }
    }
}
