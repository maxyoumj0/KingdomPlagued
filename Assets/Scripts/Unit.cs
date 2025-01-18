using UnityEngine;

public class Unit : Entity
{
    public float MovementSpeed { get; set; }
    public float AttackDamage { get; set; }
    public float AttackRange { get; set; }

    public void MoveTo(Vector3 destination)
    {
        // Movement logic.
    }

    public void Attack(Entity target)
    {
        // Attack logic.
    }

    public override void Initialize()
    {
        // Set up default stats or custom logic for units.
    }
}
