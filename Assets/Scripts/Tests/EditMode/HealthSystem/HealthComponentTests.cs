using NUnit.Framework;
using SteelSurge.Core.Network.HealthSystem.Models;

namespace SteelSurge.Tests.EditMode.HealthSystem
{
    /// <summary>
    /// Тесты для логики Health (модель данных).
    /// HealthComponent требует сетевого контекста (IsServer), поэтому тестируется отдельно.
    /// </summary>
    [TestFixture]
    public class HealthLogicTests
    {
        [Test]
        public void TakeDamage_ReducesHealthCorrectly()
        {
            Health health = new Health(100f);
            health.TakeDamage(30f);

            Assert.AreEqual(70f, health.GetCurrentHealth(), 0.01f);
        }

        [Test]
        public void TakeDamage_WhenDamageExceedsHealth_SetsToZero()
        {
            Health health = new Health(50f);
            health.TakeDamage(100f);

            Assert.AreEqual(0f, health.GetCurrentHealth(), 0.01f);
        }

        [Test]
        public void TakeDamage_WithZeroDamage_DoesNotChangeHealth()
        {
            Health health = new Health(100f);
            health.TakeDamage(0f);

            Assert.AreEqual(100f, health.GetCurrentHealth(), 0.01f);
        }

        [Test]
        public void Heal_WhenHealExceedsMaxHealth_ClampsToMax()
        {
            Health health = new Health(100f);
            health.Heal(50f);

            Assert.AreEqual(100f, health.GetCurrentHealth(), 0.01f);
        }

        [Test]
        public void Heal_WithNegativeHeal_ClampsToZero()
        {
            Health health = new Health(50f);
            health.Heal(-100f);

            Assert.AreEqual(0f, health.GetCurrentHealth(), 0.01f);
        }

        [Test]
        public void IsAlive_WhenHealthAboveZero_ReturnsTrue()
        {
            Health health = new Health(1f);
            Assert.IsTrue(health.IsAlive);
        }

        [Test]
        public void IsAlive_WhenHealthIsZero_ReturnsFalse()
        {
            Health health = new Health(0f);
            Assert.IsFalse(health.IsAlive);
        }

        [Test]
        public void IsDead_WhenHealthIsZero_ReturnsTrue()
        {
            Health health = new Health(0f);
            Assert.IsTrue(health.IsDead);
        }

        [Test]
        public void IsDead_WhenHealthAboveZero_ReturnsFalse()
        {
            Health health = new Health(1f);
            Assert.IsFalse(health.IsDead);
        }
    }
}
