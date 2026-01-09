using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using Unity.Netcode;
using System.IO;

/// <summary>
/// Creates enemy prefabs for testing.
/// </summary>
public class EnemyPrefabCreator : Editor
{
    private static string PrefabPath = "Assets/_Project/Prefabs/Enemies/";
    
    [MenuItem("Tools/StayAlive/Create Enemy Prefabs")]
    public static void CreateEnemyPrefabs()
    {
        if (!Directory.Exists(PrefabPath))
        {
            Directory.CreateDirectory(PrefabPath);
        }
        
        // Create enemy types
        CreateEnemy("Rusher", Color.red, 50f, 10f, 5f, 1.2f);
        CreateEnemy("Tank", new Color(0.4f, 0.2f, 0.6f), 150f, 20f, 3f, 0.6f);
        CreateEnemy("Ranged", Color.green, 30f, 8f, 8f, 1f);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("[EnemyPrefabCreator] Enemy prefabs created at: " + PrefabPath);
        Debug.Log("Remember to assign these to WaveManager!");
    }
    
    private static void CreateEnemy(string name, Color color, float health, float damage, float speed, float scale)
    {
        // Create body
        GameObject enemy = new GameObject(name);
        
        // Body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(enemy.transform);
        body.transform.localPosition = new Vector3(0, 1f, 0);
        body.transform.localScale = new Vector3(scale, scale, scale);
        
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        body.GetComponent<Renderer>().sharedMaterial = mat;
        
        // Eyes
        GameObject eyes = new GameObject("Eyes");
        eyes.transform.SetParent(enemy.transform);
        eyes.transform.localPosition = new Vector3(0, 1.5f, 0.3f * scale);
        
        for (int i = -1; i <= 1; i += 2)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Eye";
            eye.transform.SetParent(eyes.transform);
            eye.transform.localPosition = new Vector3(i * 0.15f * scale, 0, 0);
            eye.transform.localScale = Vector3.one * 0.1f * scale;
            eye.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Standard")) { color = Color.white };
            DestroyImmediate(eye.GetComponent<Collider>());
        }
        
        // Add components
        enemy.AddComponent<NetworkObject>();
        
        // NavMeshAgent
        NavMeshAgent agent = enemy.AddComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.angularSpeed = 120f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 1.5f;
        agent.radius = 0.5f * scale;
        agent.height = 2f * scale;
        
        // EnemyAI
        EnemyAI ai = enemy.AddComponent<EnemyAI>();
        SerializedObject aiSO = new SerializedObject(ai);
        aiSO.FindProperty("_moveSpeed").floatValue = speed;
        aiSO.FindProperty("_attackDamage").floatValue = damage;
        aiSO.FindProperty("_detectionRange").floatValue = 20f;
        aiSO.FindProperty("_attackRange").floatValue = 2f;
        aiSO.ApplyModifiedProperties();
        
        // EnemyStats
        EnemyStats stats = enemy.AddComponent<EnemyStats>();
        SerializedObject statsSO = new SerializedObject(stats);
        statsSO.FindProperty("_maxHealth").floatValue = health;
        statsSO.ApplyModifiedProperties();
        
        // Collider
        CapsuleCollider col = enemy.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 1f, 0);
        col.radius = 0.5f * scale;
        col.height = 2f * scale;
        
        // Save prefab
        string path = PrefabPath + name + ".prefab";
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(enemy, path);
        DestroyImmediate(enemy);
        
        Debug.Log($"Created: {path}");
    }
    
    [MenuItem("Tools/StayAlive/Assign Enemies to WaveManager")]
    public static void AssignEnemiesToWaveManager()
    {
        WaveManager wm = FindObjectOfType<WaveManager>();
        if (wm == null)
        {
            Debug.LogError("No WaveManager found in scene!");
            return;
        }
        
        SerializedObject so = new SerializedObject(wm);
        
        // Load prefabs
        GameObject rusher = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath + "Rusher.prefab");
        GameObject tank = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath + "Tank.prefab");
        GameObject ranged = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath + "Ranged.prefab");
        
        if (rusher != null)
        {
            so.FindProperty("_rusherPrefab").objectReferenceValue = rusher;
            Debug.Log("Assigned Rusher prefab");
        }
        if (tank != null)
        {
            so.FindProperty("_tankPrefab").objectReferenceValue = tank;
            Debug.Log("Assigned Tank prefab");
        }
        if (ranged != null)
        {
            so.FindProperty("_rangedPrefab").objectReferenceValue = ranged;
            Debug.Log("Assigned Ranged prefab");
        }
        
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(wm);
        
        Debug.Log("Enemy prefabs assigned to WaveManager!");
    }
}
