using UnityEngine;
using SteelSurge.Core.UnitSystem;
using UniRx;
using System;

namespace SteelSurge.Core.UnitSystem.Handlers
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(IUnit))]
    public class UnitAnimatorHandler : MonoBehaviour, IUnitAnimatorHandler
    {
        [SerializeField] private float _moveSpeedThreshold = 0.1f;
        [SerializeField] private float _checkInterval = 0.1f;

        private Animator _animator;
        private IUnit _unit;
        private CompositeDisposable _disposables;
        private static readonly int RunHash = Animator.StringToHash("Run");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int IdleHash = Animator.StringToHash("Idle");

        private bool _isAttacking;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _unit = GetComponent<IUnit>();
            _disposables = new CompositeDisposable();
        }

        private void Start()
        {
            Observable.Interval(TimeSpan.FromSeconds(_checkInterval))
                .Select(_ => _unit.NavMesh.Velocity.sqrMagnitude > _moveSpeedThreshold * _moveSpeedThreshold)
                .DistinctUntilChanged()
                .Where(_ => !_isAttacking)
                .Subscribe(isMoving =>
                {
                    if (isMoving)
                        SetBoolInternal(AnimatorBoolParam.Run, true);
                    else
                        SetBoolInternal(AnimatorBoolParam.Idle, true);
                })
                .AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        public void SetBool(AnimatorBoolParam param, bool value)
        {
            if (param == AnimatorBoolParam.Attack)
            {
                _isAttacking = value;
            }
            
            SetBoolInternal(param, value);
        }

        private void SetBoolInternal(AnimatorBoolParam param, bool value)
        {
            int hash = GetHash(param);
            
            foreach (AnimatorBoolParam p in System.Enum.GetValues(typeof(AnimatorBoolParam)))
            {
                if (p != param)
                {
                    _animator.SetBool(GetHash(p), false);
                }
            }
            
            _animator.SetBool(hash, value);
        }

        public void PlayAttack()
        {
            SetBool(AnimatorBoolParam.Attack, true);
        }

        private int GetHash(AnimatorBoolParam param)
        {
            return param switch
            {
                AnimatorBoolParam.Run => RunHash,
                AnimatorBoolParam.Attack => AttackHash,
                AnimatorBoolParam.Idle => IdleHash,
                _ => 0
            };
        }
    }
}
