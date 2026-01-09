# StayAlive - Test Framework

Unity Test Framework setup for the StayAlive survival roguelike FPS.

## Directory Structure

```
Assets/Tests/
├── EditMode/                    # Unit tests (run in Editor)
│   ├── EditModeTests.asmdef
│   ├── InventorySlotTests.cs    # Inventory slot struct tests
│   └── CraftingRecipeTests.cs   # Recipe validation tests
└── PlayMode/                    # Integration tests (run in Play Mode)
    ├── PlayModeTests.asmdef
    └── PlayerStatsPlayModeTests.cs  # PlayerStats component tests
```

## Running Tests

### In Unity Editor

1. **Open Test Runner**: Window → General → Test Runner
2. **EditMode Tab**: Click "Run All" for unit tests
3. **PlayMode Tab**: Click "Run All" for integration tests

### Command Line (requires Unity Pro)

```bash
# Edit Mode tests
Unity.exe -runTests -batchmode -projectPath . -testPlatform EditMode -testResults test-results-editmode.xml

# Play Mode tests
Unity.exe -runTests -batchmode -projectPath . -testPlatform PlayMode -testResults test-results-playmode.xml
```

## Test Types

### EditMode Tests
- Run without entering Play Mode
- Fast execution
- Best for: Pure logic, data validation, ScriptableObjects

### PlayMode Tests
- Run in actual Play Mode
- Can test MonoBehaviours, coroutines, physics
- Best for: Component behavior, multi-frame logic, integration

## Writing New Tests

### EditMode Example
```csharp
using NUnit.Framework;

[TestFixture]
public class MyEditModeTests
{
    [Test]
    public void MyMethod_WhenCondition_ExpectedResult()
    {
        // Arrange
        var myClass = new MyClass();
        
        // Act
        var result = myClass.DoSomething();
        
        // Assert
        Assert.AreEqual(expected, result);
    }
}
```

### PlayMode Example
```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

public class MyPlayModeTests
{
    [UnityTest]
    public IEnumerator MyComponent_WhenAction_ExpectedBehavior()
    {
        // Arrange
        var go = new GameObject();
        var component = go.AddComponent<MyComponent>();
        
        // Act
        yield return new WaitForSeconds(0.1f);
        
        // Assert
        Assert.IsNotNull(component);
        
        // Cleanup
        Object.Destroy(go);
    }
}
```

## Best Practices

1. **Arrange-Act-Assert**: Structure tests clearly
2. **One assertion per test** (when practical)
3. **Cleanup resources**: Use `[TearDown]` or finally blocks
4. **Descriptive names**: `MethodName_Condition_ExpectedResult`
5. **Test edge cases**: null inputs, boundaries, error conditions

## Network Testing Notes

`PlayerStats` and other `NetworkBehaviour` classes have limited testability without a `NetworkManager`. The sample PlayMode tests verify:
- Component existence
- Default serialized values
- Properties are accessible

Full network integration tests require:
- NetworkManager setup
- Host/client spawning
- Consider using Unity's Multiplayer Test Framework

## Next Steps

After verifying this framework works:
1. **Test Design**: `/bmad-bmgd-workflows-gametest-test-design`
2. **Automate Tests**: `/bmad-bmgd-workflows-gametest-automate`
3. **Performance Tests**: `/bmad-bmgd-workflows-gametest-performance`
