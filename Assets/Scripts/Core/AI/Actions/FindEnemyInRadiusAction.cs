using RenownedGames.AITree;
using RenownedGames.Apex;
using SteelSurge.Core.Network;
using UnityEngine;

namespace SteelSurge.Core
{
    public class FindEnemyInRadiusAction : TaskNode
    {
        [Title("Node")]
        [SerializeField]
        private float searchRadius = 15f;

        [Title("Blackboard")]
        [SerializeField]
        private Key targetKey;

        protected override State OnUpdate()
        {

            Debug.Log("Finding enemy in radius...");

            Transform ownerTransform = GetOwner().transform;
            Collider[] colliders = Physics.OverlapSphere(ownerTransform.position, searchRadius);

            foreach (Collider collider in colliders)
            {
                if (collider.gameObject == GetOwner().gameObject)
                {
                    continue; // Skip self
                }
                if (collider.TryGetComponent(out IOwnershipObject ownershipObject))
                {
                    Debug.Log($"Found object: {collider.gameObject.name}, IsEnemy: {ownershipObject.IsEnemy}");
                    if (ownershipObject.IsEnemy)
                    {
                        if (targetKey is TransformKey transformKey)
                        {
                            transformKey.SetValue(collider.transform);
                            return State.Success;
                        }
                        else if (targetKey is Vector3Key vector3Key)
                        {
                            vector3Key.SetValue(collider.transform.position);
                            return State.Success;
                        }

                        return State.Failure;
                    }
                }
            }

            return State.Failure;
        }
    }
}
