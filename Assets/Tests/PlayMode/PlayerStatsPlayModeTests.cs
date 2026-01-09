using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace StayAlive.Tests.PlayMode
{
    /// <summary>
    /// Play Mode integration tests for PlayerStats.
    /// Tests component behavior in runtime without networking.
    /// Note: Full network tests require NetworkManager setup.
    /// </summary>
    public class PlayerStatsPlayModeTests
    {
        private GameObject _playerGO;
        private PlayerStats _playerStats;
        
        #region Setup/Teardown
        
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Create a player GameObject with PlayerStats
            _playerGO = new GameObject("TestPlayer");
            _playerStats = _playerGO.AddComponent<PlayerStats>();
            
            yield return null; // Wait for component initialization
        }
        
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // Cleanup
            if (_playerGO != null)
            {
                Object.Destroy(_playerGO);
            }
            
            yield return null;
        }
        
        #endregion
        
        #region Component Tests
        
        [UnityTest]
        public IEnumerator PlayerStats_WhenCreated_ComponentExists()
        {
            // Assert
            Assert.IsNotNull(_playerStats, "PlayerStats component should exist");
            
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator PlayerStats_HasNetworkBehaviourBase()
        {
            // Assert - PlayerStats inherits from NetworkBehaviour
            Assert.IsTrue(_playerStats is Unity.Netcode.NetworkBehaviour, 
                "PlayerStats should inherit from NetworkBehaviour");
            
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator PlayerStats_MaxHealth_IsAccessible()
        {
            // Assert - Check that MaxHealth property is accessible
            float maxHealth = _playerStats.MaxHealth;
            Assert.GreaterOrEqual(maxHealth, 0f, "MaxHealth should be non-negative");
            
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator PlayerStats_MaxHunger_IsAccessible()
        {
            // Assert
            float maxHunger = _playerStats.MaxHunger;
            Assert.GreaterOrEqual(maxHunger, 0f, "MaxHunger should be non-negative");
            
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator PlayerStats_MaxThirst_IsAccessible()
        {
            // Assert
            float maxThirst = _playerStats.MaxThirst;
            Assert.GreaterOrEqual(maxThirst, 0f, "MaxThirst should be non-negative");
            
            yield return null;
        }
        
        #endregion
        
        #region Default Value Tests
        
        [UnityTest]
        public IEnumerator PlayerStats_DefaultMaxHealth_IsNonZero()
        {
            // Assert - Typical survival game health
            Assert.Greater(_playerStats.MaxHealth, 0f, "Default MaxHealth should be greater than 0");
            
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator PlayerStats_DefaultMaxHunger_IsNonZero()
        {
            // Assert
            Assert.Greater(_playerStats.MaxHunger, 0f, "Default MaxHunger should be greater than 0");
            
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator PlayerStats_DefaultMaxThirst_IsNonZero()
        {
            // Assert
            Assert.Greater(_playerStats.MaxThirst, 0f, "Default MaxThirst should be greater than 0");
            
            yield return null;
        }
        
        #endregion
        
        #region Multiple Frame Tests
        
        [UnityTest]
        public IEnumerator PlayerStats_SurvivesMultipleFrames()
        {
            // Act - Let multiple frames pass
            yield return new WaitForSeconds(0.1f);
            
            // Assert - Component should still be valid
            Assert.IsNotNull(_playerStats, "PlayerStats should survive multiple frames");
            Assert.IsFalse(_playerStats == null, "PlayerStats should not be destroyed");
        }
        
        #endregion
    }
}
