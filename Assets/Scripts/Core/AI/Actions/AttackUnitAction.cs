using RenownedGames.AITree;
using RenownedGames.Apex;
using UnityEngine;

namespace SteelSurge.Core.AI.Actions
{
    [NodeContent("Actions/Attack Unit", "Attack target unit with damage")]
    public class AttackUnitAction : TaskNode
    {
        [Title("Node")]
        [SerializeField]
        private float attackDamage = 10f;

        [Title("Blackboard")]
        [Tooltip("Target transform key")]
        [SerializeField]
        [NonLocal]
        private Key targetKey;

        protected override State OnUpdate()
        {
            Debug.Log("Attacking unit...");

            if (targetKey == null)
            {
                Debug.LogWarning("[AttackUnitAction] Target key is null");
                return State.Failure;
            }

            Transform target = null;
            if (targetKey is TransformKey transformKey)
            {
                target = transformKey.GetValue();
            }
            else if (targetKey is Vector3Key vector3Key)
            {
                Debug.LogWarning("[AttackUnitAction] Vector3Key not supported for attack");
                return State.Failure;
            }

            if (target == null)
            {
                Debug.LogWarning("[AttackUnitAction] Target is null");
                return State.Failure;
            }

            Debug.Log($"[AttackUnitAction] Attacking target: {target.gameObject.name}, Damage: {attackDamage}");

            // TODO: Реализовать логику атаки
            // - Проверка дистанции
            // - Нанесение урона
            // - Проигрывание анимации/эффектов

            return State.Running;
        }
    }
}
