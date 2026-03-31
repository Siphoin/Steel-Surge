using NUnit.Framework;
using SteelSurge.Network.Handlers;
using UnityEngine;

namespace SteelSurge.Tests.EditMode.Network
{
    /// <summary>
    /// Тесты для SubNetworkHandler (базовый класс).
    /// </summary>
    [TestFixture]
    public class SubNetworkHandlerTests
    {
        private GameObject _gameObject;
        private TestSubNetworkHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("SubNetworkHandlerTest");
            _handler = _gameObject.AddComponent<TestSubNetworkHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void OnNetworkSpawn_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _handler.CallOnNetworkSpawn());
        }

        [Test]
        public void OnNetworkDespawn_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _handler.CallOnNetworkDespawn());
        }

        [Test]
        public void IsServer_WhenNotSpawned_ReturnsFalse()
        {
            Assert.IsFalse(_handler.IsServer);
        }

        [Test]
        public void IsClient_WhenNotSpawned_ReturnsFalse()
        {
            Assert.IsFalse(_handler.IsClient);
        }

        [Test]
        public void IsHost_WhenNotSpawned_ReturnsFalse()
        {
            Assert.IsFalse(_handler.IsHost);
        }
    }

    /// <summary>
    /// Тестовая реализация SubNetworkHandler.
    /// </summary>
    public class TestSubNetworkHandler : SubNetworkHandler
    {
        public void CallOnNetworkSpawn() => base.OnNetworkSpawn();
        public void CallOnNetworkDespawn() => base.OnNetworkDespawn();
    }
}
