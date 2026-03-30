using RenownedGames.AITree;
using RenownedGames.Apex;
using SteelSurge.Core.Network.HealthSystem.Components;
using System;
using Unity.Netcode;
using UnityEngine;

namespace SteelSurge.Core.AI.Decorators
{
    [NodeContent("Decorators/Is Alive", "Check: is target alive")]
    public class IsAliveDecorator : ObserverDecorator
    {
        [Title("Blackboard")]
        [Tooltip("Target key (GameObject or HealthComponent)")]
        [SerializeField]
        [NonLocal]
        private Key targetKey;

        [Title("Settings")]
        [Tooltip("Invert check: target is NOT alive")]
        [SerializeField]
        private bool invert = false;

        private bool _lastIsAlive;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _lastIsAlive = IsAlive();
        }

        public override event Action OnValueChange;

        public override bool CalculateResult()
        {
            bool isAlive = IsAlive();

            if (_lastIsAlive != isAlive)
            {
                _lastIsAlive = isAlive;
                OnValueChange?.Invoke();
            }

            return invert ? !isAlive : isAlive;
        }

        protected override void OnFlowUpdate() { }

        public override string GetDescription()
        {
            return invert ? "Target is NOT Alive" : "Target is Alive";
        }

        private bool IsAlive()
        {
            if (NetworkManager.Singleton == null)
            {
                return true;
            }
            var owner = GetOwner();
            if (owner == null || targetKey == null) return false;

            object targetVal = targetKey.GetValueObject();
            if (targetVal == null) return false;

            HealthComponent healthComponent = null;

            if (targetVal is GameObject go)
                healthComponent = go.GetComponent<HealthComponent>();
            else if (targetVal is HealthComponent hc)
                healthComponent = hc;
            else if (targetVal is Component comp)
                healthComponent = comp.GetComponent<HealthComponent>();

            if (healthComponent == null) return false;

            return healthComponent.IsAlive;
        }
    }
}
