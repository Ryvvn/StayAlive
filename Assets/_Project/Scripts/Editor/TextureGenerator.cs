using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureGenerator : EditorWindow
{
    [MenuItem("Tools/StayAlive/Generate Terrain Textures")]
    public static void ShowWindow()
    {
        GetWindow<TextureGenerator>("Texture Generator");
    }

    private Color _baseColor = new Color(0.3f, 0.5f, 0.2f); // Grass green
    private Color _noiseColor = new Color(0.4f, 0.6f, 0.3f);
    private int _size = 512;
    private float _noiseScale = 20f;

    private void OnGUI()
    {
        GUILayout.Label("Terrain Texture Generator", EditorStyles.boldLabel);
        
        _baseColor = EditorGUILayout.ColorField("Base Color", _baseColor);
        _noiseColor = EditorGUILayout.ColorField("Noise Color", _noiseColor);
        _size = EditorGUILayout.IntField("Size", _size);
        _noiseScale = EditorGUILayout.FloatField("Noise Scale", _noiseScale);

        if (GUILayout.Button("Generate Grass Texture"))
        {
            GenerateTexture("Terrain_Grass");
        }
        
        if (GUILayout.Button("Generate Dirt Texture"))
        {
            _baseColor = new Color(0.4f, 0.3f, 0.2f);
            _noiseColor = new Color(0.5f, 0.4f, 0.3f);
            GenerateTexture("Terrain_Dirt");
        }
    }

    private void GenerateTexture(string name)
    {
        Texture2D texture = new Texture2D(_size, _size);
        Texture2D normalMap = new Texture2D(_size, _size);
        
        for (int y = 0; y < _size; y++)
        {
            for (int x = 0; x < _size; x++)
            {
                float xCoord = (float)x / _size * _noiseScale;
                float yCoord = (float)y / _size * _noiseScale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                
                // Diffuse
                Color finalColor = Color.Lerp(_baseColor, _noiseColor, sample);
                texture.SetPixel(x, y, finalColor);
                
                // Normal (Simple Sobel-like approximation)
                float sampleR = Mathf.PerlinNoise(xCoord + 0.01f, yCoord);
                float sampleU = Mathf.PerlinNoise(xCoord, yCoord + 0.01f);
                float dX = sampleR - sample;
                float dY = sampleU - sample;
                
                Vector3 normal = new Vector3(-dX * 2f, -dY * 2f, 1f).normalized;
                Color normalColor = new Color(normal.x * 0.5f + 0.5f, normal.y * 0.5f + 0.5f, normal.z * 0.5f + 0.5f);
                normalMap.SetPixel(x, y, normalColor);
            }
        }
        
        texture.Apply();
        normalMap.Apply();
        
        SaveTexture(texture, name + "_Diff");
        SaveTexture(normalMap, name + "_Norm");
        
        AssetDatabase.Refresh();
    }

    private void SaveTexture(Texture2D tex, string name)
    {
        string path = "Assets/_Project/Art/Textures/Terrain/";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path + name + ".png", bytes);
        Debug.Log("Saved " + name + " to " + path);
    }
}
