using UnityEngine;

namespace SteelSurge.Core.UnitSystem.Handlers
{
    public interface IUnitAnimatorHandler
    {
        void SetBool(AnimatorBoolParam param, bool value);
        void PlayAttack();
    }
}
