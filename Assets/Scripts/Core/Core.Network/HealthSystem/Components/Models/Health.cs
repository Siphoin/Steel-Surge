using Unity.Netcode;
using UnityEngine;
using System;
namespace SteelSurge.Core.Network.HealthSystem.Models
{
    [Serializable]
    public struct Health : INetworkSerializable
    {
        private float CurrentValue;
        public float MaxValue;

        public bool IsDead => CurrentValue <= 0;
        public bool IsAlive => CurrentValue > 0;

        public Health(float health)
        {
            CurrentValue = health;
            MaxValue = health;
        }
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref CurrentValue);
            serializer.SerializeValue(ref MaxValue);
        }

        public float GetCurrentHealth()
        {
            return CurrentValue;
        }

        public void TakeDamage(float damage)
        {
            CurrentValue = Mathf.Clamp(CurrentValue - damage, 0, MaxValue);
        }

        public void Heal(float healAmount)
        {
            CurrentValue = Mathf.Clamp(CurrentValue + healAmount, 0, MaxValue);
        }
    }
}
