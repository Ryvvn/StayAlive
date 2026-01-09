using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.Netcode;

/// <summary>
/// Editor tool to create basic building pieces.
/// </summary>
public class BuildingPieceCreator : Editor
{
    private static string PiecePath = "Assets/_Project/ScriptableObjects/Buildings/";
    private static string PrefabPath = "Assets/_Project/Prefabs/Buildings/";
    
    [MenuItem("Tools/StayAlive/Create Basic Building Pieces")]
    public static void CreateBuildingPieces()
    {
        if (!Directory.Exists(PiecePath)) Directory.CreateDirectory(PiecePath);
        if (!Directory.Exists(PrefabPath)) Directory.CreateDirectory(PrefabPath);
        
        // Load materials for items
        var woodItem = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/_Project/ScriptableObjects/Items/Item_WoodPlank.asset");
        var stoneItem = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/_Project/ScriptableObjects/Items/Item_StoneBrick.asset");
        
        // Create building pieces
        CreatePiece("Wooden Wall", "A basic wooden wall", BuildingPiece.BuildingCategory.Structure, 
            new BuildingPiece.BuildingCost[] { new() { Item = woodItem, Quantity = 5 } },
            new Vector3(2, 3, 0.2f));
        
        CreatePiece("Stone Wall", "A sturdy stone wall", BuildingPiece.BuildingCategory.Structure,
            new BuildingPiece.BuildingCost[] { new() { Item = stoneItem, Quantity = 8 } },
            new Vector3(2, 3, 0.3f), 200f);
        
        CreatePiece("Wooden Floor", "A wooden floor tile", BuildingPiece.BuildingCategory.Foundation,
            new BuildingPiece.BuildingCost[] { new() { Item = woodItem, Quantity = 4 } },
            new Vector3(2, 0.1f, 2), 80f, true);
        
        CreatePiece("Stone Floor", "A stone floor tile", BuildingPiece.BuildingCategory.Foundation,
            new BuildingPiece.BuildingCost[] { new() { Item = stoneItem, Quantity = 6 } },
            new Vector3(2, 0.15f, 2), 150f, true);
        
        CreatePiece("Wooden Door", "A wooden door", BuildingPiece.BuildingCategory.Structure,
            new BuildingPiece.BuildingCost[] { new() { Item = woodItem, Quantity = 4 } },
            new Vector3(1, 2.5f, 0.15f));
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("[BuildingPieceCreator] Created basic building pieces at: " + PiecePath);
    }
    
    private static void CreatePiece(string name, string desc, BuildingPiece.BuildingCategory category,
        BuildingPiece.BuildingCost[] costs, Vector3 size, float health = 100f, bool isFoundation = false)
    {
        string path = PiecePath + "Building_" + name.Replace(" ", "") + ".asset";
        
        BuildingPiece piece = AssetDatabase.LoadAssetAtPath<BuildingPiece>(path);
        if (piece == null)
        {
            piece = ScriptableObject.CreateInstance<BuildingPiece>();
            AssetDatabase.CreateAsset(piece, path);
        }
        
        piece.PieceName = name;
        piece.Description = desc;
        piece.Category = category;
        piece.Costs = new System.Collections.Generic.List<BuildingPiece.BuildingCost>(costs);
        piece.GridSize = size;
        piece.MaxHealth = health;
        piece.IsFoundation = isFoundation;
        piece.SnapToGrid = true;
        piece.GridSnapSize = 1f;
        piece.CanRotate = true;
        piece.RotationStep = 90f;
        
        // Create basic prefab
        piece.Prefab = CreateBuildingPrefab(name, size, isFoundation);
        
        EditorUtility.SetDirty(piece);
    }
    
    private static GameObject CreateBuildingPrefab(string name, Vector3 size, bool isFoundation)
    {
        string prefabPath = PrefabPath + name.Replace(" ", "") + ".prefab";
        
        // Check if prefab exists
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null) return existingPrefab;
        
        // Create new prefab
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.localScale = size;
        
        // Adjust pivot for walls (center at bottom)
        if (!isFoundation)
        {
            var mesh = go.GetComponent<MeshFilter>();
            // We'll just offset the visual, the collider is fine
        }
        
        // Add components
        go.AddComponent<BuildingHealth>();
        go.AddComponent<NetworkObject>();
        
        // Set layer
        go.layer = LayerMask.NameToLayer("Default"); // Change to "Building" if layer exists
        
        // Color based on type
        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = name.Contains("Wood") ? new Color(0.6f, 0.4f, 0.2f) : new Color(0.5f, 0.5f, 0.5f);
            renderer.material = mat;
        }
        
        // Save prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        DestroyImmediate(go);
        
        Debug.Log("Created prefab: " + prefabPath);
        return prefab;
    }
}
