namespace SteelSurge.Core.Network
{
    public interface IOwnershipObject
    {
        bool IsEnemy { get; }
        bool IsAlly { get; }
    }
}
