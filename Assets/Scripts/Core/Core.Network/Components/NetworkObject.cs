using UniRx;
using Zenject;
using SteelSurge.Network.Handlers;
using SteelSurge.Core.Network.Handlers;

namespace SteelSurge.Core.Network.Components
{
    public abstract class NetworkObject : SteelSurge.Network.Components.NetworkObject
    {
        [Inject] private INetworkHandler _networkHandler;

        public SessionPlayer SessionPlayer
        {
            get
            {
                SessionPlayerHandler sessionPlayerHandler = _networkHandler.GetSubHandler<SessionPlayerHandler>();
                if (sessionPlayerHandler != null)
                {
                    return sessionPlayerHandler.Get(OwnerClientId);
                }

                return SessionPlayer.Empty;
            }
        }

   
    }
}
