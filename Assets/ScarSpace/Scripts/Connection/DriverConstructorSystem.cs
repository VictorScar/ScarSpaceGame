using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class DriverConstructorSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // 1. Создаем драйвер (если его нет)
        if (!SystemAPI.TryGetSingleton<NetworkStreamDriver>(out var driver))
        {
            var entity = EntityManager.CreateEntity(typeof(NetworkStreamDriver));
            Debug.Log($"✅ [DriverSystem] Драйвер создан в мире: {World.Name}");
        }

        // 2. Логика для Клиента (Connect)
        if ((World.Flags & WorldFlags.GameClient) == WorldFlags.GameClient)
        {
             var connectionQuery = SystemAPI.QueryBuilder().WithAll<NetworkId>().Build();
             if (connectionQuery.IsEmpty)
             {
                 Debug.Log("🔌 [DriverSystem] Клиент подключается к 127.0.0.1:7979...");
                 
                 var req = EntityManager.CreateEntity(typeof(NetworkStreamRequestConnect));
                 // Исправлено: Endpoint вместо Address
                 EntityManager.SetComponentData(req, new NetworkStreamRequestConnect 
                 { 
                     Endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(7979) 
                 });
             }
        }
        
        // 3. Логика для Сервера (Listen)
        if ((World.Flags & WorldFlags.GameServer) == WorldFlags.GameServer)
        {
            var listenQuery = SystemAPI.QueryBuilder().WithAll<NetworkStreamRequestListen>().Build();
            if (listenQuery.IsEmpty)
            {
                 Debug.Log("🎧 [DriverSystem] Сервер запускает Listen на порту 7979...");
                 
                 var req = EntityManager.CreateEntity(typeof(NetworkStreamRequestListen));
                 // Здесь поле тоже называется Endpoint
                 EntityManager.SetComponentData(req, new NetworkStreamRequestListen 
                 { 
                     Endpoint = NetworkEndpoint.AnyIpv4.WithPort(7979) 
                 });
            }
        }

        Enabled = false; // Выполняем 1 раз и выключаемся
    }
}
