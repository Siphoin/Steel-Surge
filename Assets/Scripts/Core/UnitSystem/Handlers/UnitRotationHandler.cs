using UnityEngine;
using SteelSurge.Core.UnitSystem;

namespace SteelSurge.Core.UnitSystem.Handlers
{
    [RequireComponent(typeof(Unit))]
    public class UnitRotationHandler : MonoBehaviour, IUnitRotationHandler
    {
        [SerializeField] private float _rotationSpeed = 180f;
        [SerializeField] private float _stopRotateThreshold = 0.1f;

        private IUnit _unit;
        private Transform _transform;
        private Quaternion? _targetRotation;

        private void Awake()
        {
            _unit = GetComponent<IUnit>();
            _transform = transform;
        }

        private void Update()
        {
            if (_targetRotation.HasValue)
            {
                RotateSmoothly(_targetRotation.Value);
                return;
            }

            if (_unit.NavMesh.HasPath && _unit.NavMesh.Velocity.sqrMagnitude > _stopRotateThreshold * _stopRotateThreshold)
            {
                RotateTowardsVelocity();
            }
        }

        private void RotateTowardsVelocity()
        {
            Vector3 direction = _unit.NavMesh.Velocity;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                _transform.rotation = Quaternion.RotateTowards(
                    _transform.rotation,
                    targetRot,
                    _rotationSpeed * Time.deltaTime
                );
            }
        }

        private void RotateSmoothly(Quaternion targetRot)
        {
            _transform.rotation = Quaternion.RotateTowards(
                _transform.rotation,
                targetRot,
                _rotationSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(_transform.rotation, targetRot) < 0.1f)
            {
                _transform.rotation = targetRot;
                _targetRotation = null;
            }
        }

        public void LookAt(Vector3 target)
        {
            Vector3 direction = target - _transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                _transform.rotation = targetRot;
                _targetRotation = null;
            }
        }

        public void SetTargetRotation(Quaternion rotation)
        {
            _targetRotation = rotation;
        }

        public void SetTargetDirection(Vector3 direction)
        {
            direction.y = 0;
            if (direction.sqrMagnitude > 0.001f)
            {
                _targetRotation = Quaternion.LookRotation(direction);
            }
        }

        public void CancelRotation()
        {
            _targetRotation = null;
        }

        public void SetRotationSpeed(float speed)
        {
            _rotationSpeed = Mathf.Max(1f, speed);
        }

        public float GetRotationSpeed()
        {
            return _rotationSpeed;
        }
    }
}
