using UnityEngine;

namespace SteelSurge.Core.UnitSystem.Handlers
{
    public interface IUnitRotationHandler
    {
        void LookAt(Vector3 target);
        void SetTargetRotation(Quaternion rotation);
        void SetTargetDirection(Vector3 direction);
        void CancelRotation();
        void SetRotationSpeed(float speed);
        float GetRotationSpeed();
    }
}
