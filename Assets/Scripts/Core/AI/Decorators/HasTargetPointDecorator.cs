using RenownedGames.AITree;
using RenownedGames.Apex;
using System;
using UnityEngine;

namespace SteelSurge.Core.AI.Decorators
{
    [NodeContent("Decorators/Has Target Point", "Проверяет: установлен ли TargetPoint (не negative infinity)")]
    public class HasTargetPointDecorator : ObserverDecorator
    {
        [Title("Blackboard")]
        [Tooltip("Ключ TargetPoint (Vector3)")]
        [SerializeField]
        [NonLocal]
        private Key targetPointKey;

        private bool _lastHasPoint;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _lastHasPoint = HasTargetPoint();
        }

        public override event Action OnValueChange;

        public override bool CalculateResult()
        {
            bool hasPoint = HasTargetPoint();

            if (_lastHasPoint != hasPoint)
            {
                _lastHasPoint = hasPoint;
                OnValueChange?.Invoke();
            }

            return hasPoint;
        }

        protected override void OnFlowUpdate() { }

        public override string GetDescription() => "TargetPoint установлен";

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