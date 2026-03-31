using RenownedGames.AITree;
using RenownedGames.Apex;
using System;
using UnityEngine;

namespace SteelSurge.Core.AI.Decorators
{
    [NodeContent("Decorators/Has Not Target Point", "Проверяет: не установлен ли TargetPoint (negative infinity)")]
    public class HasNotTargetPointDecorator : ObserverDecorator
    {
        [Title("Blackboard")]
        [Tooltip("Ключ TargetPoint (Vector3)")]
        [SerializeField]
        [NonLocal]
        private Key targetPointKey;

        private bool _lastHasNoPoint;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _lastHasNoPoint = !HasTargetPoint();
        }

        public override event Action OnValueChange;

        public override bool CalculateResult()
        {
            bool hasNoPoint = !HasTargetPoint();

            if (_lastHasNoPoint != hasNoPoint)
            {
                _lastHasNoPoint = hasNoPoint;
                OnValueChange?.Invoke();
            }

            return hasNoPoint;
        }

        protected override void OnFlowUpdate() { }

        public override string GetDescription() => "TargetPoint не установлен";

        private bool HasTargetPoint()
        {
            if (targetPointKey == null)
            {
                return false;
            }

            if (targetPointKey.GetValueObject() is Vector3 point)
            {
                return point != Vector3.negativeInfinity;
            }

            return false;
        }
    }
}
