using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine; // Для Debug.Log

public struct GoInGameRequest : IRpcCommand
{
    public int DesiredColorID; 
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
public partial class GoInGameServerSystem : SystemBase
{
    [BurstCompile]
    protected override void OnUpdate()
    {
        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(World.Unmanaged);

        if (!SystemAPI.TryGetSingletonRW<ServerGameState>(out var serverState))
        {
            var entity = EntityManager.CreateEntity(typeof(ServerGameState));
            EntityManager.SetComponentData(entity, new ServerGameState{NextColorIndex = 0});
            return;
        }
        
        // Ищем входящие запросы RPC
        foreach (var (req, sourceConn, entity) in SystemAPI.Query<RefRO<GoInGameRequest>, RefRO<ReceiveRpcCommandRequest>>()
                     .WithEntityAccess())
        {

            var connectionEntity = sourceConn.ValueRO.SourceConnection;
            var assignedColorId = serverState.ValueRO.NextColorIndex;
            serverState.ValueRW.NextColorIndex++;
            
            ecb.AddComponent(connectionEntity, new PlayerData
            {
                ColorID = assignedColorId
            });
            
            Debug.Log($"[Server] Игрок зашел. Выдан ColorID: {assignedColorId}");
            ecb.AddComponent<NetworkStreamInGame>(connectionEntity);
            ecb.DestroyEntity(entity);
        }
    }
}
