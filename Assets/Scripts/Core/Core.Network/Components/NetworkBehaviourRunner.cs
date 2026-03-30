using RenownedGames.AITree;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace SteelSurge.Core.Network.Components
{
    [RequireComponent(typeof(BehaviourRunner))]
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkBehaviourRunner : NetworkBehaviour
    {
        [SerializeField, ReadOnly]  private BehaviourRunner _behaviourRunner;
        [SerializeField, ReadOnly] private NetworkObject _networkObject;
        private void LateUpdate()
        {
            if (!IsSpawned) return;

            bool isOwner = _networkObject.IsOwner;

            if (isOwner && !_behaviourRunner.enabled)
            {
                _behaviourRunner.enabled = true;
            }
            else if (!isOwner && _behaviourRunner.enabled)
            {
                _behaviourRunner.enabled = false;
            }
        }

        private void OnValidate()
        {
            if (!_networkObject)
            {
                _networkObject = GetComponent<NetworkObject>();
            }

            if (!_behaviourRunner)
            {
                _behaviourRunner = GetComponent<BehaviourRunner>();
            }
        }
    }
}
