using DG.Tweening;
using SteelSurge.Main.Configs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SteelSurge.UI.SO
{
    [CreateAssetMenu(fileName = "BlackFadeScreenViewSettings", menuName = "SteelSurge/UI/Black Fade Screen View Settings")]
    public class BlackFadeScreenViewSettings : ScriptableConfig
    {
        [SerializeField] private Ease _ease = Ease.Linear;
        [SerializeField, MinValue(0)] private float _speed = 3;

        public Ease Ease => _ease;
        public float Speed => _speed;
    }
}
