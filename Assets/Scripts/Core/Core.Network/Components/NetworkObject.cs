using Zenject;
using SteelSurge.Network.Handlers;
using SteelSurge.Core.Network.Handlers;

namespace SteelSurge.Core.Network.Components
{
    public abstract class NetworkObject : SteelSurge.Network.Components.NetworkObject, IOwnershipObject
    {
        [Inject] private INetworkHandler _networkHandler;

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

        public bool IsEnemy => LocalPlayer.IsEnemyForPlayer(SessionPlayer);

        public bool IsAlly => LocalPlayer.IsAllyForPlayer(SessionPlayer);



   
    }
}
