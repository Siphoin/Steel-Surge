using SteelSurge.Core.UnitSystem.Components;

namespace SteelSurge.Core.UnitSystem
{
    public interface IUnit
    {
        IUnitNavMesh NavMesh { get; }
    }
}