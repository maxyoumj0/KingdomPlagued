using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public static class PlayerCameraHelper
{
    public static Camera GetPlayerCamera(Entity playerEntity, EntityManager entityManager)
    {
        if (entityManager.HasComponent<LinkedEntityGroup>(playerEntity))
        {
            DynamicBuffer<LinkedEntityGroup> linkedEntities = entityManager.GetBuffer<LinkedEntityGroup>(playerEntity);

            foreach (var linkedEntity in linkedEntities)
            {
                if (entityManager.HasComponent<Camera>(linkedEntity.Value))
                {
                    return entityManager.GetComponentObject<Camera>(linkedEntity.Value);
                }
            }
        }

        Debug.LogError("No Camera found in LinkedEntityGroup!");
        return null;
    }
}
