using UnityEngine;

namespace SteelSurge.Core.UnitSystem.Components
{
    public interface IUnitNavMesh
    {
        Vector3 Velocity { get; }
        bool IsStopped { get; }
        float Speed { get; }
        float AngularSpeed { get; }
        float Acceleration { get; }
        float RemainingDistance { get; }
        bool HasPath { get; }
        bool PathPending { get; }
        bool IsPathStale { get; }
        Vector3 Destination { get; }

        void SetDestination(Vector3 destination);
        void Stop();
        void Resume();
        void Warp(Vector3 position);
        void ResetPath();
        void SetSpeed(float speed);
        void SetAngularSpeed(float angularSpeed);
        void SetAcceleration(float acceleration);
    }
}
