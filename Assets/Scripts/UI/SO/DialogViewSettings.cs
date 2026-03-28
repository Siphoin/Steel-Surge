using SteelSurge.Main.Configs;
using UnityEngine;

namespace SteelSurge.UI.SO
{
    [CreateAssetMenu(fileName = "DialogViewSettings", menuName = "SteelSurge/UI/Dialog View Settings")]
    public class DialogViewSettings : ScriptableConfig
    {
        [SerializeField] private float _delaySeconds = 0.05f;

        public float DelaySeconds => _delaySeconds;
    }
}