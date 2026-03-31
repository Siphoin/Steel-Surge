namespace SteelSurge.Core.Network
{
    public interface IOwnershipObject
    {
        bool IsEnemy { get; }
        bool IsAlly { get; }
        void ChangeOwnership(ulong newOwnerClientId);
        void RemoveOwnership();
    }
}
