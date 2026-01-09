#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor utility to auto-create MainMenu UI hierarchy.
/// Access via: Tools > StayAlive > Create Main Menu UI
/// </summary>
public class MainMenuUICreator : Editor
{
    [MenuItem("Tools/StayAlive/Create Main Menu UI")]
    public static void CreateMainMenuUI()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create EventSystem if not exists
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        // Create background panel
        GameObject bgPanel = CreatePanel(canvasGO.transform, "BackgroundPanel", new Color(0.1f, 0.1f, 0.15f, 1f));
        RectTransform bgRect = bgPanel.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        // Create main content container
        GameObject mainContainer = CreatePanel(bgPanel.transform, "MainContainer", Color.clear);
        RectTransform mainRect = mainContainer.GetComponent<RectTransform>();
        mainRect.anchorMin = new Vector2(0.5f, 0.5f);
        mainRect.anchorMax = new Vector2(0.5f, 0.5f);
        mainRect.sizeDelta = new Vector2(400, 500);
        
        // Add vertical layout
        VerticalLayoutGroup vlg = mainContainer.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        mainContainer.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Title
        CreateTMPText(mainContainer.transform, "TitleText", "STAY ALIVE", 48, FontStyles.Bold);
        
        // Spacer
        CreateSpacer(mainContainer.transform, 30);
        
        // Host Button
        GameObject hostBtn = CreateButton(mainContainer.transform, "HostButton", "HOST GAME", new Color(0.2f, 0.6f, 0.3f, 1f));
        
        // Join Button
        GameObject joinBtn = CreateButton(mainContainer.transform, "JoinButton", "JOIN GAME", new Color(0.3f, 0.5f, 0.7f, 1f));
        
        // Quit Button
        GameObject quitBtn = CreateButton(mainContainer.transform, "QuitButton", "QUIT", new Color(0.6f, 0.3f, 0.3f, 1f));
        
        // Create Join Panel (hidden by default)
        GameObject joinPanel = CreatePanel(bgPanel.transform, "JoinPanel", new Color(0.15f, 0.15f, 0.2f, 0.95f));
        RectTransform joinPanelRect = joinPanel.GetComponent<RectTransform>();
        joinPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
        joinPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
        joinPanelRect.sizeDelta = new Vector2(400, 250);
        joinPanel.SetActive(false);
        
        VerticalLayoutGroup joinVlg = joinPanel.AddComponent<VerticalLayoutGroup>();
        joinVlg.spacing = 15;
        joinVlg.padding = new RectOffset(30, 30, 30, 30);
        joinVlg.childAlignment = TextAnchor.MiddleCenter;
        joinVlg.childControlWidth = true;
        joinVlg.childControlHeight = false;
        
        CreateTMPText(joinPanel.transform, "JoinTitle", "ENTER JOIN CODE", 24, FontStyles.Bold);
        GameObject joinCodeInput = CreateInputField(joinPanel.transform, "JoinCodeInput", "192.168.1.1:7777");
        GameObject connectBtn = CreateButton(joinPanel.transform, "ConnectButton", "CONNECT", new Color(0.3f, 0.6f, 0.4f, 1f));
        GameObject cancelJoinBtn = CreateButton(joinPanel.transform, "CancelJoinButton", "CANCEL", new Color(0.5f, 0.5f, 0.5f, 1f));
        
        // Create Host Panel (hidden by default)
        GameObject hostPanel = CreatePanel(bgPanel.transform, "HostPanel", new Color(0.15f, 0.15f, 0.2f, 0.95f));
        RectTransform hostPanelRect = hostPanel.GetComponent<RectTransform>();
        hostPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
        hostPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
        hostPanelRect.sizeDelta = new Vector2(400, 250);
        hostPanel.SetActive(false);
        
        VerticalLayoutGroup hostVlg = hostPanel.AddComponent<VerticalLayoutGroup>();
        hostVlg.spacing = 15;
        hostVlg.padding = new RectOffset(30, 30, 30, 30);
        hostVlg.childAlignment = TextAnchor.MiddleCenter;
        hostVlg.childControlWidth = true;
        hostVlg.childControlHeight = false;
        
        CreateTMPText(hostPanel.transform, "HostTitle", "YOUR JOIN CODE", 24, FontStyles.Bold);
        GameObject joinCodeDisplay = CreateTMPText(hostPanel.transform, "JoinCodeDisplay", "192.168.1.1:7777", 32, FontStyles.Normal);
        joinCodeDisplay.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        GameObject copyBtn = CreateButton(hostPanel.transform, "CopyCodeButton", "COPY CODE", new Color(0.4f, 0.5f, 0.6f, 1f));
        GameObject cancelHostBtn = CreateButton(hostPanel.transform, "CancelHostButton", "STOP HOSTING", new Color(0.6f, 0.3f, 0.3f, 1f));
        
        // Create Connecting Panel (hidden by default)
        GameObject connectingPanel = CreatePanel(bgPanel.transform, "ConnectingPanel", new Color(0.1f, 0.1f, 0.15f, 0.95f));
        RectTransform connectingRect = connectingPanel.GetComponent<RectTransform>();
        connectingRect.anchorMin = new Vector2(0.5f, 0.5f);
        connectingRect.anchorMax = new Vector2(0.5f, 0.5f);
        connectingRect.sizeDelta = new Vector2(300, 150);
        connectingPanel.SetActive(false);
        
        VerticalLayoutGroup connVlg = connectingPanel.AddComponent<VerticalLayoutGroup>();
        connVlg.spacing = 10;
        connVlg.padding = new RectOffset(20, 20, 20, 20);
        connVlg.childAlignment = TextAnchor.MiddleCenter;
        
