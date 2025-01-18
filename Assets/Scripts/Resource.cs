using UnityEngine;

public class Resource : Entity
{
    public int RemainingQuantity { get; private set; }
    public ResourceType ResourceType { get; set; } // Enum for resource types.

    public void Harvest(int amount)
    {
        RemainingQuantity -= amount;
        if (RemainingQuantity <= 0)
        {
            Destroy(gameObject); // Remove resource when depleted.
        }
    }
}

public enum ResourceType
{
    Wood,
    Stone,
    Gold
}

