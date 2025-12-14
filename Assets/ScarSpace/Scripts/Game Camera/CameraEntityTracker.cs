using Unity.Entities;
using Unity.NetCode; 
using Unity.Transforms;
using UnityEngine;

public class CameraEntityTracker : MonoBehaviour
{
    private EntityManager _em;
    private EntityQuery _query;
    private bool _isInitialized = false;

    void TryInit()
    {
        // Перебираем ВСЕ миры, чтобы найти именно Клиентский
        foreach (var world in World.All)
        {
            if (world.IsClient() && !world.IsThinClient())
            {
                _em = world.EntityManager;
                // Создаем запрос в ЭТОМ мире
                _query = _em.CreateEntityQuery(typeof(LocalTransform), typeof(ActiveCameraTag));
                _isInitialized = true;
                Debug.Log($"✅ [Tracker] Успешно подключился к миру: {world.Name}");
                break;
            }
        }
    }

    void LateUpdate()
    {
        if (!_isInitialized)
        {
            TryInit();
            return;
        }

        // ВАЖНО: Проверка IsEmptyIgnoreFilter (быстрее)
        if (_query.IsEmptyIgnoreFilter) 
        {
            // Если хотите видеть, что он ищет, но не находит:
            // Debug.LogWarning("[Tracker] Ищу камеру... пока пусто.");
            return;
        }

        var entity = _query.GetSingletonEntity();
        var pos = _em.GetComponentData<LocalTransform>(entity);

        transform.position = pos.Position;
        transform.rotation = pos.Rotation;
    }
}