using UnityEngine;
using UnityEngine.AI;
using SteelSurge.Core.UnitSystem.Components;
using SteelSurge.Core.UnitSystem.Handlers;
using Sirenix.OdinInspector;

namespace SteelSurge.Core.UnitSystem
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(UnitNavMesh))]
    [RequireComponent(typeof(UnitStateMachine))]
    [RequireComponent(typeof(UnitRotationHandler))]
    public class Unit : SteelSurge.Core.Network.Components.NetworkObject, IUnit
    {
        [SerializeField, ReadOnly] private UnitNavMesh _navMesh;
        [SerializeField, ReadOnly] private UnitStateMachine _stateMachine;
        [SerializeField, ReadOnly] private UnitRotationHandler _rotationHandler;

        public IUnitNavMesh NavMesh => _navMesh;
        public IUnitStateMachine StateMachine => _stateMachine;
        public IUnitRotationHandler RotationHandler => _rotationHandler;

        private void OnValidate()
        {
            if (!_navMesh)
            {
                _navMesh = GetComponent<UnitNavMesh>();
            }

            if (!_stateMachine)
            {
                _stateMachine = GetComponent<UnitStateMachine>();
            }

            if (!_rotationHandler)
            {
                _rotationHandler = GetComponent<UnitRotationHandler>();
            }
        }
    }
}
