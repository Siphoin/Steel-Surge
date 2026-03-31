using Sirenix.OdinInspector;
using SteelSurge.Core.Network.Components;
using Unity.Netcode;
using UnityEngine;
using NetworkObject = SteelSurge.Core.Network.Components.NetworkObject;

namespace SteelSurge.Core.Network
{
    public class NetworkObjectOwnershipDebug : MonoBehaviour
    {
        [Header("References")]
        [Required]
        [SerializeField]
        private NetworkObject _networkObject;

        [Header("Debug Settings")]
        [SerializeField]
        private bool _showDebugButtons = true;
        [SerializeField]
        private ulong _targetClientId = 1000;

        [Button("Set Owner to Client ID")]
        [ShowIf("_showDebugButtons")]
        private void SetOwnerToClientId()
        {
            if (_networkObject == null)
            {
                Debug.LogWarning("[NetworkObjectOwnershipDebug] NetworkObject is null");
                return;
            }

            _networkObject.ChangeOwnership(_targetClientId);
            Debug.Log($"[NetworkObjectOwnershipDebug] Requested ownership change to {_targetClientId}");
        }

        [Button("Set Owner to Server")]
        [ShowIf("_showDebugButtons")]
        private void SetOwnerToServer()
        {
            if (_networkObject == null)
            {
                Debug.LogWarning("[NetworkObjectOwnershipDebug] NetworkObject is null");
                return;
            }

            _networkObject.ChangeOwnership(NetworkManager.ServerClientId);
            Debug.Log($"[NetworkObjectOwnershipDebug] Ownership changed to server");
        }

        [Button("Reset Ownership")]
        [ShowIf("_showDebugButtons")]
        private void ResetOwnership()
        {
            if (_networkObject == null)
            {
                Debug.LogWarning("[NetworkObjectOwnershipDebug] NetworkObject is null");
                return;
            }

            _networkObject.RemoveOwnership();
            Debug.Log("[NetworkObjectOwnershipDebug] Ownership reset");
        }

        [Button("Log Connected Clients")]
        [ShowIf("_showDebugButtons")]
        private void LogConnectedClients()
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogWarning("[NetworkObjectOwnershipDebug] NetworkManager is null");
                return;
            }

            Debug.Log($"[NetworkObjectOwnershipDebug] Connected clients ({NetworkManager.Singleton.ConnectedClientsIds.Count}):");
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                Debug.Log($"  - Client ID: {clientId}");
            }
        }

        private void OnValidate()
        {
            if (!_networkObject)
            {
                _networkObject = GetComponent<NetworkObject>();
            }
        }
    }
}
