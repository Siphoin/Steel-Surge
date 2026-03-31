using NUnit.Framework;
using SteelSurge.Core.Network;
using SteelSurge.Network.Models;
using Unity.Collections;
using Unity.Netcode;
using System;

namespace SteelSurge.Tests.EditMode.Network
{
    /// <summary>
    /// Тесты для SessionPlayer (модель данных).
    /// </summary>
    [TestFixture]
    public class SessionPlayerTests
    {
        [Test]
        public void Empty_ReturnsPlayerWithDefaultValues()
        {
            SessionPlayer empty = SessionPlayer.Empty;

            Assert.AreEqual(0, empty.ClientId);
            Assert.AreEqual(0, empty.CurrentEnergy);
            Assert.AreEqual(0, empty.TeamColor);
            Assert.AreEqual(default(FixedString64Bytes), empty.PlayerName);
        }

        [Test]
        public void IsEmpty_WhenPlayerIsEmpty_ReturnsTrue()
        {
            SessionPlayer empty = SessionPlayer.Empty;

            Assert.IsTrue(empty.IsEmpty);
        }

        [Test]
        public void IsEmpty_WhenPlayerHasValues_ReturnsFalse()
        {
            SessionPlayer player = new SessionPlayer
            {
                ClientId = 1,
                CurrentEnergy = 100,
                TeamColor = 2,
                PlayerName = new FixedString64Bytes("TestPlayer")
            };

            Assert.IsFalse(player.IsEmpty);
        }

        [Test]
        public void Constructor_FromNetworkPlayer_SetsCorrectValues()
        {
            NetworkPlayer networkPlayer = new NetworkPlayer
            {
                ClientId = 42,
                Name = new FixedString64Bytes("Hero"),
                InstanceId = new NetworkGuid(Guid.NewGuid()),
                CurrentRoomId = new NetworkGuid(Guid.NewGuid())
            };

            SessionPlayer sessionPlayer = new SessionPlayer(networkPlayer, teamColor: 3, 2);

            Assert.AreEqual(42, sessionPlayer.ClientId);
            Assert.AreEqual(100, sessionPlayer.CurrentEnergy);
            Assert.AreEqual(3, sessionPlayer.TeamColor);
            Assert.AreEqual(new FixedString64Bytes("Hero"), sessionPlayer.PlayerName);
        }

        [Test]
        public void Equals_CompareSameClientId_ReturnsTrue()
        {
            SessionPlayer player1 = new SessionPlayer
            {
                ClientId = 1,
                CurrentEnergy = 100,
                TeamColor = 2,
                PlayerName = new FixedString64Bytes("Player1")
            };

            SessionPlayer player2 = new SessionPlayer
            {
                ClientId = 1,
                CurrentEnergy = 50,
                TeamColor = 3,
                PlayerName = new FixedString64Bytes("Player2")
            };

            Assert.IsTrue(player1.Equals(player2));
        }

        [Test]
        public void Equals_CompareDifferentClientId_ReturnsFalse()
        {
            SessionPlayer player1 = new SessionPlayer
            {
                ClientId = 1,
                CurrentEnergy = 100,
                TeamColor = 2,
                PlayerName = new FixedString64Bytes("Player1")
            };

            SessionPlayer player2 = new SessionPlayer
            {
                ClientId = 2,
                CurrentEnergy = 100,
                TeamColor = 2,
                PlayerName = new FixedString64Bytes("Player1")
            };

            Assert.IsFalse(player1.Equals(player2));
        }

        [Test]
        public void Equals_CompareWithObject_SameClientId_ReturnsTrue()
        {
            SessionPlayer player1 = new SessionPlayer
            {
                ClientId = 1,
                CurrentEnergy = 100,
                TeamColor = 2,
                PlayerName = new FixedString64Bytes("Player1")
            };

            SessionPlayer player2 = new SessionPlayer
            {
                ClientId = 1,
                CurrentEnergy = 50,
                TeamColor = 3,
                PlayerName = new FixedString64Bytes("Player2")
            };

            Assert.IsTrue(player1.Equals((object)player2));
        }

        [Test]
        public void Equals_CompareWithObject_DifferentType_ReturnsFalse()
        {
            SessionPlayer player = new SessionPlayer
            {
                ClientId = 1,
                CurrentEnergy = 100,
                TeamColor = 2,
                PlayerName = new FixedString64Bytes("Player1")
            };

            Assert.IsFalse(player.Equals("string"));
        }

        [Test]
        public void Equals_CompareWithNull_ReturnsFalse()
        {
            SessionPlayer player = new SessionPlayer
            {
                ClientId = 1,
                CurrentEnergy = 100,
                TeamColor = 2,
                PlayerName = new FixedString64Bytes("Player1")
            };

            Assert.IsFalse(player.Equals(null));
        }

        [Test]
        public void GetHashCode_ReturnsClientIdHashCode()
        {
            SessionPlayer player = new SessionPlayer
            {
                ClientId = 42,
                CurrentEnergy = 100,
                TeamColor = 2,
                PlayerName = new FixedString64Bytes("Player")
            };

            Assert.AreEqual(player.ClientId.GetHashCode(), player.GetHashCode());
        }

        [Test]
        public void Equals_CompareWithEmpty_ReturnsFalse()
        {
            SessionPlayer player = new SessionPlayer
            {
                ClientId = 1,
                CurrentEnergy = 100,
                TeamColor = 2,
                PlayerName = new FixedString64Bytes("TestPlayer")
            };

            Assert.IsFalse(player.Equals(SessionPlayer.Empty));
        }
    }
}
