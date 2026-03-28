using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SteelSurge.Main
{
    public abstract class ScriptableObjectIdentity : SerializedScriptableObject, IIdentity
    {
        [SerializeField, ReadOnly] private string _guidObject = Guid.NewGuid().ToString();

        public string GUID => _guidObject;
    }
}
