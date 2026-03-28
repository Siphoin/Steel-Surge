using SteelSurge.Network.Models;

namespace SteelSurge.Network.Signals
{
    public class RoomPlayersUpdatedSignal
    {
        public NetworkGuid RoomId { get; private set; }

        public RoomPlayersUpdatedSignal(NetworkGuid roomId)
        {
            RoomId = roomId;
        }
    }
}