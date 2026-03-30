using Unity.Netcode;
using UnityEngine;

namespace SteelSurge.Core.Network
{
    public struct Health : INetworkSerializable
    {
        private float CurrentValue;
        public float MaxValue;
        private float v;

        public Health(float v) : this()
        {
            this.v = v;
        }

        public bool IsDead => CurrentValue <= 0;
        public bool IsAlive => CurrentValue > 0;
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
