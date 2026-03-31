using UnityEngine;
using Sirenix.OdinInspector;

namespace SteelSurge.Core
{
    public abstract class ScriptableBaseObject : ScriptableObject
    {
        [SerializeField, BoxGroup("Info")] private Sprite _icon;
        [SerializeField, BoxGroup("Info")] private string _title;
        [SerializeField, BoxGroup("Info"), TextArea] private string _description;

        public Sprite Icon => _icon;
        public string Title => _title;
        public string Description => _description;
    }
}
