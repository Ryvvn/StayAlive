using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.Netcode;

/// <summary>
/// Editor tool to create tower BuildingPiece assets for the unified building system.
/// </summary>
public class TowerBuildingCreator : Editor
{
    private static string BuildingPath = "Assets/_Project/ScriptableObjects/Buildings/";
    private static string PrefabPath = "Assets/_Project/Prefabs/Towers/";
    
    [MenuItem("Tools/StayAlive/Create Tower Building Pieces")]
    public static void CreateTowerBuildingPieces()
    {
        // Ensure directories exist
        if (!Directory.Exists(BuildingPath))
        {
            Directory.CreateDirectory(BuildingPath);
        }
        if (!Directory.Exists(PrefabPath))
        {
            Directory.CreateDirectory(PrefabPath);
        }
        
        // Create tower building pieces
        CreateTowerPiece("Basic Turret", "Standard auto-targeting turret", 100f, true, false,
            new Color(0.4f, 0.4f, 0.4f), BuildingPiece.BuildingCategory.Defense);
        
        CreateTowerPiece("Slow Tower", "Slows enemies in range", 80f, true, false,
            new Color(0.2f, 0.4f, 0.8f), BuildingPiece.BuildingCategory.Defense);
        
        CreateTowerPiece("Power Relay", "Extends power network range", 50f, false, true,
            new Color(0.2f, 0.8f, 0.8f), BuildingPiece.BuildingCategory.Utility);
        
        CreateTowerPiece("Mortar Tower", "Area damage tower", 150f, true, false,
            new Color(0.5f, 0.3f, 0.2f), BuildingPiece.BuildingCategory.Defense);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("[TowerBuildingCreator] Tower BuildingPieces created at: " + BuildingPath);
    }
    
    private static void CreateTowerPiece(string name, string description, float health, 
        bool isTower, bool isPowerRelay, Color color, BuildingPiece.BuildingCategory category)
    {
        // Create prefab first
        GameObject prefab = CreateTowerPrefab(name.Replace(" ", ""), color, isTower, isPowerRelay);
        
        // Create BuildingPiece asset
        BuildingPiece piece = ScriptableObject.CreateInstance<BuildingPiece>();
        piece.PieceName = name;
        piece.Description = description;
        piece.Category = category;
        piece.Prefab = prefab;
        piece.MaxHealth = health;
        piece.IsTower = isTower;
        piece.IsPowerRelay = isPowerRelay;
        piece.RequiresPower = true; // All towers require power
        piece.SnapToGrid = true;
        piece.GridSnapSize = 2f;
        piece.CanRotate = true;
        piece.RotationStep = 45f;
        
        string assetPath = BuildingPath + name.Replace(" ", "_") + ".asset";
        AssetDatabase.CreateAsset(piece, assetPath);
        
        Debug.Log($"Created: {assetPath}");
    }
    
    private static GameObject CreateTowerPrefab(string name, Color color, bool isTower, bool isPowerRelay)
    {
        // Create base
        GameObject tower = new GameObject(name);
        
        // Base platform
        GameObject basePlatform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        basePlatform.name = "Base";
        basePlatform.transform.SetParent(tower.transform);
        basePlatform.transform.localPosition = new Vector3(0, 0.25f, 0);
        basePlatform.transform.localScale = new Vector3(2f, 0.5f, 2f);
        basePlatform.GetComponent<Renderer>().sharedMaterial = CreateMaterial(color * 0.7f);
        
        // Tower body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(tower.transform);
        body.transform.localPosition = new Vector3(0, 1f, 0);
        body.transform.localScale = new Vector3(1f, 1.5f, 1f);
        body.GetComponent<Renderer>().sharedMaterial = CreateMaterial(color);
        
        // Turret head (if tower)
        if (isTower)
        {
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "TurretHead";
            head.transform.SetParent(tower.transform);
            head.transform.localPosition = new Vector3(0, 2f, 0);
            head.transform.localScale = new Vector3(0.8f, 0.4f, 1.2f);
            head.GetComponent<Renderer>().sharedMaterial = CreateMaterial(color * 1.2f);
            
            // Barrel
            GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "Barrel";
            barrel.transform.SetParent(head.transform);
            barrel.transform.localPosition = new Vector3(0, 0, 0.8f);
            barrel.transform.localRotation = Quaternion.Euler(90, 0, 0);
            barrel.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
            barrel.GetComponent<Renderer>().sharedMaterial = CreateMaterial(Color.gray);
        }
        
        // Add network object
        tower.AddComponent<NetworkObject>();
        
        // Add specific components
        if (isTower)
        {
            tower.AddComponent<TowerController>();
        }
        
        if (isPowerRelay)
        {
            tower.AddComponent<PowerRelay>();
            
            // Add radius indicator line renderer
            GameObject radiusIndicator = new GameObject("RadiusIndicator");
            radiusIndicator.transform.SetParent(tower.transform);
            radiusIndicator.transform.localPosition = Vector3.zero;
            LineRenderer lr = radiusIndicator.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startWidth = 0.1f;
            lr.endWidth = 0.1f;
            lr.loop = true;
        }
        
        // Add building health
        tower.AddComponent<BuildingHealth>();
        
        // Save prefab
        string prefabPath = PrefabPath + name + ".prefab";
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(tower, prefabPath);
        DestroyImmediate(tower);
        
        Debug.Log($"Created prefab: {prefabPath}");
        return savedPrefab;
    }
    
    private static Material CreateMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        return mat;
    }
}
