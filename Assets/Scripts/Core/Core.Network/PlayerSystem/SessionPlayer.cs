using SteelSurge.Network.Models;
using System;
using Unity.Collections;
using Unity.Netcode;

namespace SteelSurge.Core.Network
{
    [Serializable]
    public struct SessionPlayer : INetworkSerializable, IEquatable<SessionPlayer>
    {
        public int CurrentEnergy;
        public ulong ClientId;
        public byte TeamColor;
        public byte PlayerTeam;
        public FixedString64Bytes PlayerName;

        public static SessionPlayer Empty => new SessionPlayer
        {
            ClientId = 0,
            CurrentEnergy = 0,
            TeamColor = 0,
            PlayerName = default
        };

        public bool IsEmpty => Equals(Empty);

        public SessionPlayer(NetworkPlayer networkPlayer, byte teamColor, byte playerTeam)
        {
            CurrentEnergy = 100;
            ClientId = networkPlayer.ClientId;
            TeamColor = teamColor;
            PlayerName = networkPlayer.Name;
            PlayerTeam = playerTeam;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref CurrentEnergy);
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref TeamColor);
            serializer.SerializeValue(ref PlayerTeam);
            serializer.SerializeValue(ref PlayerName);
        }

        public bool Equals(SessionPlayer other)
        {
            return ClientId == other.ClientId;
        }

        public bool IsEnemyForPlayer(SessionPlayer other)
        {
            return PlayerTeam != other.PlayerTeam;
        }

        public bool IsAllyForPlayer(SessionPlayer other)
        {
            return PlayerTeam == other.PlayerTeam;
        }

        public override bool Equals(object obj)
        {
            return obj is SessionPlayer other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ClientId.GetHashCode();
        }
    }
}
