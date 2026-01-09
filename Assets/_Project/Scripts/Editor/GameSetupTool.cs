using UnityEngine;
using UnityEditor;
using Unity.Netcode;

/// <summary>
/// Master setup tool for StayAlive game scene.
/// Creates and configures all required managers and objects.
/// </summary>
public class GameSetupTool : Editor
{
    [MenuItem("Tools/StayAlive/Setup Game Scene")]
    public static void SetupGameScene()
    {
        Debug.Log("=== StayAlive Game Scene Setup ===");
        
        // 1. Create Managers Container
        GameObject managers = CreateOrFind("--- MANAGERS ---");
        
        // 2. NetworkManager
        SetupNetworkManager(managers.transform);
        
        // 3. GameManager
        SetupGameManager(managers.transform);
        
        // 4. DayNightManager
        SetupDayNightManager(managers.transform);
        
        // 5. WaveManager
        SetupWaveManager(managers.transform);
        
        // 6. ObjectPool
        SetupObjectPool(managers.transform);
        
        // 7. World Container
        GameObject world = CreateOrFind("--- WORLD ---");
        SetupWorldGenerator(world.transform);
        SetupPowerBase(world.transform);
        
        // 8. UI Container
        GameObject ui = CreateOrFind("--- UI ---");
        
        // 9. Lighting
        SetupLighting();
        
        Debug.Log("=== Scene Setup Complete! ===");
        Debug.Log("Next steps:");
        Debug.Log("1. Run Tools > StayAlive > Create Building Pieces");
        Debug.Log("2. Run Tools > StayAlive > Create Tower Building Pieces");
        Debug.Log("3. Run Tools > StayAlive > Create Basic Items and Recipes");
        Debug.Log("4. Create enemy prefabs and assign to WaveManager");
        Debug.Log("5. Create player prefab and assign to NetworkManager");
    }
    
