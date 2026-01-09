using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor tool to create the Crafting UI Canvas.
/// </summary>
public class CraftingUICreator : Editor
{
    [MenuItem("Tools/StayAlive/Create Crafting UI")]
    public static void CreateCraftingUI()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("CraftingCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Main Panel (hidden by default)
        GameObject panel = CreatePanel(canvasGO.transform, "CraftingPanel");
        panel.SetActive(false);
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.1f, 0.1f);
        panelRT.anchorMax = new Vector2(0.9f, 0.9f);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        
        // Title
        CreateText(panel.transform, "Title", "CRAFTING", 32, TextAlignmentOptions.Top);
        
        // Recipe List (Left Side)
        GameObject recipeList = CreatePanel(panel.transform, "RecipeList");
        RectTransform listRT = recipeList.GetComponent<RectTransform>();
        listRT.anchorMin = new Vector2(0, 0);
        listRT.anchorMax = new Vector2(0.35f, 0.9f);
        listRT.offsetMin = new Vector2(10, 10);
        listRT.offsetMax = new Vector2(-5, -40);
        
        // ScrollView for recipes
        GameObject scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(recipeList.transform);
        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        RectTransform scrollRT = scrollView.GetComponent<RectTransform>();
        scrollRT.anchorMin = Vector2.zero;
        scrollRT.anchorMax = Vector2.one;
        scrollRT.offsetMin = Vector2.zero;
        scrollRT.offsetMax = Vector2.zero;
        
        // Content container
        GameObject content = new GameObject("Content");
        content.transform.SetParent(scrollView.transform);
        RectTransform contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = new Vector2(0, 500);
        
        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.spacing = 5;
        vlg.padding = new RectOffset(5, 5, 5, 5);
        
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        scrollRect.content = contentRT;
        scrollRect.viewport = scrollRT;
        
        // Details Panel (Right Side)
        GameObject details = CreatePanel(panel.transform, "DetailsPanel");
        RectTransform detailsRT = details.GetComponent<RectTransform>();
        detailsRT.anchorMin = new Vector2(0.37f, 0);
        detailsRT.anchorMax = new Vector2(1, 0.9f);
        detailsRT.offsetMin = new Vector2(5, 10);
        detailsRT.offsetMax = new Vector2(-10, -40);
        
        // Selected item icon
        GameObject iconGO = new GameObject("SelectedIcon");
        iconGO.transform.SetParent(details.transform);
        Image iconImg = iconGO.AddComponent<Image>();
        RectTransform iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.1f, 0.7f);
        iconRT.anchorMax = new Vector2(0.3f, 0.95f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;
        
        // Selected name
        CreateText(details.transform, "SelectedName", "Select a Recipe", 24, TextAlignmentOptions.TopLeft)
            .GetComponent<RectTransform>().anchorMin = new Vector2(0.35f, 0.85f);
        
        // Description
        CreateText(details.transform, "SelectedDescription", "Recipe description here...", 16, TextAlignmentOptions.TopLeft)
            .GetComponent<RectTransform>().anchorMin = new Vector2(0.1f, 0.5f);
        
        // Ingredients Panel
        GameObject ingredients = CreatePanel(details.transform, "IngredientsPanel");
        RectTransform ingRT = ingredients.GetComponent<RectTransform>();
        ingRT.anchorMin = new Vector2(0.1f, 0.2f);
        ingRT.anchorMax = new Vector2(0.9f, 0.5f);
        ingRT.offsetMin = Vector2.zero;
        ingRT.offsetMax = Vector2.zero;
        
        // Add layout group for ingredients
        VerticalLayoutGroup ingVLG = ingredients.AddComponent<VerticalLayoutGroup>();
        ingVLG.childControlWidth = true;
        ingVLG.childControlHeight = false;
        ingVLG.childForceExpandWidth = true;
        ingVLG.spacing = 5;
        ingVLG.padding = new RectOffset(10, 10, 10, 10);
        
        ContentSizeFitter ingCSF = ingredients.AddComponent<ContentSizeFitter>();
        ingCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Craft Button
        GameObject craftBtn = CreateButton(details.transform, "CraftButton", "CRAFT");
        RectTransform btnRT = craftBtn.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.3f, 0.05f);
        btnRT.anchorMax = new Vector2(0.7f, 0.15f);
        btnRT.offsetMin = Vector2.zero;
        btnRT.offsetMax = Vector2.zero;
        
        // Close Button
        GameObject closeBtn = CreateButton(panel.transform, "CloseButton", "X");
        RectTransform closeBtnRT = closeBtn.GetComponent<RectTransform>();
        closeBtnRT.anchorMin = new Vector2(0.92f, 0.92f);
        closeBtnRT.anchorMax = new Vector2(0.98f, 0.98f);
        closeBtnRT.offsetMin = Vector2.zero;
        closeBtnRT.offsetMax = Vector2.zero;
        
        // Progress Panel (hidden)
        GameObject progress = CreatePanel(panel.transform, "ProgressPanel");
        progress.SetActive(false);
        RectTransform progressRT = progress.GetComponent<RectTransform>();
        progressRT.anchorMin = new Vector2(0.3f, 0.45f);
        progressRT.anchorMax = new Vector2(0.7f, 0.55f);
        progressRT.offsetMin = Vector2.zero;
        progressRT.offsetMax = Vector2.zero;
        
        // Progress Bar Background
        GameObject progressBG = new GameObject("ProgressBarBG");
        progressBG.transform.SetParent(progress.transform);
        Image bgImg = progressBG.AddComponent<Image>();
        bgImg.color = Color.gray;
        RectTransform bgRT = progressBG.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        
        // Progress Bar Fill
        GameObject progressFill = new GameObject("ProgressBarFill");
        progressFill.transform.SetParent(progress.transform);
        Image fillImg = progressFill.AddComponent<Image>();
        fillImg.color = Color.green;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        RectTransform fillRT = progressFill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        
        // Add CraftingUI component
        CraftingUI craftingUI = canvasGO.AddComponent<CraftingUI>();
        
        Debug.Log("[CraftingUICreator] Crafting UI Canvas created! Assign references in Inspector.");
        Selection.activeGameObject = canvasGO;
    }
    
    private static GameObject CreatePanel(Transform parent, string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
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
        img.color = new Color(0.2f, 0.6f, 0.2f);
        Button btn = btnGO.AddComponent<Button>();
        
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
