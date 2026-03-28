using SteelSurge.Network.Models;

namespace SteelSurge.Network.Signals
{
    public class NewChatMessageSignal
    {
        public NetworkMessage Message {  get; private set; }
        public NewChatMessageSignal(NetworkMessage message) => Message = message;
    }
}