using UnityEngine;

public class Building : Entity
{
    public int MaxCapacity { get; set; }
    public bool IsUnderConstruction { get; private set; }

    public void ProduceUnit(Unit unitType)
    {
        // Spawn or queue a unit for production.
    }

    public void Upgrade()
    {
        // Upgrade logic for building.
    }
}
