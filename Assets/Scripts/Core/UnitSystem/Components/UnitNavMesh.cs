using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace SteelSurge.Core.UnitSystem.Components
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class UnitNavMesh : MonoBehaviour, IUnitNavMesh
    {
       [SerializeField, ReadOnly] private NavMeshAgent _agent;

        public Vector3 Velocity => _agent.velocity;
        public bool IsStopped => _agent.isStopped;
        public float Speed => _agent.speed;
        public float AngularSpeed => _agent.angularSpeed;
        public float Acceleration => _agent.acceleration;
        public float RemainingDistance => _agent.remainingDistance;
        public bool HasPath => _agent.hasPath;
        public bool PathPending => _agent.pathPending;
        public bool IsPathStale => _agent.isPathStale;
        public Vector3 Destination => _agent.destination;

        private void Awake()
        {
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
        }

        public void SetDestination(Vector3 destination)
        {
            _agent.SetDestination(destination);
        }

        public void Stop()
        {
            _agent.isStopped = true;
        }

        public void Resume()
        {
            _agent.isStopped = false;
        }

        public void Warp(Vector3 position)
        {
            _agent.Warp(position);
        }

        public void ResetPath()
        {
            _agent.ResetPath();
        }

        public void SetSpeed(float speed)
        {
            _agent.speed = speed;
        }

        public void SetAngularSpeed(float angularSpeed)
        {
            _agent.angularSpeed = angularSpeed;
        }

        public void SetAcceleration(float acceleration)
        {
            _agent.acceleration = acceleration;
        }

        private void OnValidate()
        {
            if (!_agent)
            {
                _agent = GetComponent<NavMeshAgent>();
            }
        }
    }
}
