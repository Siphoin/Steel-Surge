using Unity.Netcode;
using UnityEngine;
using UniRx;
using System;
using SteelSurge.Core.Network.HealthSystem.Models;

namespace SteelSurge.Core.Network.HealthSystem.Components
{
    public class HealthComponent : SteelSurge.Core.Network.Components.NetworkObject
    {
        [SerializeField] private float _maxHealth = 100f;

        private NetworkVariable<Health> _health = new(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Server);
        private readonly ReactiveProperty<Health> _healthReactive = new();

        public IReadOnlyReactiveProperty<Health> HealthProperty => _healthReactive;
        public IObservable<Health> OnHealthChanged => _healthReactive;
        public IObservable<Unit> OnDied => _healthReactive
            .Where(h => h.IsDead)
            .Select(_ => Unit.Default);

        public bool IsAlive => _health.Value.IsAlive;
        public bool IsDead => _health.Value.IsDead;
        public float CurrentHealth => _health.Value.GetCurrentHealth();
        public float MaxHealth => _maxHealth;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _health.Value = new Health(_maxHealth);
            _healthReactive.Value = _health.Value;
            _health.OnValueChanged += OnHealthValueChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _health.OnValueChanged -= OnHealthValueChanged;
        }

        public void TakeDamage(float damage)
        {
            if (!IsServer || damage <= 0 || IsDead)
                return;

            Health health = _health.Value;
            health.TakeDamage(damage);
            _health.Value = health;
            _healthReactive.Value = health;
        }

        public void Heal(float healAmount)
        {
            if (!IsServer || healAmount <= 0 || IsDead)
                return;

            Health health = _health.Value;
            health.Heal(healAmount);
            _health.Value = health;
            _healthReactive.Value = health;
        }

        public void SetHealth(float health)
        {
            if (!IsServer)
                return;

            Health newHealth = new Health(Mathf.Clamp(health, 0, _maxHealth));
            _health.Value = newHealth;
            _healthReactive.Value = newHealth;
        }

        private void OnHealthValueChanged(Health previous, Health current)
        {
            _healthReactive.Value = current;
        }
    }
}