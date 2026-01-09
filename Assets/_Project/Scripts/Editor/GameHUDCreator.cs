#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor utility to create the Game HUD UI.
/// Access via: Tools > StayAlive > Create Game HUD
/// </summary>
public class GameHUDCreator : Editor
{
    [MenuItem("Tools/StayAlive/Create Game HUD")]
    public static void CreateGameHUD()
    {
        // Check if GameHUD already exists
        if (Object.FindObjectOfType<GameHUD>() != null)
        {
            if (!EditorUtility.DisplayDialog("HUD Exists",
                "A GameHUD already exists. Create another?", "Yes", "Cancel"))
            {
                return;
            }
        }

        // Create Canvas
        GameObject canvasGO = new GameObject("GameHUD_Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Add GameHUD script
        GameHUD gameHUD = canvasGO.AddComponent<GameHUD>();

        // Create Vitals Panel (bottom left)
        GameObject vitalsPanel = CreatePanel(canvasGO.transform, "VitalsPanel", 
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(20, 20), new Vector2(300, 120));
        
        // Health Bar
        Slider healthBar = CreateStatBar(vitalsPanel.transform, "HealthBar", 
            new Color(0.8f, 0.2f, 0.2f), 0, 90);
        
        // Hunger Bar
        Slider hungerBar = CreateStatBar(vitalsPanel.transform, "HungerBar", 
            new Color(0.8f, 0.6f, 0.2f), 0, 50);
        
        // Thirst Bar
        Slider thirstBar = CreateStatBar(vitalsPanel.transform, "ThirstBar", 
            new Color(0.2f, 0.5f, 0.8f), 0, 10);

        // Health Text
        TextMeshProUGUI healthText = CreateText(vitalsPanel.transform, "HealthText", "100/100",
            new Vector2(210, 90), new Vector2(80, 30), 16, TextAlignmentOptions.Right);

        // Create Wave Panel (top center)
        GameObject wavePanel = CreatePanel(canvasGO.transform, "WavePanel",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(300, 80));
        
        TextMeshProUGUI waveText = CreateText(wavePanel.transform, "WaveText", "Wave 1",
            new Vector2(0, -10), new Vector2(300, 40), 28, TextAlignmentOptions.Center);
        waveText.fontStyle = FontStyles.Bold;
        
        TextMeshProUGUI enemiesText = CreateText(wavePanel.transform, "EnemiesText", "0/0",
            new Vector2(0, -50), new Vector2(300, 30), 18, TextAlignmentOptions.Center);

        // Create Time Panel (top right)
        GameObject timePanel = CreatePanel(canvasGO.transform, "TimePanel",
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -20), new Vector2(200, 80));
        
        TextMeshProUGUI phaseText = CreateText(timePanel.transform, "PhaseText", "DAY",
            new Vector2(-100, -10), new Vector2(100, 40), 24, TextAlignmentOptions.Right);
        phaseText.fontStyle = FontStyles.Bold;
        
        TextMeshProUGUI timeText = CreateText(timePanel.transform, "TimeText", "4:00",
            new Vector2(-100, -50), new Vector2(100, 30), 20, TextAlignmentOptions.Right);

        // Create Weapon Panel (bottom right)
        GameObject weaponPanel = CreatePanel(canvasGO.transform, "WeaponPanel",
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(-20, 20), new Vector2(200, 80));
        
        TextMeshProUGUI weaponNameText = CreateText(weaponPanel.transform, "WeaponName", "Pistol",
            new Vector2(-100, -10), new Vector2(180, 30), 18, TextAlignmentOptions.Right);
        
        TextMeshProUGUI ammoText = CreateText(weaponPanel.transform, "AmmoText", "30 / 90",
            new Vector2(-100, -45), new Vector2(180, 40), 28, TextAlignmentOptions.Right);
        ammoText.fontStyle = FontStyles.Bold;

        // Create Notification Text (center)
        TextMeshProUGUI notificationText = CreateText(canvasGO.transform, "NotificationText", "",
            new Vector2(0, 100), new Vector2(600, 60), 32, TextAlignmentOptions.Center);
        notificationText.fontStyle = FontStyles.Bold;
        notificationText.color = Color.yellow;
        notificationText.gameObject.SetActive(false);
        
        // Set anchors for notification
        RectTransform notifRect = notificationText.GetComponent<RectTransform>();
        notifRect.anchorMin = new Vector2(0.5f, 0.5f);
        notifRect.anchorMax = new Vector2(0.5f, 0.5f);

        // Create Tower Health Panel (top left)
        GameObject towerPanel = CreatePanel(canvasGO.transform, "TowerHealthPanel",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(250, 60));
        
        TextMeshProUGUI towerLabel = CreateText(towerPanel.transform, "TowerLabel", "TOWER",
            new Vector2(125, -5), new Vector2(100, 25), 14, TextAlignmentOptions.Left);
        
        Slider towerHealthBar = CreateStatBar(towerPanel.transform, "TowerHealthBar",
            new Color(0.3f, 0.8f, 0.3f), 0, 25);
        towerHealthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(230, 20);
        
        TextMeshProUGUI towerHealthText = CreateText(towerPanel.transform, "TowerHealthText", "1000/1000",
            new Vector2(185, 25), new Vector2(80, 25), 14, TextAlignmentOptions.Right);

        // Assign references to GameHUD
        SerializedObject hudSO = new SerializedObject(gameHUD);
        hudSO.FindProperty("_healthBar").objectReferenceValue = healthBar;
        hudSO.FindProperty("_healthText").objectReferenceValue = healthText;
        hudSO.FindProperty("_hungerBar").objectReferenceValue = hungerBar;
        hudSO.FindProperty("_thirstBar").objectReferenceValue = thirstBar;
        hudSO.FindProperty("_waveText").objectReferenceValue = waveText;
        hudSO.FindProperty("_enemiesText").objectReferenceValue = enemiesText;
        hudSO.FindProperty("_timeText").objectReferenceValue = timeText;
        hudSO.FindProperty("_phaseText").objectReferenceValue = phaseText;
        hudSO.FindProperty("_ammoText").objectReferenceValue = ammoText;
        hudSO.FindProperty("_weaponNameText").objectReferenceValue = weaponNameText;
        hudSO.FindProperty("_towerHealthBar").objectReferenceValue = towerHealthBar;
        hudSO.FindProperty("_towerHealthText").objectReferenceValue = towerHealthText;
        hudSO.FindProperty("_notificationText").objectReferenceValue = notificationText;
        hudSO.ApplyModifiedProperties();

        Selection.activeGameObject = canvasGO;
        
        Debug.Log("[GameHUDCreator] Game HUD created!");
        EditorUtility.DisplayDialog("HUD Created",
            "Game HUD has been created with:\n\n" +
            "• Vitals Panel (Health, Hunger, Thirst)\n" +
            "• Wave Info Panel\n" +
            "• Time/Phase Panel\n" +
            "• Weapon/Ammo Panel\n" +
            "• Tower Health Panel\n" +
            "• Notification Text\n\n" +
            "All references auto-assigned to GameHUD script!", "OK");
    }

