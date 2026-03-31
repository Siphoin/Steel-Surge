using NUnit.Framework;
using SteelSurge.Main;
using SteelSurge.Network.Handlers;
using SteelSurge.Network.Models;
using SteelSurge.Network.Signals;
using UnityEngine;

namespace SteelSurge.Tests.EditMode.Network
{
    /// <summary>
    /// Тесты для NetworkPlayerHandler (базовая логика).
    /// Полноценное тестирование требует PlayMode из-за NetworkBehaviour.
    /// </summary>
    [TestFixture]
    public class NetworkPlayerHandlerTests
    {
        private GameObject _gameObject;
        private NetworkPlayerHandler _handler;
        private SignalBus _signalBus;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("NetworkPlayerHandlerTest");
            _handler = _gameObject.AddComponent<NetworkPlayerHandler>();
            _signalBus = new SignalBus();
            _handler.Initialize(_signalBus);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void GetPlayer_WhenNoPlayers_ReturnsEmpty()
        {
            var player = _handler.GetPlayer(1);

            Assert.IsTrue(player.IsEmpty);
        }

        [Test]
        public void SignalBus_AfterInitialize_ReturnsInjectedSignalBus()
        {
            Assert.AreEqual(_signalBus, _handler.SignalBus);
        }
    }
}
