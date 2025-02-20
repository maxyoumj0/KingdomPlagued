using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;

class TESTDUMMY : MonoBehaviour
{
    class TESTDUMMYBaker : Baker<TESTDUMMY>
    {
        public override void Bake(TESTDUMMY authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent<PhysicsVelocity>(entity);
            AddComponent<DUMMYComponent>(entity);
            AddComponent(entity, new GhostInstance());
        }
    }
}

[GhostComponent]
class DUMMYComponent : IComponentData
{

}