using NUnit.Framework;
using SteelSurge.Main;
using SteelSurge.Network.Configs;
using SteelSurge.Network.Handlers;
using UnityEngine;

namespace SteelSurge.Tests.EditMode.Network
{
    /// <summary>
    /// Тесты для NetworkHandler (без запуска сети).
    /// </summary>
    [TestFixture]
    public class NetworkHandlerTests
    {
        private GameObject _gameObject;
        private NetworkHandler _networkHandler;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("NetworkHandlerTest");
            _networkHandler = _gameObject.AddComponent<NetworkHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void Initialize_SetsSignalBusAndConfig()
        {
            // Arrange
            var signalBus = new SignalBus();
            var mockConfig = ScriptableObject.CreateInstance<NetworkHandlerConfig>();

            // Act
            _networkHandler.Initialize(signalBus, mockConfig);

            // Assert
            Assert.IsNotNull(_networkHandler.SignalBus);
            Assert.IsNotNull(_networkHandler.Config);
            Object.DestroyImmediate(mockConfig);
        }

        [Test]
        public void GetSubHandler_Initially_ReturnsNull()
        {
            var result = _networkHandler.GetSubHandler<SubNetworkHandler>();
            Assert.IsNull(result);
        }

        [Test]
        public void SubHandlers_InitiallyEmpty()
        {
            Assert.IsEmpty(_networkHandler.SubHandlers);
        }

        [Test]
        public void TestApprovalCheck_ReturnsApprovedResponse()
        {
            var response = _networkHandler.TestApprovalCheck(1, "TestPlayer");

            Assert.IsTrue(response.Approved);
            Assert.IsTrue(response.CreatePlayerObject);
            Assert.IsFalse(response.Pending);
        }

        [Test]
        public void TestApprovalCheck_WithEmptyName_ReturnsApproved()
        {
            var response = _networkHandler.TestApprovalCheck(2, "");

            Assert.IsTrue(response.Approved);
            Assert.IsTrue(response.CreatePlayerObject);
            Assert.IsFalse(response.Pending);
        }

        [Test]
        public void TestApprovalCheck_WithNullName_ReturnsApproved()
        {
            var response = _networkHandler.TestApprovalCheck(3, null);

            Assert.IsTrue(response.Approved);
            Assert.IsTrue(response.CreatePlayerObject);
            Assert.IsFalse(response.Pending);
        }
    }
}
