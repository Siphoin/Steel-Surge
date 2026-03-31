using Zenject;
using SteelSurge.Network.Handlers;
using SteelSurge.Core.Network.Handlers;
using UnityEngine;
using Sirenix.OdinInspector;
namespace SteelSurge.Core.Network.Components
{
    public abstract class NetworkObject : SteelSurge.Network.Components.NetworkObject, IOwnershipObject
    {
        [Inject] private INetworkHandler _networkHandler;
        [SerializeField, ReadOnly] private Unity.Netcode.NetworkObject _networkObjectInternal;

        private SessionPlayerHandler SessionPlayerHandler
        {
            get
            {
                return _networkHandler?.GetSubHandler<SessionPlayerHandler>();
            }
        }

        public SessionPlayer LocalPlayer => SessionPlayerHandler.LocalPlayer;

        public SessionPlayer SessionPlayer
        {
            get
            {
                if (SessionPlayerHandler != null)
                {
                    return SessionPlayerHandler.Get(OwnerClientId);
                }

                return SessionPlayer.Empty;
            }
        }

        public bool IsEnemy => true;

        public bool IsAlly => LocalPlayer.IsAllyForPlayer(SessionPlayer);

        public void ChangeOwnership(ulong newOwnerClientId)
        {
            if (IsServer)
            {
                _networkObjectInternal.ChangeOwnership(newOwnerClientId);
            }
        }

        public void RemoveOwnership()
        {
            if (IsServer)
            {
                _networkObjectInternal.RemoveOwnership();
            }
        }

        private void OnValidate()
        {
            if (!_networkObjectInternal)
            {
                _networkObjectInternal = GetComponent<Unity.Netcode.NetworkObject>();
            }
        }
    }
}
