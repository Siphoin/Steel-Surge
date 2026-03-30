using RenownedGames.AITree;
using UnityEngine;

namespace SteelSurge.Core.UnitSystem.Components
{
    [RequireComponent(typeof(BehaviourRunner))]
    public class UnitStateMachine : MonoBehaviour, IUnitStateMachine
    {
        private BehaviourRunner _behaviourRunner;
        private Blackboard _blackboard;

        private TransformKey _selfKey;
        private TransformKey _targetKey;
        private Vector3Key _targetPointKey;
        private FloatKey _attackRangeKey;
        private FloatKey _attackSpeedKey;
        private StringKey _debugStringKey;

        public Transform Self => _selfKey?.GetValue();
        
        public Transform Target
        {
            get => _targetKey?.GetValue();
            set
            {
                if (_targetKey != null)
                    _targetKey.SetValue(value);
            }
        }

        public Vector3 TargetPoint
        {
            get => _targetPointKey?.GetValue() ?? Vector3.zero;
            set
            {
                if (_targetPointKey != null)
                    _targetPointKey.SetValue(value);
            }
        }

        public float AttackRange
        {
            get => _attackRangeKey?.GetValue() ?? 0f;
            set
            {
                if (_attackRangeKey != null)
                    _attackRangeKey.SetValue(value);
            }
        }

        public float AttackSpeed
        {
            get => _attackSpeedKey?.GetValue() ?? 0f;
            set
            {
                if (_attackSpeedKey != null)
                    _attackSpeedKey.SetValue(value);
            }
        }

        public string DebugString
        {
            get => _debugStringKey?.GetValue() ?? string.Empty;
            set
            {
                if (_debugStringKey != null)
                    _debugStringKey.SetValue(value);
            }
        }

        private void Awake()
        {
            _behaviourRunner = GetComponent<BehaviourRunner>();
        }

        private void Start()
        {
            InitializeBlackboard();
        }

        private void InitializeBlackboard()
        {
            _blackboard = _behaviourRunner.GetBlackboard();
            if (_blackboard == null)
            {
                Debug.LogWarning($"[UnitStateMachine] Blackboard not found on {gameObject.name}");
                return;
            }

            if (_blackboard.TryFindKey("Self", out _selfKey))
            {
                _selfKey.SetValue(transform);
            }

            _blackboard.TryFindKey("Target", out _targetKey);
            _blackboard.TryFindKey("TargetPoint", out _targetPointKey);
            _blackboard.TryFindKey("AttackRange", out _attackRangeKey);
            _blackboard.TryFindKey("AttackSpeed", out _attackSpeedKey);
            _blackboard.TryFindKey("DEBUG_STRING", out _debugStringKey);
        }

        public void SetTarget(Transform target)
        {
            Target = target;
        }

        public void SetTargetPoint(Vector3 point)
        {
            TargetPoint = point;
        }

        public void ClearTarget()
        {
            Target = null;
        }
    }
}
