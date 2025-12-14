using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

// Работаем только на клиенте (визуал)
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)] 
public partial struct TeamColorSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 1. Получаем конфиг с цветами
        if (!SystemAPI.TryGetSingletonEntity<GameConfig>(out var configEntity))
            return;

        // Получаем буфер цветов
        var colorBuffer = SystemAPI.GetBuffer<TeamColorElement>(configEntity);

        // 2. Проходим по всем сущностям, у которых есть FractionID и LinkedEntityGroup (дети)
        // Мы используем ChangeFilter, чтобы обрабатывать только те сущности, 
        // у которых FractionID только что изменился (или появился).
        foreach (var (fraction, linkedGroup) in SystemAPI.Query<RefRO<FractionID>, DynamicBuffer<LinkedEntityGroup>>()
                     .WithChangeFilter<FractionID>()) // Оптимизация!
        {
            int teamId = fraction.ValueRO.Value;

            // Защита от выхода за границы массива
            if (teamId < 0 || teamId >= colorBuffer.Length)
                teamId = 0;

            float4 teamColor = colorBuffer[teamId].Value;

            // 3. Ищем среди детей того, кто умеет менять цвет
            foreach (var child in linkedGroup)
            {
                // Проверяем, есть ли на ребенке компонент цвета
                if (SystemAPI.HasComponent<URPMaterialPropertyBaseColor>(child.Value))
                {
                    // Меняем цвет
                    SystemAPI.SetComponent(child.Value, new URPMaterialPropertyBaseColor { Value = teamColor });
                }
            }
        }
    }
}