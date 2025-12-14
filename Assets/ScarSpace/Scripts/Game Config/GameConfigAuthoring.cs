using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// Данные конфига: хранят ссылку на Entity-префаб башни
// Authoring скрипт для настройки в редакторе
public class GameConfigAuthoring : MonoBehaviour
{
    public GameObject TowerPrefab; // Сюда перетащим префаб цилиндра
    public TeamColorsConfig TeamColors;

    class Baker : Baker<GameConfigAuthoring>
    {
        public override void Bake(GameConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new GameConfig
            {
                TowerPrefab = GetEntity(authoring.TowerPrefab, TransformUsageFlags.Dynamic)
            });

            if (authoring.TeamColors != null)
            {
                var buffer = AddBuffer<TeamColorElement>(entity);

                foreach (var color in authoring.TeamColors.Colors)
                {
                    buffer.Add(new TeamColorElement
                    {
                        Value = new float4(color.r, color.g, color.b, color.a)
                    });
                }
            }
        }
    }
}

public struct GameConfig : IComponentData
{
    public Entity TowerPrefab;
}

public struct TeamColorElement : IBufferElementData
{
    public float4 Value;
}