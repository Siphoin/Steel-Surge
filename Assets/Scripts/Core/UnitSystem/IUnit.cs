using SteelSurge.Core.UnitSystem.Components;
using SteelSurge.Core.UnitSystem.Handlers;

namespace SteelSurge.Core.UnitSystem
{
    public interface IUnit
    {
        IUnitNavMesh NavMesh { get; }
        IUnitStateMachine StateMachine { get; }
        IUnitRotationHandler RotationHandler { get; }
        IUnitAnimatorHandler AnimatorHandler { get; }
    }
}