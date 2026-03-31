using NUnit.Framework;
using SteelSurge.Network.Handlers;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace SteelSurge.Tests.PlayMode.Network
{
    /// <summary>
    /// PlayMode тесты для NetworkHandler.
    /// Требуют игрового контекста для работы NetworkManager и MonoBehaviour.
    /// </summary>
    [TestFixture]
    public class NetworkHandlerPlayModeTests
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
            _networkHandler.Shutdown();
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void GetSubHandler_WithoutInitialization_ReturnsNull()
        {
            var result = _networkHandler.GetSubHandler<SubNetworkHandler>();
            Assert.IsNull(result);
        }

        [Test]
        public void SubHandlers_InitiallyEmpty()
        {
            Assert.IsEmpty(_networkHandler.SubHandlers);
        }

        [UnityTest]
        public IEnumerator Shutdown_StopsNetworkManager()
        {
            yield return null;
            
            _networkHandler.Shutdown();
            
            Assert.IsNull(NetworkManager.Singleton);
        }

        [Test]
        public void TestApprovalCheck_WithPlayerName_ReturnsApproved()
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
    }
}