    private static GameObject CreateOrFind(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null)
        {
            obj = new GameObject(name);
        }
        return obj;
    }
    
    private static void SetupNetworkManager(Transform parent)
    {
        if (FindObjectOfType<NetworkManager>() != null)
        {
            Debug.Log("NetworkManager already exists");
            return;
        }
        
        GameObject nm = new GameObject("NetworkManager");
        nm.transform.SetParent(parent);
        nm.AddComponent<NetworkManager>();
        Debug.Log("Created NetworkManager");
    }
    
    private static void SetupGameManager(Transform parent)
    {
        if (GameManager.Instance != null)
        {
            Debug.Log("GameManager already exists");
            return;
        }
        
        GameObject gm = new GameObject("GameManager");
        gm.transform.SetParent(parent);
        gm.AddComponent<GameManager>();
        gm.AddComponent<NetworkObject>();
        Debug.Log("Created GameManager");
    }
    
    private static void SetupDayNightManager(Transform parent)
    {
        if (DayNightManager.Instance != null)
        {
            Debug.Log("DayNightManager already exists");
            return;
        }
        
        GameObject dnm = new GameObject("DayNightManager");
        dnm.transform.SetParent(parent);
        dnm.AddComponent<DayNightManager>();
        dnm.AddComponent<NetworkObject>();
        Debug.Log("Created DayNightManager");
    }
    
    private static void SetupWaveManager(Transform parent)
    {
        if (WaveManager.Instance != null)
        {
            Debug.Log("WaveManager already exists");
            return;
        }
        
        GameObject wm = new GameObject("WaveManager");
        wm.transform.SetParent(parent);
        wm.AddComponent<WaveManager>();
        wm.AddComponent<NetworkObject>();
        
        // Create spawn points
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f;
            float distance = 40f;
            Vector3 pos = new Vector3(
                Mathf.Sin(angle * Mathf.Deg2Rad) * distance,
                0,
                Mathf.Cos(angle * Mathf.Deg2Rad) * distance
            );
            
            GameObject sp = new GameObject($"SpawnPoint_{i}");
            sp.transform.SetParent(wm.transform);
            sp.transform.position = pos;
            sp.transform.LookAt(Vector3.zero);
        }
        
        Debug.Log("Created WaveManager with 4 spawn points");
    }
    
    private static void SetupObjectPool(Transform parent)
    {
        if (ObjectPool.Instance != null)
        {
            Debug.Log("ObjectPool already exists");
            return;
        }
        
        GameObject op = new GameObject("ObjectPool");
        op.transform.SetParent(parent);
        op.AddComponent<ObjectPool>();
        Debug.Log("Created ObjectPool");
    }
    
    private static void SetupWorldGenerator(Transform parent)
    {
        var existing = FindObjectOfType<WorldGenerator>();
        if (existing != null)
        {
            Debug.Log("WorldGenerator already exists");
            return;
        }
        
        GameObject wg = new GameObject("WorldGenerator");
        wg.transform.SetParent(parent);
        wg.AddComponent<WorldGenerator>();
        wg.AddComponent<NetworkObject>();
        
        // Add NavMeshSurface if available
        var navMeshType = System.Type.GetType("Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation");
        if (navMeshType != null)
        {
            wg.AddComponent(navMeshType);
        }
        
        Debug.Log("Created WorldGenerator");
    }
    
    private static void SetupPowerBase(Transform parent)
    {
        if (PowerSystem.Instance != null)
        {
            Debug.Log("PowerSystem already exists");
            return;
        }
        
        // Create base tower
        GameObject powerBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        powerBase.name = "PowerBase";
        powerBase.transform.SetParent(parent);
        powerBase.transform.position = Vector3.zero;
        powerBase.transform.localScale = new Vector3(3f, 2f, 3f);
        powerBase.tag = "Tower";
        
        powerBase.AddComponent<PowerSystem>();
        powerBase.AddComponent<NetworkObject>();
        
        // Add radius indicator
        GameObject radiusObj = new GameObject("RadiusIndicator");
        radiusObj.transform.SetParent(powerBase.transform);
        radiusObj.transform.localPosition = Vector3.zero;
        LineRenderer lr = radiusObj.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.cyan;
        lr.endColor = Color.cyan;
        lr.startWidth = 0.2f;
        lr.endWidth = 0.2f;
        lr.loop = true;
        
        // Wire up reference
        var ps = powerBase.GetComponent<PowerSystem>();
        SerializedObject so = new SerializedObject(ps);
        so.FindProperty("_radiusIndicator").objectReferenceValue = lr;
        so.ApplyModifiedProperties();
        
        Debug.Log("Created PowerBase with PowerSystem");
    }
    
    private static void SetupLighting()
    {
        // Find or create directional light (sun)
        Light sun = null;
        foreach (var light in FindObjectsOfType<Light>())
        {
            if (light.type == LightType.Directional)
            {
                sun = light;
                break;
            }
        }
        
        if (sun == null)
        {
            GameObject sunObj = new GameObject("Sun");
            sun = sunObj.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(50, -30, 0);
        }
        
        sun.intensity = 1f;
        sun.shadows = LightShadows.Soft;
        
        Debug.Log("Configured lighting");
    }
    
    [MenuItem("Tools/StayAlive/Validate Scene Setup")]
    public static void ValidateSceneSetup()
    {
        Debug.Log("=== Validating Scene Setup ===");
        
        int issues = 0;
        
        // Check managers
        if (FindObjectOfType<NetworkManager>() == null) { Debug.LogWarning("❌ Missing: NetworkManager"); issues++; }
        else Debug.Log("✓ NetworkManager");
        
        if (GameManager.Instance == null) { Debug.LogWarning("❌ Missing: GameManager"); issues++; }
        else Debug.Log("✓ GameManager");
        
        if (DayNightManager.Instance == null) { Debug.LogWarning("❌ Missing: DayNightManager"); issues++; }
        else Debug.Log("✓ DayNightManager");
        
        if (WaveManager.Instance == null) { Debug.LogWarning("❌ Missing: WaveManager"); issues++; }
        else Debug.Log("✓ WaveManager");
        
        if (PowerSystem.Instance == null) { Debug.LogWarning("❌ Missing: PowerSystem"); issues++; }
        else Debug.Log("✓ PowerSystem");
        
        // Check player prefab
        var nm = FindObjectOfType<NetworkManager>();
        if (nm != null && nm.NetworkConfig.PlayerPrefab == null)
        {
            Debug.LogWarning("❌ NetworkManager: No Player Prefab assigned");
            issues++;
        }
        
        // Check enemy prefabs
        var wm = FindObjectOfType<WaveManager>();
        if (wm != null)
        {
            SerializedObject so = new SerializedObject(wm);
            if (so.FindProperty("_rusherPrefab").objectReferenceValue == null)
            {
                Debug.LogWarning("❌ WaveManager: No enemy prefabs assigned");
                issues++;
            }
        }
        
        if (issues == 0)
        {
            Debug.Log("=== All checks passed! ===");
        }
        else
        {
            Debug.LogWarning($"=== {issues} issues found ===");
        }
    }
}
