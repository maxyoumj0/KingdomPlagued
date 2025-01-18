using Unity.Netcode;
using UnityEngine;

public abstract class Entity : NetworkBehaviour
{
    public Vector3 Position { get; set; }
    public bool IsSelected { get; private set; }
    public int MaxHealth;
    public int CurHealth;

    public virtual void Select(Color playerColor)
    {
        IsSelected = true;
        
    }

    public virtual void Deselect()
    {
        IsSelected = false;
        // Remove visual mark.
    }

    // Method to initialize entity data.
    public abstract void Initialize();
}
