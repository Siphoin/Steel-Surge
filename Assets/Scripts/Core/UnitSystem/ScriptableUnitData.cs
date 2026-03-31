using UnityEngine;
using Sirenix.OdinInspector;

namespace SteelSurge.Core.UnitSystem
{
    [CreateAssetMenu(fileName = "New Unit", menuName = "Steel Surge/Unit System/Unit SO")]
    public class ScriptableUnitData : ScriptableBaseObject
    {
        [BoxGroup("Settings")] [SerializeField] private float _health = 100f;
        [BoxGroup("Settings")] [SerializeField] private float _speed = 5f;
        [BoxGroup("Settings")] [SerializeField] private float _damage = 10f;
        [BoxGroup("Settings")] [SerializeField] private float _attackRange = 2f;
        [BoxGroup("Settings")] [SerializeField] private float _attackSpeed = 1f;
        public float Health => _health;
        public float Speed => _speed;
        public float Damage => _damage;
        public float AttackRange => _attackRange;
        public float AttackSpeed => _attackSpeed;
    }
}
