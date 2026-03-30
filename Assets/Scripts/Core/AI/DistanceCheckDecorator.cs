using UnityEngine;
using RenownedGames.AITree;
using RenownedGames.Apex;
using System;

namespace SteelSurge.Core.AI.Decorators
{
    [NodeContent("Decorators/Distance Check", "Universal distance check")]
    public class DistanceCheckDecorator : ObserverDecorator
    {
        public enum ComparisonType
        {
            LessThan,
            LessThanOrEqual,
            GreaterThan,
            GreaterThanOrEqual,
            Equal
        }

        [Title("Blackboard")]
        [Tooltip("Target key (Transform or Vector3)")]
        [SerializeField]
        [NonLocal]
        private Key targetKey;

        [Tooltip("Distance key (Float)")]
        [SerializeField]
        [NonLocal]
        private Key rangeKey;

        [Title("Settings")]
        [Tooltip("Comparison type: how we compare actual distance with the number from Blackboard")]
        [SerializeField]
        private ComparisonType comparison = ComparisonType.LessThanOrEqual;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (targetKey != null) targetKey.ValueChanged += OnValueChange;
            if (rangeKey != null) rangeKey.ValueChanged += OnValueChange;
        }

        public override event Action OnValueChange;

        public override bool CalculateResult()
        {
            if (GetOwner() == null || targetKey == null || rangeKey == null) return false;

            Vector3 targetPosition;
            object targetVal = targetKey.GetValueObject();

            if (targetVal is Transform t) targetPosition = t.position;
            else if (targetVal is Vector3 v) targetPosition = v;
            else return false;

            object rangeVal = rangeKey.GetValueObject();
            if (!(rangeVal is float threshold)) return false;

            float currentDistance = Vector3.Distance(GetOwner().transform.position, targetPosition);

            // Universal comparison logic
            return comparison switch
            {
                ComparisonType.LessThan => currentDistance < threshold,
                ComparisonType.LessThanOrEqual => currentDistance <= threshold,
                ComparisonType.GreaterThan => currentDistance > threshold,
                ComparisonType.GreaterThanOrEqual => currentDistance >= threshold,
                ComparisonType.Equal => Mathf.Approximately(currentDistance, threshold),
                _ => false,
            };
        }

        protected override void OnFlowUpdate() { }

        public override string GetDescription()
        {
            string symbol = comparison switch
            {
                ComparisonType.LessThan => "<",
                ComparisonType.LessThanOrEqual => "<=",
                ComparisonType.GreaterThan => ">",
                ComparisonType.GreaterThanOrEqual => ">=",
                ComparisonType.Equal => "==",
                _ => "?"
            };
            return $"Distance to {targetKey?.name} {symbol} {rangeKey?.name}";
        }
    }
}