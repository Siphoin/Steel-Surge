using SteelSurge.Network.Models;
using Unity.Collections;
using Unity.Netcode;

namespace SteelSurge.Network.Handlers
{
    public interface INetworkRoomHandler : ISubNetworkHandler
    {
        NetworkList<NetworkRoom> ActiveRooms { get; }

        void RequestCreateRoom(FixedString128Bytes roomName, int maxPlayers);

        void RequestJoinRoom(NetworkGuid instanceId);

        void RequestLeaveRoom(NetworkGuid instanceId);
        void RequestJoinOrCreateRoom(FixedString128Bytes roomName, int maxPlayers);

        void RemoveRoom(NetworkGuid instanceId);
    }
}