        GameObject statusText = CreateTMPText(connectingPanel.transform, "StatusText", "Connecting...", 20, FontStyles.Normal);
        
        // Create Error Text (at bottom)
        GameObject errorText = CreateTMPText(bgPanel.transform, "ErrorText", "", 18, FontStyles.Normal);
        errorText.GetComponent<TextMeshProUGUI>().color = Color.red;
        RectTransform errorRect = errorText.GetComponent<RectTransform>();
        errorRect.anchorMin = new Vector2(0.5f, 0.1f);
        errorRect.anchorMax = new Vector2(0.5f, 0.1f);
        errorRect.sizeDelta = new Vector2(500, 50);
        errorText.SetActive(false);
        
        // Add MainMenuUI component and assign references
        MainMenuUI menuUI = canvasGO.AddComponent<MainMenuUI>();
        
        // Use SerializedObject to assign private fields
        SerializedObject so = new SerializedObject(menuUI);
        so.FindProperty("_mainMenuContainer").objectReferenceValue = mainContainer; // Main container to hide
        so.FindProperty("_hostButton").objectReferenceValue = hostBtn.GetComponent<Button>();
        so.FindProperty("_joinButton").objectReferenceValue = joinBtn.GetComponent<Button>();
        so.FindProperty("_quitButton").objectReferenceValue = quitBtn.GetComponent<Button>();
        so.FindProperty("_joinPanel").objectReferenceValue = joinPanel;
        so.FindProperty("_joinCodeInput").objectReferenceValue = joinCodeInput.GetComponent<TMP_InputField>();
        so.FindProperty("_connectButton").objectReferenceValue = connectBtn.GetComponent<Button>();
        so.FindProperty("_cancelJoinButton").objectReferenceValue = cancelJoinBtn.GetComponent<Button>();
        so.FindProperty("_hostPanel").objectReferenceValue = hostPanel;
        so.FindProperty("_joinCodeDisplay").objectReferenceValue = joinCodeDisplay.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_copyCodeButton").objectReferenceValue = copyBtn.GetComponent<Button>();
        so.FindProperty("_cancelHostButton").objectReferenceValue = cancelHostBtn.GetComponent<Button>();
        so.FindProperty("_connectingPanel").objectReferenceValue = connectingPanel;
        so.FindProperty("_statusText").objectReferenceValue = statusText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_errorText").objectReferenceValue = errorText.GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedProperties();
        
        // Select the created object
        Selection.activeGameObject = canvasGO;
        
        Debug.Log("[MainMenuUICreator] Main Menu UI created successfully! All references assigned.");
        EditorUtility.DisplayDialog("Main Menu Created", "MainMenu UI has been created with all references assigned.\n\nMake sure NetworkGameManager is in the scene!", "OK");
    }
    
    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        Image img = panel.AddComponent<Image>();
        img.color = color;
        
        return panel;
    }
    
    private static GameObject CreateButton(Transform parent, string name, string text, Color color)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        
        RectTransform rect = btnGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(250, 50);
        
        Image img = btnGO.AddComponent<Image>();
        img.color = color;
        
        Button btn = btnGO.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = color * 1.2f;
        colors.pressedColor = color * 0.8f;
        btn.colors = colors;
        
        // Add text child
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 20;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        return btnGO;
    }
    
    private static GameObject CreateTMPText(Transform parent, string name, string text, float fontSize, FontStyles style)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);
        
        RectTransform rect = textGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(350, fontSize + 20);
        
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        return textGO;
    }
    
    private static GameObject CreateInputField(Transform parent, string name, string placeholder)
    {
        GameObject inputGO = new GameObject(name);
        inputGO.transform.SetParent(parent, false);
        
        RectTransform rect = inputGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 50);
        
        Image img = inputGO.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.25f, 1f);
        
        TMP_InputField input = inputGO.AddComponent<TMP_InputField>();
        
        // Text Area
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputGO.transform, false);
        RectTransform taRect = textArea.AddComponent<RectTransform>();
        taRect.anchorMin = Vector2.zero;
        taRect.anchorMax = Vector2.one;
        taRect.offsetMin = new Vector2(10, 5);
        taRect.offsetMax = new Vector2(-10, -5);
        textArea.AddComponent<RectMask2D>();
        
        // Placeholder
        GameObject placeholderGO = new GameObject("Placeholder");
        placeholderGO.transform.SetParent(textArea.transform, false);
        RectTransform phRect = placeholderGO.AddComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI phTmp = placeholderGO.AddComponent<TextMeshProUGUI>();
        phTmp.text = placeholder;
        phTmp.fontSize = 18;
        phTmp.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        phTmp.alignment = TextAlignmentOptions.Left;
        
        // Text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(textArea.transform, false);
        RectTransform tRect = textGO.AddComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI tTmp = textGO.AddComponent<TextMeshProUGUI>();
        tTmp.fontSize = 18;
        tTmp.color = Color.white;
        tTmp.alignment = TextAlignmentOptions.Left;
        
        input.textComponent = tTmp;
        input.placeholder = phTmp;
        input.textViewport = taRect;
        
        return inputGO;
    }
    
    private static void CreateSpacer(Transform parent, float height)
    {
        GameObject spacer = new GameObject("Spacer");
        spacer.transform.SetParent(parent, false);
        RectTransform rect = spacer.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(10, height);
        LayoutElement le = spacer.AddComponent<LayoutElement>();
        le.minHeight = height;
        le.preferredHeight = height;
    }
}
#endif
