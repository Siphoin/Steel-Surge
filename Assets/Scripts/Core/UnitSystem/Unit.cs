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
    [RequireComponent(typeof(UnitAnimatorHandler))]
    public class Unit : SteelSurge.Core.Network.Components.NetworkObject, IUnit
    {
        [SerializeField, ReadOnly] private UnitNavMesh _navMesh;
        [SerializeField, ReadOnly] private UnitStateMachine _stateMachine;
        [SerializeField, ReadOnly] private UnitRotationHandler _rotationHandler;
        [SerializeField, ReadOnly] private UnitAnimatorHandler _animatorHandler;
        [SerializeField] private ScriptableUnitData _data;

        public ScriptableUnitData Data => _data;
        public IUnitNavMesh NavMesh => _navMesh;
        public IUnitStateMachine StateMachine => _stateMachine;
        public IUnitRotationHandler RotationHandler => _rotationHandler;
        public IUnitAnimatorHandler AnimatorHandler => _animatorHandler;

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

            if (!_animatorHandler)
            {
                _animatorHandler = GetComponent<UnitAnimatorHandler>();
            }
        }
    }
}
