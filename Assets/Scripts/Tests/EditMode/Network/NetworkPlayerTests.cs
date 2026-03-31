using NUnit.Framework;
using SteelSurge.Network.Models;
using Unity.Collections;
using Unity.Netcode;
using System;

namespace SteelSurge.Tests.EditMode.Network
{
    /// <summary>
    /// Тесты для NetworkPlayer (модель данных).
    /// </summary>
    [TestFixture]
    public class NetworkPlayerTests
    {
        [Test]
        public void Empty_ReturnsPlayerWithDefaultValues()
        {
            NetworkPlayer empty = NetworkPlayer.Empty;

            Assert.AreEqual(0, empty.ClientId);
            Assert.AreEqual(default(FixedString64Bytes), empty.Name);
            Assert.AreEqual(default(NetworkGuid), empty.InstanceId);
            Assert.AreEqual(default(NetworkGuid), empty.CurrentRoomId);
        }

        [Test]
        public void IsEmpty_WhenPlayerIsEmpty_ReturnsTrue()
        {
            NetworkPlayer empty = NetworkPlayer.Empty;

            Assert.IsTrue(empty.IsEmpty);
        }

        [Test]
        public void IsEmpty_WhenPlayerHasValues_ReturnsFalse()
        {
            NetworkPlayer player = new NetworkPlayer
            {
                ClientId = 1,
                Name = new FixedString64Bytes("TestPlayer"),
                InstanceId = new NetworkGuid(Guid.NewGuid()),
                CurrentRoomId = new NetworkGuid(Guid.NewGuid())
            };

            Assert.IsFalse(player.IsEmpty);
        }

        [Test]
        public void Equals_CompareSameInstanceId_ReturnsTrue()
        {
            NetworkGuid guid = new NetworkGuid(Guid.NewGuid());

            NetworkPlayer player1 = new NetworkPlayer
            {
                ClientId = 1,
                Name = new FixedString64Bytes("Player1"),
                InstanceId = guid,
                CurrentRoomId = new NetworkGuid(Guid.NewGuid())
            };

            NetworkPlayer player2 = new NetworkPlayer
            {
                ClientId = 2,
                Name = new FixedString64Bytes("Player2"),
                InstanceId = guid,
                CurrentRoomId = new NetworkGuid(Guid.NewGuid())
            };

            Assert.IsTrue(player1.Equals(player2));
        }

        [Test]
        public void Equals_CompareDifferentInstanceId_ReturnsFalse()
        {
            NetworkPlayer player1 = new NetworkPlayer
            {
                ClientId = 1,
                Name = new FixedString64Bytes("Player1"),
                InstanceId = new NetworkGuid(Guid.NewGuid()),
                CurrentRoomId = new NetworkGuid(Guid.NewGuid())
            };

            NetworkPlayer player2 = new NetworkPlayer
            {
                ClientId = 1,
                Name = new FixedString64Bytes("Player1"),
                InstanceId = new NetworkGuid(Guid.NewGuid()),
                CurrentRoomId = new NetworkGuid(Guid.NewGuid())
            };

            Assert.IsFalse(player1.Equals(player2));
        }

        [Test]
        public void Equals_CompareWithEmpty_ReturnsFalse()
        {
            NetworkPlayer player = new NetworkPlayer
            {
                ClientId = 1,
                Name = new FixedString64Bytes("TestPlayer"),
                InstanceId = new NetworkGuid(Guid.NewGuid()),
                CurrentRoomId = new NetworkGuid(Guid.NewGuid())
            };

            Assert.IsFalse(player.Equals(NetworkPlayer.Empty));
        }
    }
}
