using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;

public class PrefabAuthoring : MonoBehaviour
{
    public GameObject PlayerPrefab;

    public class PrefabBaker : Baker<PrefabAuthoring>
    {
        public override void Bake(PrefabAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new PrefabReferencesComponent
            {
                PlayerPrefab = GetEntity(authoring.PlayerPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct PrefabReferencesComponent : IComponentData
{
    public Entity PlayerPrefab;
}