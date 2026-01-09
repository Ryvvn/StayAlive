using NUnit.Framework;

namespace StayAlive.Tests.EditMode
{
    /// <summary>
    /// Unit tests for the Inventory.InventorySlot struct.
    /// Tests basic slot operations without requiring network spawning.
    /// </summary>
    [TestFixture]
    public class InventorySlotTests
    {
        #region Empty Slot Tests
        
        [Test]
        public void EmptySlot_HasNegativeItemId()
        {
            // Arrange & Act
            var slot = Inventory.InventorySlot.Empty;
            
            // Assert
            Assert.AreEqual(-1, slot.ItemId, "Empty slot should have ItemId of -1");
        }
        
        [Test]
        public void EmptySlot_HasZeroQuantity()
        {
            // Arrange & Act
            var slot = Inventory.InventorySlot.Empty;
            
            // Assert
            Assert.AreEqual(0, slot.Quantity, "Empty slot should have Quantity of 0");
        }
        
        [Test]
        public void EmptySlot_IsEmpty_ReturnsTrue()
        {
            // Arrange
            var slot = Inventory.InventorySlot.Empty;
            
            // Act & Assert
            Assert.IsTrue(slot.IsEmpty, "Empty slot IsEmpty should return true");
        }
        
        #endregion
        
        #region Slot With Items Tests
        
        [Test]
        public void SlotWithItem_IsEmpty_ReturnsFalse()
        {
            // Arrange
            var slot = new Inventory.InventorySlot(1, 10);
            
            // Act & Assert
            Assert.IsFalse(slot.IsEmpty, "Slot with positive ItemId and Quantity should not be empty");
        }
        
        [Test]
        public void SlotWithZeroQuantity_IsEmpty_ReturnsTrue()
        {
            // Arrange
            var slot = new Inventory.InventorySlot(1, 0);
            
            // Act & Assert
            Assert.IsTrue(slot.IsEmpty, "Slot with zero quantity should be empty");
        }
        
        [Test]
        public void SlotWithNegativeItemId_IsEmpty_ReturnsTrue()
        {
            // Arrange
            var slot = new Inventory.InventorySlot(-1, 10);
            
            // Act & Assert
            Assert.IsTrue(slot.IsEmpty, "Slot with negative ItemId should be empty");
        }
        
        [Test]
        public void SlotConstructor_SetsCorrectValues()
        {
            // Arrange & Act
            var slot = new Inventory.InventorySlot(5, 25);
            
            // Assert
            Assert.AreEqual(5, slot.ItemId, "ItemId should match constructor value");
            Assert.AreEqual(25, slot.Quantity, "Quantity should match constructor value");
        }
        
        #endregion
        
        #region Equality Tests
        
        [Test]
        public void Equals_SameSlotsAreEqual()
        {
            // Arrange
            var slot1 = new Inventory.InventorySlot(1, 10);
            var slot2 = new Inventory.InventorySlot(1, 10);
            
            // Act & Assert
            Assert.IsTrue(slot1.Equals(slot2), "Slots with same ItemId and Quantity should be equal");
        }
        
        [Test]
        public void Equals_DifferentItemIdsAreNotEqual()
        {
            // Arrange
            var slot1 = new Inventory.InventorySlot(1, 10);
            var slot2 = new Inventory.InventorySlot(2, 10);
            
            // Act & Assert
            Assert.IsFalse(slot1.Equals(slot2), "Slots with different ItemIds should not be equal");
        }
        
        [Test]
        public void Equals_DifferentQuantitiesAreNotEqual()
        {
            // Arrange
            var slot1 = new Inventory.InventorySlot(1, 10);
            var slot2 = new Inventory.InventorySlot(1, 20);
            
            // Act & Assert
            Assert.IsFalse(slot1.Equals(slot2), "Slots with different Quantities should not be equal");
        }
        
        #endregion
    }
}
