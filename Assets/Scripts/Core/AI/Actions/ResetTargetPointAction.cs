using RenownedGames.AITree;
using RenownedGames.Apex;
using UnityEngine;

namespace SteelSurge.Core.AI.Actions
{
    [NodeContent("Actions/Reset Target Point", "Reset TargetPoint key to negative infinity")]
    public class ResetTargetPointAction : TaskNode
    {
        [Title("Blackboard")]
        [Tooltip("TargetPoint key (Vector3)")]
        [SerializeField]
        [NonLocal]
        private Key targetPointKey;

        protected override State OnUpdate()
        {

            if (targetPointKey is Vector3Key vector3Key)
            {
                vector3Key.SetValue(Vector3.negativeInfinity);
                return State.Success;
            }

            return State.Failure;
        }
    }
}
