using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

partial struct BuildingSelectedRpc : IRpcCommand
{
    public BuildingEnum BuildingEnum;
}
