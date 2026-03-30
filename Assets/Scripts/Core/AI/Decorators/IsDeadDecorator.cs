using RenownedGames.AITree;
using RenownedGames.Apex;
using SteelSurge.Core.Network.HealthSystem.Components;
using System;
using Unity.Netcode;
using UnityEngine;

namespace SteelSurge.Core.AI.Decorators
{
    [NodeContent("Decorators/Is Dead", "Check: is target dead")]
    public class IsDeadDecorator : ObserverDecorator
    {
        [Title("Blackboard")]
        [Tooltip("Target key (GameObject or HealthComponent)")]
        [SerializeField]
        [NonLocal]
        private Key targetKey;

        [Title("Settings")]
        [Tooltip("Invert check: target is NOT dead")]
        [SerializeField]
        private bool invert = false;

        private bool _lastIsDead;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _lastIsDead = IsDead();
        }

        public override event Action OnValueChange;

        public override bool CalculateResult()
        {
            bool isDead = IsDead();

            if (_lastIsDead != isDead)
            {
                _lastIsDead = isDead;
                OnValueChange?.Invoke();
            }

            return invert ? !isDead : isDead;
        }

        protected override void OnFlowUpdate() { }

        public override string GetDescription()
        {
            return invert ? "Target is NOT Dead" : "Target is Dead";
        }

        private bool IsDead()
        {
            if (NetworkManager.Singleton == null)
            {
                return false;
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

            return healthComponent.IsDead;
        }
    }
}