    private static GameObject CreatePanel(Transform parent, string name, 
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.5f);
        
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
        
        return panel;
    }

    private static Slider CreateStatBar(Transform parent, string name, Color fillColor, float x, float y)
    {
        GameObject barGO = new GameObject(name);
        barGO.transform.SetParent(parent, false);
        
        RectTransform barRect = barGO.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0, 1);
        barRect.anchorMax = new Vector2(0, 1);
        barRect.pivot = new Vector2(0, 1);
        barRect.anchoredPosition = new Vector2(x + 10, -y);
        barRect.sizeDelta = new Vector2(200, 25);
        
        Slider slider = barGO.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;
        slider.interactable = false;
        
        // Background
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(barGO.transform, false);
        Image bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        // Fill Area
        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(barGO.transform, false);
        RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0);
        fillAreaRect.anchorMax = new Vector2(1, 1);
        fillAreaRect.offsetMin = new Vector2(5, 5);
        fillAreaRect.offsetMax = new Vector2(-5, -5);
        
        // Fill
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        Image fillImage = fillGO.AddComponent<Image>();
        fillImage.color = fillColor;
        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        
        slider.fillRect = fillRect;
        slider.targetGraphic = bgImage;
        
        return slider;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text,
        Vector2 position, Vector2 size, int fontSize, TextAlignmentOptions alignment)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);
        
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        
        RectTransform rect = textGO.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        return tmp;
    }
}
#endif
