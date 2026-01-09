using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor tool to create the Building UI Canvas.
/// </summary>
public class BuildingUICreator : Editor
{
    [MenuItem("Tools/StayAlive/Create Building UI")]
    public static void CreateBuildingUI()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("BuildingCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Main Panel (hidden by default)
        GameObject panel = CreatePanel(canvasGO.transform, "BuildingPanel");
        panel.SetActive(false);
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.1f, 0.15f);
        panelRT.anchorMax = new Vector2(0.9f, 0.85f);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        
        // Title
        GameObject title = CreateText(panel.transform, "Title", "BUILD MODE", 36, TextAlignmentOptions.Top);
        RectTransform titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 0.9f);
        titleRT.anchorMax = new Vector2(1, 1f);
        titleRT.offsetMin = Vector2.zero;
        titleRT.offsetMax = Vector2.zero;
        
        // Building Grid (Left Side)
        GameObject buildingList = CreatePanel(panel.transform, "BuildingList");
        RectTransform listRT = buildingList.GetComponent<RectTransform>();
        listRT.anchorMin = new Vector2(0.02f, 0.05f);
        listRT.anchorMax = new Vector2(0.48f, 0.88f);
        listRT.offsetMin = Vector2.zero;
        listRT.offsetMax = Vector2.zero;
        
        // ScrollView for buildings
        GameObject scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(buildingList.transform);
        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        RectTransform scrollRT = scrollView.GetComponent<RectTransform>();
        scrollRT.anchorMin = Vector2.zero;
        scrollRT.anchorMax = Vector2.one;
        scrollRT.offsetMin = new Vector2(10, 10);
        scrollRT.offsetMax = new Vector2(-10, -10);
        
        // Content container with grid layout
        GameObject content = new GameObject("Content");
        content.transform.SetParent(scrollView.transform);
        RectTransform contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = new Vector2(0, 400);
        
        GridLayoutGroup glg = content.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(120, 140);
        glg.spacing = new Vector2(10, 10);
        glg.padding = new RectOffset(10, 10, 10, 10);
        glg.childAlignment = TextAnchor.UpperLeft;
        
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        scrollRect.content = contentRT;
        scrollRect.viewport = scrollRT;
        
        // Details Panel (Right Side)
        GameObject details = CreatePanel(panel.transform, "DetailsPanel");
        RectTransform detailsRT = details.GetComponent<RectTransform>();
        detailsRT.anchorMin = new Vector2(0.52f, 0.05f);
        detailsRT.anchorMax = new Vector2(0.98f, 0.88f);
        detailsRT.offsetMin = Vector2.zero;
        detailsRT.offsetMax = Vector2.zero;
        
        // Selected Icon
        GameObject iconGO = new GameObject("SelectedIcon");
        iconGO.transform.SetParent(details.transform);
        Image iconImg = iconGO.AddComponent<Image>();
        iconImg.color = new Color(0.3f, 0.3f, 0.3f);
        RectTransform iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.1f, 0.6f);
        iconRT.anchorMax = new Vector2(0.4f, 0.95f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;
        
        // Selected Name
        GameObject nameGO = CreateText(details.transform, "SelectedName", "Select a Building", 28, TextAlignmentOptions.TopLeft);
        RectTransform nameRT = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0.45f, 0.8f);
        nameRT.anchorMax = new Vector2(0.95f, 0.95f);
        nameRT.offsetMin = Vector2.zero;
        nameRT.offsetMax = Vector2.zero;
        
        // Description
        GameObject descGO = CreateText(details.transform, "SelectedDescription", "Click a building to see details", 16, TextAlignmentOptions.TopLeft);
        RectTransform descRT = descGO.GetComponent<RectTransform>();
        descRT.anchorMin = new Vector2(0.45f, 0.6f);
        descRT.anchorMax = new Vector2(0.95f, 0.78f);
        descRT.offsetMin = Vector2.zero;
        descRT.offsetMax = Vector2.zero;
        
        // Cost Header
        CreateText(details.transform, "CostHeader", "COST:", 18, TextAlignmentOptions.TopLeft)
            .GetComponent<RectTransform>().anchorMin = new Vector2(0.1f, 0.45f);
        
        // Cost Text
        GameObject costGO = CreateText(details.transform, "SelectedCost", "- 5x Wood Plank\n- 3x Stone Brick", 16, TextAlignmentOptions.TopLeft);
        RectTransform costRT = costGO.GetComponent<RectTransform>();
        costRT.anchorMin = new Vector2(0.1f, 0.2f);
        costRT.anchorMax = new Vector2(0.9f, 0.44f);
        costRT.offsetMin = Vector2.zero;
        costRT.offsetMax = Vector2.zero;
        
        // Build Button
        GameObject buildBtn = CreateButton(details.transform, "BuildButton", "SELECT & BUILD");
        RectTransform btnRT = buildBtn.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.2f, 0.05f);
        btnRT.anchorMax = new Vector2(0.8f, 0.15f);
        btnRT.offsetMin = Vector2.zero;
        btnRT.offsetMax = Vector2.zero;
        
        // Close Button
        GameObject closeBtn = CreateButton(panel.transform, "CloseButton", "X");
        closeBtn.GetComponent<Image>().color = new Color(0.7f, 0.2f, 0.2f);
        RectTransform closeBtnRT = closeBtn.GetComponent<RectTransform>();
        closeBtnRT.anchorMin = new Vector2(0.93f, 0.91f);
        closeBtnRT.anchorMax = new Vector2(0.99f, 0.99f);
        closeBtnRT.offsetMin = Vector2.zero;
        closeBtnRT.offsetMax = Vector2.zero;
        
        // Instructions
        GameObject instr = CreateText(panel.transform, "Instructions", "Press B to toggle | 1-9 to quick select | R to rotate | Right-click to cancel", 14, TextAlignmentOptions.Bottom);
        RectTransform instrRT = instr.GetComponent<RectTransform>();
        instrRT.anchorMin = new Vector2(0, 0);
        instrRT.anchorMax = new Vector2(1, 0.05f);
        instrRT.offsetMin = Vector2.zero;
        instrRT.offsetMax = Vector2.zero;
        
        // Add BuildingUI component
        BuildingUI buildingUI = canvasGO.AddComponent<BuildingUI>();
        
        // Wire references
        SerializedObject so = new SerializedObject(buildingUI);
        so.FindProperty("_panel").objectReferenceValue = panel;
        so.FindProperty("_buildingListContent").objectReferenceValue = content.transform;
        so.FindProperty("_selectedIcon").objectReferenceValue = iconImg;
        so.FindProperty("_selectedName").objectReferenceValue = nameGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_selectedCost").objectReferenceValue = costGO.GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedProperties();
        
        Debug.Log("[BuildingUICreator] Building UI Canvas created! Assign BuildingSlotPrefab in Inspector.");
        Selection.activeGameObject = canvasGO;
    }
    
    [MenuItem("Tools/StayAlive/Create Building Slot Prefab")]
    public static void CreateBuildingSlotPrefab()
    {
        string prefabPath = "Assets/_Project/Prefabs/UI/";
        if (!System.IO.Directory.Exists(prefabPath))
        {
            System.IO.Directory.CreateDirectory(prefabPath);
        }
        
        // Root object
        GameObject root = new GameObject("BuildingSlot");
        RectTransform rootRT = root.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(120, 140);
        
        // Background
        Image bg = root.AddComponent<Image>();
        bg.color = new Color(0.25f, 0.25f, 0.25f, 0.95f);
        
        // Button
        Button btn = root.AddComponent<Button>();
        btn.targetGraphic = bg;
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.4f, 0.6f, 0.4f);
        colors.pressedColor = new Color(0.3f, 0.5f, 0.3f);
        btn.colors = colors;
        
        // Add BuildingSlotUI component
        BuildingSlotUI slotUI = root.AddComponent<BuildingSlotUI>();
        
        // Icon
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(root.transform);
        RectTransform iconRT = iconGO.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.1f, 0.35f);
        iconRT.anchorMax = new Vector2(0.9f, 0.95f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;
        Image iconImg = iconGO.AddComponent<Image>();
        iconImg.color = new Color(0.5f, 0.5f, 0.5f);
        
        // Name Text
        GameObject nameGO = new GameObject("NameText");
        nameGO.transform.SetParent(root.transform);
        RectTransform nameRT = nameGO.AddComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0, 0.15f);
        nameRT.anchorMax = new Vector2(1, 0.35f);
        nameRT.offsetMin = new Vector2(5, 0);
        nameRT.offsetMax = new Vector2(-5, 0);
        TextMeshProUGUI nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
        nameTMP.text = "Building";
        nameTMP.fontSize = 14;
        nameTMP.alignment = TextAlignmentOptions.Center;
        nameTMP.color = Color.white;
        
        // Cost Text
        GameObject costGO = new GameObject("CostText");
        costGO.transform.SetParent(root.transform);
        RectTransform costRT = costGO.AddComponent<RectTransform>();
        costRT.anchorMin = new Vector2(0, 0);
        costRT.anchorMax = new Vector2(1, 0.15f);
        costRT.offsetMin = new Vector2(5, 2);
        costRT.offsetMax = new Vector2(-5, 0);
        TextMeshProUGUI costTMP = costGO.AddComponent<TextMeshProUGUI>();
        costTMP.text = "5x Wood";
        costTMP.fontSize = 11;
        costTMP.alignment = TextAlignmentOptions.Center;
        costTMP.color = new Color(0.7f, 0.7f, 0.7f);
        
        // Wire up references
        SerializedObject so = new SerializedObject(slotUI);
        so.FindProperty("_icon").objectReferenceValue = iconImg;
        so.FindProperty("_nameText").objectReferenceValue = nameTMP;
        so.FindProperty("_costText").objectReferenceValue = costTMP;
        so.FindProperty("_button").objectReferenceValue = btn;
        so.FindProperty("_background").objectReferenceValue = bg;
        so.ApplyModifiedProperties();
        
        // Save as prefab
        string path = prefabPath + "BuildingSlot.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        
        Debug.Log("Created: " + path);
    }
    
    private static GameObject CreatePanel(Transform parent, string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0.08f, 0.08f, 0.08f, 0.95f);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return panel;
    }
    
    private static GameObject CreateText(Transform parent, string name, string text, int fontSize, TextAlignmentOptions alignment)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        RectTransform rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return textGO;
    }
    
    private static GameObject CreateButton(Transform parent, string name, string label)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent);
        Image img = btnGO.AddComponent<Image>();
        img.color = new Color(0.2f, 0.5f, 0.2f);
        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = img;
        
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        
        return btnGO;
    }
}
