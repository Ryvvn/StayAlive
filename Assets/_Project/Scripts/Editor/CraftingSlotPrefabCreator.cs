using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Creates the Recipe Slot and Ingredient Slot prefabs for CraftingUI.
/// </summary>
public class CraftingSlotPrefabCreator : Editor
{
    private static string PrefabPath = "Assets/_Project/Prefabs/UI/";
    
    [MenuItem("Tools/StayAlive/Create Crafting Slot Prefabs")]
    public static void CreateSlotPrefabs()
    {
        // Ensure directory exists
        if (!System.IO.Directory.Exists(PrefabPath))
        {
            System.IO.Directory.CreateDirectory(PrefabPath);
        }
        
        CreateRecipeSlotPrefab();
        CreateIngredientSlotPrefab();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("[CraftingSlotPrefabCreator] Prefabs created at: " + PrefabPath);
    }
    
    private static void CreateRecipeSlotPrefab()
    {
        // Root object
        GameObject root = new GameObject("RecipeSlot");
        RectTransform rootRT = root.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(200, 50);
        
        // Background
        Image bg = root.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        
        // Button
        Button btn = root.AddComponent<Button>();
        btn.targetGraphic = bg;
        
        // Add RecipeSlotUI component
        RecipeSlotUI slotUI = root.AddComponent<RecipeSlotUI>();
        
        // Icon
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(root.transform);
        RectTransform iconRT = iconGO.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0, 0);
        iconRT.anchorMax = new Vector2(0, 1);
        iconRT.pivot = new Vector2(0, 0.5f);
        iconRT.anchoredPosition = new Vector2(5, 0);
        iconRT.sizeDelta = new Vector2(40, -10);
        Image iconImg = iconGO.AddComponent<Image>();
        iconImg.color = Color.white;
        
        // Name Text
        GameObject nameGO = new GameObject("NameText");
        nameGO.transform.SetParent(root.transform);
        RectTransform nameRT = nameGO.AddComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0, 0);
        nameRT.anchorMax = new Vector2(1, 1);
        nameRT.offsetMin = new Vector2(50, 5);
        nameRT.offsetMax = new Vector2(-5, -5);
        TextMeshProUGUI nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
        nameTMP.text = "Recipe Name";
        nameTMP.fontSize = 16;
        nameTMP.alignment = TextAlignmentOptions.MidlineLeft;
        nameTMP.color = Color.white;
        
        // Wire up RecipeSlotUI references via SerializedObject
        SerializedObject so = new SerializedObject(slotUI);
        so.FindProperty("_icon").objectReferenceValue = iconImg;
        so.FindProperty("_nameText").objectReferenceValue = nameTMP;
        so.FindProperty("_button").objectReferenceValue = btn;
        so.FindProperty("_background").objectReferenceValue = bg;
        so.ApplyModifiedProperties();
        
        // Save as prefab
        string path = PrefabPath + "RecipeSlot.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        
        Debug.Log("Created: " + path);
    }
    
    private static void CreateIngredientSlotPrefab()
    {
        // Root object
        GameObject root = new GameObject("IngredientSlot");
        RectTransform rootRT = root.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(180, 40);
        
        // Background
        Image bg = root.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
        
        // Horizontal layout
        HorizontalLayoutGroup hlg = root.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.spacing = 10;
        hlg.padding = new RectOffset(5, 5, 5, 5);
        
        // Icon
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(root.transform);
        RectTransform iconRT = iconGO.AddComponent<RectTransform>();
        iconRT.sizeDelta = new Vector2(30, 30);
        Image iconImg = iconGO.AddComponent<Image>();
        iconImg.color = Color.white;
        LayoutElement iconLE = iconGO.AddComponent<LayoutElement>();
        iconLE.minWidth = 30;
        iconLE.preferredWidth = 30;
        
        // Text (Name + Count)
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(root.transform);
        RectTransform textRT = textGO.AddComponent<RectTransform>();
        TextMeshProUGUI textTMP = textGO.AddComponent<TextMeshProUGUI>();
        textTMP.text = "Item Name (0/5)";
        textTMP.fontSize = 14;
        textTMP.alignment = TextAlignmentOptions.MidlineLeft;
        textTMP.color = Color.white;
        LayoutElement textLE = textGO.AddComponent<LayoutElement>();
        textLE.flexibleWidth = 1;
        
        // Save as prefab
        string path = PrefabPath + "IngredientSlot.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        
        Debug.Log("Created: " + path);
    }
}
