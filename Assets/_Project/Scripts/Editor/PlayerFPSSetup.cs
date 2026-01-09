#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Unity.Netcode;

/// <summary>
/// Editor utility to set up the Player prefab with First Person Controller components.
/// Access via: Tools > StayAlive > Setup Player FPS Controller
/// </summary>
public class PlayerFPSSetup : Editor
{
    [MenuItem("Tools/StayAlive/Setup Player FPS Controller")]
    public static void SetupPlayerFPSController()
    {
        // Get selected GameObject
        GameObject selected = Selection.activeGameObject;
        
        if (selected == null)
        {
            EditorUtility.DisplayDialog("No Selection", 
                "Please select the Player prefab or GameObject in the scene to set up.", "OK");
            return;
        }
        
        // Confirm
        if (!EditorUtility.DisplayDialog("Setup FPS Controller",
            $"This will add FPS controller components to '{selected.name}':\n\n" +
            "• CharacterController\n" +
            "• FirstPersonController\n" +
            "• PlayerLook\n" +
            "• Camera (if not exists)\n\n" +
            "Continue?", "Yes", "Cancel"))
        {
            return;
        }
        
        Undo.RecordObject(selected, "Setup FPS Controller");
        
        // Add CharacterController if missing
        if (selected.GetComponent<CharacterController>() == null)
        {
            var cc = Undo.AddComponent<CharacterController>(selected);
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0, 0.9f, 0);
            Debug.Log("[PlayerFPSSetup] Added CharacterController");
        }
        
        // Add NetworkObject if missing
        if (selected.GetComponent<NetworkObject>() == null)
        {
            Undo.AddComponent<NetworkObject>(selected);
            Debug.Log("[PlayerFPSSetup] Added NetworkObject");
        }
        
        // Add ClientNetworkTransform if missing
        if (selected.GetComponent<ClientNetworkTransform>() == null)
        {
            Undo.AddComponent<ClientNetworkTransform>(selected);
            Debug.Log("[PlayerFPSSetup] Added ClientNetworkTransform");
        }
        
        // Create Camera setup if no camera exists
        Camera existingCamera = selected.GetComponentInChildren<Camera>();
        if (existingCamera == null)
        {
            // Create camera holder
            GameObject cameraHolder = new GameObject("CameraHolder");
            cameraHolder.transform.SetParent(selected.transform);
            cameraHolder.transform.localPosition = new Vector3(0, 1.6f, 0); // Eye height
            cameraHolder.transform.localRotation = Quaternion.identity;
            Undo.RegisterCreatedObjectUndo(cameraHolder, "Create Camera Holder");
            
            // Create camera
            GameObject cameraObj = new GameObject("PlayerCamera");
            cameraObj.transform.SetParent(cameraHolder.transform);
            cameraObj.transform.localPosition = Vector3.zero;
            cameraObj.transform.localRotation = Quaternion.identity;
            
            var cam = cameraObj.AddComponent<Camera>();
            cam.nearClipPlane = 0.1f;
            cam.fieldOfView = 70f;
            
            cameraObj.AddComponent<AudioListener>();
            
            Undo.RegisterCreatedObjectUndo(cameraObj, "Create Player Camera");
            
            
            Debug.Log("[PlayerFPSSetup] Created Camera setup");
        }
        
        EditorUtility.SetDirty(selected);
        
        Debug.Log("[PlayerFPSSetup] Player FPS Controller setup complete!");
        EditorUtility.DisplayDialog("Setup Complete",
            $"FPS Controller components added to '{selected.name}'.\n\n" +
            "Remember to:\n" +
            "1. Apply changes to prefab if editing a prefab instance\n" +
            "2. Assign this prefab to NetworkGameManager's Player Prefab field", "OK");
    }
    
    [MenuItem("Tools/StayAlive/Create New Player Prefab")]
    public static void CreateNewPlayerPrefab()
    {
        // Create player object
        GameObject player = new GameObject("Player");
        
        // Add capsule visual
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = "PlayerModel";
        capsule.transform.SetParent(player.transform);
        capsule.transform.localPosition = new Vector3(0, 1f, 0);
        
        // Remove collider from visual (CharacterController has its own)
        DestroyImmediate(capsule.GetComponent<Collider>());
        
        // Now setup FPS components
        Selection.activeGameObject = player;
        SetupPlayerFPSController();
        
        // Create prefab folder if needed
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Player"))
        {
            AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Player");
        }
        
        // Save as prefab
        string prefabPath = "Assets/_Project/Prefabs/Player/Player.prefab";
        
        // Check for existing
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            if (!EditorUtility.DisplayDialog("Prefab Exists",
                "Player.prefab already exists. Overwrite?", "Overwrite", "Cancel"))
            {
                DestroyImmediate(player);
                return;
            }
        }
        
        PrefabUtility.SaveAsPrefabAsset(player, prefabPath);
        DestroyImmediate(player);
        
        // Select the prefab
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        Debug.Log($"[PlayerFPSSetup] Created Player prefab at: {prefabPath}");
        EditorUtility.DisplayDialog("Prefab Created",
            $"Player prefab created at:\n{prefabPath}", "OK");
    }
}
#endif
