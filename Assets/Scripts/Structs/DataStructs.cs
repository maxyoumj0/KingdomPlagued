using UnityEngine;

public struct BuildingData
{
    public string Type;      // Building type (e.g., House, Mine)
    public Vector3 Position; // Position in the world
    public int OwnerId;      // Player ID of the owner
    public float Health;     // Building health
}

public struct PawnData
{
    public string Type;      // Pawn type (e.g., Worker, Soldier)
    public Vector3 Position; // Position in the world
    public int OwnerId;      // Player ID of the owner
    public float Health;     // Pawn health
    public Vector3 Destination; // Destination for movement
}

public enum BuildingEnum { TestBuilding }