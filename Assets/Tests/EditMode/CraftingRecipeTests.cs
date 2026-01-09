using NUnit.Framework;
using UnityEngine;

namespace StayAlive.Tests.EditMode
{
    /// <summary>
    /// Unit tests for CraftingRecipe validation.
    /// Tests recipe configuration without requiring runtime components.
    /// </summary>
    [TestFixture]
    public class CraftingRecipeTests
    {
        #region Recipe Validation Tests
        
        [Test]
        public void NewRecipe_DefaultOutputQuantity_IsOne()
        {
            // Arrange
            var recipe = ScriptableObject.CreateInstance<CraftingRecipe>();
            
            try
            {
                // Assert
                Assert.AreEqual(1, recipe.OutputQuantity, "Default output quantity should be 1");
            }
            finally
            {
                // Cleanup
                Object.DestroyImmediate(recipe);
            }
        }
        
        [Test]
        public void NewRecipe_DefaultCraftingStation_NotRequired()
        {
            // Arrange
            var recipe = ScriptableObject.CreateInstance<CraftingRecipe>();
            
            try
            {
                // Assert
                Assert.IsFalse(recipe.RequiresCraftingStation, "Default recipe should not require crafting station");
            }
            finally
            {
                // Cleanup
                Object.DestroyImmediate(recipe);
            }
        }
        
        [Test]
        public void NewRecipe_DefaultCategory_IsTools()
        {
            // Arrange
            var recipe = ScriptableObject.CreateInstance<CraftingRecipe>();
            
            try
            {
                // Assert
                Assert.AreEqual(CraftingRecipe.RecipeCategory.Tools, recipe.Category, "Default category should be Tools");
            }
            finally
            {
                // Cleanup
                Object.DestroyImmediate(recipe);
            }
        }
        
        [Test]
        public void NewRecipe_IngredientsListInitialized()
        {
            // Arrange
            var recipe = ScriptableObject.CreateInstance<CraftingRecipe>();
            
            try
            {
                // Assert
                Assert.IsNotNull(recipe.Ingredients, "Ingredients list should be initialized");
                Assert.AreEqual(0, recipe.Ingredients.Count, "Ingredients list should start empty");
            }
            finally
            {
                // Cleanup
                Object.DestroyImmediate(recipe);
            }
        }
        
        [Test]
        public void CanCraft_NullInventory_ReturnsFalse()
        {
            // Arrange
            var recipe = ScriptableObject.CreateInstance<CraftingRecipe>();
            var database = ScriptableObject.CreateInstance<ItemDatabase>();
            
            try
            {
                // Act
                bool result = recipe.CanCraft(null, database);
                
                // Assert
                Assert.IsFalse(result, "CanCraft should return false with null inventory");
            }
            finally
            {
                // Cleanup
                Object.DestroyImmediate(recipe);
                Object.DestroyImmediate(database);
            }
        }
        
        [Test]
        public void CanCraft_NullDatabase_ReturnsFalse()
        {
            // Arrange
            var recipe = ScriptableObject.CreateInstance<CraftingRecipe>();
            
            try
            {
                // Act - Using null for both since Inventory is a MonoBehaviour
                bool result = recipe.CanCraft(null, null);
                
                // Assert
                Assert.IsFalse(result, "CanCraft should return false with null database");
            }
            finally
            {
                // Cleanup
                Object.DestroyImmediate(recipe);
            }
        }
        
        #endregion
        
        #region Recipe Category Tests
        
        [Test]
        public void RecipeCategory_HasExpectedValues()
        {
            // Assert all expected categories exist
            Assert.IsTrue(System.Enum.IsDefined(typeof(CraftingRecipe.RecipeCategory), "Tools"));
            Assert.IsTrue(System.Enum.IsDefined(typeof(CraftingRecipe.RecipeCategory), "Weapons"));
            Assert.IsTrue(System.Enum.IsDefined(typeof(CraftingRecipe.RecipeCategory), "Ammo"));
            Assert.IsTrue(System.Enum.IsDefined(typeof(CraftingRecipe.RecipeCategory), "Food"));
            Assert.IsTrue(System.Enum.IsDefined(typeof(CraftingRecipe.RecipeCategory), "Building"));
            Assert.IsTrue(System.Enum.IsDefined(typeof(CraftingRecipe.RecipeCategory), "Upgrades"));
        }
        
        #endregion
    }
}
