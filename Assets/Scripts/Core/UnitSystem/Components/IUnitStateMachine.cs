using UnityEngine;

namespace SteelSurge.Core.UnitSystem.Components
{
    public interface IUnitStateMachine
    {
        Transform Self { get; }
        Transform Target { get; set; }
        Vector3 TargetPoint { get; set; }
        float AttackRange { get; set; }
        float AttackSpeed { get; set; }
        string DebugString { get; set; }

        void SetTarget(Transform target);
        void SetTargetPoint(Vector3 point);
        void ClearTarget();
    }
}
