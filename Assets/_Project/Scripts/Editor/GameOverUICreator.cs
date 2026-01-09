using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor tool to create GameOver UI panels.
/// </summary>
public class GameOverUICreator : Editor
{
    [MenuItem("Tools/StayAlive/Create GameOver UI")]
    public static void CreateGameOverUI()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("GameOverCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200; // Above other UI
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // ===== VICTORY PANEL =====
        GameObject victoryPanel = CreatePanel(canvasGO.transform, "VictoryPanel", new Color(0.1f, 0.3f, 0.1f, 0.95f));
        victoryPanel.SetActive(false);
        
        CreateText(victoryPanel.transform, "VictoryTitle", "VICTORY!", 72, Color.green)
            .GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.7f);
        
        CreateText(victoryPanel.transform, "VictoryWaveText", "You survived all 20 waves!", 32, Color.white)
            .GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.5f);
        
        GameObject victoryRestart = CreateButton(victoryPanel.transform, "VictoryRestartButton", "PLAY AGAIN");
        RectTransform vrRT = victoryRestart.GetComponent<RectTransform>();
        vrRT.anchorMin = new Vector2(0.3f, 0.2f);
        vrRT.anchorMax = new Vector2(0.7f, 0.3f);
        
        GameObject victoryMenu = CreateButton(victoryPanel.transform, "VictoryMainMenuButton", "MAIN MENU");
        victoryMenu.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f);
        RectTransform vmRT = victoryMenu.GetComponent<RectTransform>();
        vmRT.anchorMin = new Vector2(0.3f, 0.1f);
        vmRT.anchorMax = new Vector2(0.7f, 0.18f);
        
        // ===== DEFEAT PANEL =====
        GameObject defeatPanel = CreatePanel(canvasGO.transform, "DefeatPanel", new Color(0.3f, 0.1f, 0.1f, 0.95f));
        defeatPanel.SetActive(false);
        
        CreateText(defeatPanel.transform, "DefeatTitle", "DEFEAT", 72, Color.red)
            .GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.7f);
        
        CreateText(defeatPanel.transform, "DefeatWaveText", "Survived until wave 5", 32, Color.white)
            .GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.5f);
        
        GameObject defeatRestart = CreateButton(defeatPanel.transform, "DefeatRestartButton", "TRY AGAIN");
        defeatRestart.GetComponent<Image>().color = new Color(0.6f, 0.3f, 0.2f);
        RectTransform drRT = defeatRestart.GetComponent<RectTransform>();
        drRT.anchorMin = new Vector2(0.3f, 0.2f);
        drRT.anchorMax = new Vector2(0.7f, 0.3f);
        
        GameObject defeatMenu = CreateButton(defeatPanel.transform, "DefeatMainMenuButton", "MAIN MENU");
        defeatMenu.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f);
        RectTransform dmRT = defeatMenu.GetComponent<RectTransform>();
        dmRT.anchorMin = new Vector2(0.3f, 0.1f);
        dmRT.anchorMax = new Vector2(0.7f, 0.18f);
        
        // Add GameOverUI component
        GameOverUI ui = canvasGO.AddComponent<GameOverUI>();
        
        // Wire refs
        SerializedObject so = new SerializedObject(ui);
        so.FindProperty("_victoryPanel").objectReferenceValue = victoryPanel;
        so.FindProperty("_defeatPanel").objectReferenceValue = defeatPanel;
        so.FindProperty("_victoryWaveText").objectReferenceValue = victoryPanel.transform.Find("VictoryWaveText").GetComponent<TextMeshProUGUI>();
        so.FindProperty("_defeatWaveText").objectReferenceValue = defeatPanel.transform.Find("DefeatWaveText").GetComponent<TextMeshProUGUI>();
        so.FindProperty("_victoryRestartButton").objectReferenceValue = victoryRestart.GetComponent<Button>();
        so.FindProperty("_victoryMainMenuButton").objectReferenceValue = victoryMenu.GetComponent<Button>();
        so.FindProperty("_defeatRestartButton").objectReferenceValue = defeatRestart.GetComponent<Button>();
        so.FindProperty("_defeatMainMenuButton").objectReferenceValue = defeatMenu.GetComponent<Button>();
        so.ApplyModifiedProperties();
        
        Debug.Log("[GameOverUICreator] GameOver UI created!");
        Selection.activeGameObject = canvasGO;
    }
    
    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);
        Image img = panel.AddComponent<Image>();
        img.color = color;
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return panel;
    }
    
    private static GameObject CreateText(Transform parent, string name, string text, int fontSize, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }
    
    private static GameObject CreateButton(Transform parent, string name, string label)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent);
        Image img = btnGO.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 0.2f);
        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = img;
        
        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
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
