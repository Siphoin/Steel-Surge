using Unity.Netcode;

namespace SteelSurge.Network.Handlers
{
    public interface INetworkHandler
    {
        void StartHost();
        void StartClient();
        void StartServer();
        void Shutdown();
        public T GetSubHandler<T>();
    }
}