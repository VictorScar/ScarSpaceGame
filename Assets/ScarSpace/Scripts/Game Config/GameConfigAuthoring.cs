using Unity.Entities;
using UnityEngine;

// Данные конфига: хранят ссылку на Entity-префаб башни
// Authoring скрипт для настройки в редакторе
public class GameConfigAuthoring : MonoBehaviour
{
    public GameObject TowerPrefab; // Сюда перетащим префаб цилиндра

    class Baker : Baker<GameConfigAuthoring>
    {
        public override void Bake(GameConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new GameConfig
            {
                TowerPrefab = GetEntity(authoring.TowerPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct GameConfig : IComponentData
{
    public Entity TowerPrefab;
}