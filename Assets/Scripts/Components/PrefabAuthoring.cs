using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;

public class PrefabAuthoring : MonoBehaviour
{
    public GameObject PlayerPrefab;
    public GameObject GrassTilePrefab;
    public GameObject DirtTilePrefab;
    public GameObject SandTilePrefab;
    public GameObject WaterTilePrefab;
    public GameObject StoneTilePrefab;
    public GameObject MapManagerPrefab;
    public GameObject TestBuildingBlueprintPrefab;
    public GameObject TestBuildingPrefab;

    public class PrefabBaker : Baker<PrefabAuthoring>
    {
        public override void Bake(PrefabAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new PrefabReferencesComponent
            {
                PlayerPrefab = GetEntity(authoring.PlayerPrefab, TransformUsageFlags.Dynamic),
                GrassTilePrefab = GetEntity(authoring.GrassTilePrefab, TransformUsageFlags.Renderable),
                DirtTilePrefab = GetEntity(authoring.DirtTilePrefab, TransformUsageFlags.Renderable),
                SandTilePrefab = GetEntity(authoring.SandTilePrefab, TransformUsageFlags.Renderable),
                WaterTilePrefab = GetEntity(authoring.WaterTilePrefab, TransformUsageFlags.Renderable),
                StoneTilePrefab = GetEntity(authoring.StoneTilePrefab, TransformUsageFlags.Renderable),
                MapManagerPrefab = GetEntity(authoring.MapManagerPrefab, TransformUsageFlags.Renderable),
                TestBuildingBlueprintPrefab = GetEntity(authoring.TestBuildingBlueprintPrefab, TransformUsageFlags.Dynamic),
                TestBuildingPrefab = GetEntity(authoring.TestBuildingPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct PrefabReferencesComponent : IComponentData
{
    public Entity PlayerPrefab;
    public Entity GrassTilePrefab;
    public Entity DirtTilePrefab;
    public Entity SandTilePrefab;
    public Entity WaterTilePrefab;
    public Entity StoneTilePrefab;
    public Entity MapManagerPrefab;
    public Entity TestBuildingBlueprintPrefab;
    public Entity TestBuildingPrefab;
}