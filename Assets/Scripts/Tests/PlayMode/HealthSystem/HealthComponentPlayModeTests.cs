using NUnit.Framework;
using SteelSurge.Core.Network.HealthSystem.Components;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace SteelSurge.Tests.PlayMode.HealthSystem
{
    /// <summary>
    /// PlayMode тесты для HealthComponent.
    /// Требуют игрового контекста для работы IsServer и NetworkBehaviour.
    /// </summary>
    [TestFixture]
    public class HealthComponentPlayModeTests
    {
        private GameObject _gameObject;
        private HealthComponent _healthComponent;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("HealthComponentTest");
            _healthComponent = _gameObject.AddComponent<HealthComponent>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void MaxHealth_ReturnsSerializedMaxHealth()
        {
            Assert.AreEqual(100f, _healthComponent.MaxHealth, 0.01f);
        }

        [Test]
        public void CurrentHealth_ReturnsZero_WhenNotSpawned()
        {
            Assert.AreEqual(0f, _healthComponent.CurrentHealth, 0.01f);
        }

        [UnityTest]
        public IEnumerator IsAlive_ReturnsFalse_WhenNotSpawned()
        {
            yield return null;
            Assert.IsFalse(_healthComponent.IsAlive);
        }

        [UnityTest]
        public IEnumerator IsDead_ReturnsTrue_WhenNotSpawned()
        {
            yield return null;
            Assert.IsTrue(_healthComponent.IsDead);
        }
    }
}
