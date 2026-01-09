#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor utility to auto-create Muck-style Lobby UI.
/// Access via: Tools > StayAlive > Create Lobby UI
/// </summary>
public class LobbyUICreator : Editor
{
    [MenuItem("Tools/StayAlive/Create Lobby UI")]
    public static void CreateLobbyUI()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("LobbyCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // Create Lobby Panel - COMPACT SIZE
        GameObject lobbyPanel = CreatePanel(canvas.transform, "LobbyPanel", new Color(0.1f, 0.1f, 0.15f, 0.95f));
        RectTransform lobbyRect = lobbyPanel.GetComponent<RectTransform>();
        lobbyRect.anchorMin = new Vector2(0.5f, 0.5f);
        lobbyRect.anchorMax = new Vector2(0.5f, 0.5f);
        lobbyRect.sizeDelta = new Vector2(400, 380); // Smaller height
        
        VerticalLayoutGroup vlg = lobbyPanel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8; // Reduced spacing
        vlg.padding = new RectOffset(20, 20, 15, 15); // Smaller padding
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        
        // Title - smaller
        GameObject title = CreateTMPText(lobbyPanel.transform, "LobbyTitle", "LOBBY", 28, FontStyles.Bold);
        title.GetComponent<LayoutElement>().preferredHeight = 35;
        
        // Join Code Section - horizontal compact
        GameObject codeSection = CreatePanel(lobbyPanel.transform, "CodeSection", new Color(0.15f, 0.15f, 0.2f, 1f));
        HorizontalLayoutGroup codeHlg = codeSection.AddComponent<HorizontalLayoutGroup>();
        codeHlg.spacing = 8;
        codeHlg.padding = new RectOffset(10, 10, 5, 5);
        codeHlg.childAlignment = TextAnchor.MiddleCenter;
        codeSection.AddComponent<LayoutElement>().preferredHeight = 40;
        
        GameObject codeLabel = CreateTMPText(codeSection.transform, "CodeLabel", "Code:", 14, FontStyles.Normal);
        codeLabel.GetComponent<LayoutElement>().preferredWidth = 50;
        codeLabel.GetComponent<LayoutElement>().preferredHeight = 30;
        
        GameObject joinCodeText = CreateTMPText(codeSection.transform, "JoinCodeText", "192.168.1.1:7777", 16, FontStyles.Bold);
        joinCodeText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        joinCodeText.GetComponent<LayoutElement>().preferredWidth = 200;
        joinCodeText.GetComponent<LayoutElement>().preferredHeight = 30;
        
        GameObject copyBtn = CreateButton(codeSection.transform, "CopyCodeButton", "COPY", new Color(0.4f, 0.5f, 0.6f, 1f), 60, 30);
        
        // Player List Header - smaller
        GameObject playersHeader = CreateTMPText(lobbyPanel.transform, "PlayersHeader", "PLAYERS", 16, FontStyles.Bold);
        playersHeader.GetComponent<LayoutElement>().preferredHeight = 25;
        
        // Player List Container - compact
        GameObject playerListBg = CreatePanel(lobbyPanel.transform, "PlayerListContainer", new Color(0.15f, 0.15f, 0.2f, 1f));
        playerListBg.AddComponent<LayoutElement>().preferredHeight = 120; // Fixed small height
        VerticalLayoutGroup playerVlg = playerListBg.AddComponent<VerticalLayoutGroup>();
        playerVlg.spacing = 3;
        playerVlg.padding = new RectOffset(5, 5, 5, 5);
        playerVlg.childAlignment = TextAnchor.UpperCenter;
        playerVlg.childControlWidth = true;
        playerVlg.childControlHeight = false;
        
        // Status Text - smaller
        GameObject statusText = CreateTMPText(lobbyPanel.transform, "StatusText", "Players: 1/4  |  Ready: 0/1", 12, FontStyles.Normal);
        statusText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.7f, 0.7f);
        statusText.GetComponent<LayoutElement>().preferredHeight = 20;
        
        // Buttons - smaller and compact
        GameObject readyBtn = CreateButton(lobbyPanel.transform, "ReadyButton", "READY", new Color(0.5f, 0.5f, 0.5f, 1f), 200, 35);
        GameObject startBtn = CreateButton(lobbyPanel.transform, "StartGameButton", "START GAME", new Color(0.2f, 0.6f, 0.3f, 1f), 200, 35);
        GameObject leaveBtn = CreateButton(lobbyPanel.transform, "LeaveLobbyButton", "LEAVE", new Color(0.6f, 0.3f, 0.3f, 1f), 200, 35);
        
        // Create Player Entry Prefab Template
        GameObject playerEntryPrefab = CreatePlayerEntryPrefab(lobbyPanel.transform);
        playerEntryPrefab.SetActive(false);
        
        // Add LobbyUI component
        LobbyUI lobbyUI = lobbyPanel.AddComponent<LobbyUI>();
        
        // Assign references via SerializedObject
        SerializedObject so = new SerializedObject(lobbyUI);
        so.FindProperty("_lobbyPanel").objectReferenceValue = lobbyPanel;
        so.FindProperty("_joinCodeText").objectReferenceValue = joinCodeText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_copyCodeButton").objectReferenceValue = copyBtn.GetComponent<Button>();
        so.FindProperty("_playerListContainer").objectReferenceValue = playerListBg.transform;
        so.FindProperty("_playerEntryPrefab").objectReferenceValue = playerEntryPrefab;
        so.FindProperty("_readyButton").objectReferenceValue = readyBtn.GetComponent<Button>();
        so.FindProperty("_readyButtonText").objectReferenceValue = readyBtn.GetComponentInChildren<TextMeshProUGUI>();
        so.FindProperty("_startGameButton").objectReferenceValue = startBtn.GetComponent<Button>();
        so.FindProperty("_leaveLobbyButton").objectReferenceValue = leaveBtn.GetComponent<Button>();
        so.FindProperty("_statusText").objectReferenceValue = statusText.GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedProperties();
        
        // Hide panel by default (shown when hosting/joining)
        lobbyPanel.SetActive(false);
        
        Selection.activeGameObject = lobbyPanel;
        
        Debug.Log("[LobbyUICreator] Lobby UI created successfully!");
        EditorUtility.DisplayDialog("Lobby UI Created", 
            "Muck-style Lobby UI has been created!\n\n" +
            "It will show automatically when hosting or joining.\n\n" +
            "Make sure LobbyManager is in the scene (on NetworkManager object).", "OK");
    }
    
    private static GameObject CreatePlayerEntryPrefab(Transform parent)
    {
        GameObject entry = new GameObject("PlayerEntryPrefab");
        entry.transform.SetParent(parent, false);
        
        RectTransform rect = entry.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(340, 28); // Compact height
        
        Image bg = entry.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
        
        HorizontalLayoutGroup hlg = entry.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 5;
        hlg.padding = new RectOffset(8, 8, 2, 2);
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        
        LayoutElement entryLE = entry.AddComponent<LayoutElement>();
        entryLE.preferredHeight = 28;
        
        // Player Name
        GameObject nameGO = new GameObject("PlayerName");
        nameGO.transform.SetParent(entry.transform, false);
        nameGO.AddComponent<RectTransform>().sizeDelta = new Vector2(180, 24);
        TextMeshProUGUI nameTmp = nameGO.AddComponent<TextMeshProUGUI>();
        nameTmp.text = "Player Name";
        nameTmp.fontSize = 14;
        nameTmp.alignment = TextAlignmentOptions.Left;
        LayoutElement nameLE = nameGO.AddComponent<LayoutElement>();
        nameLE.preferredWidth = 180;
        
        // Ready Indicator (small circle)
        GameObject readyIndicator = new GameObject("ReadyIndicator");
        readyIndicator.transform.SetParent(entry.transform, false);
        readyIndicator.AddComponent<RectTransform>().sizeDelta = new Vector2(14, 14);
        Image riImg = readyIndicator.AddComponent<Image>();
        riImg.color = Color.red;
        LayoutElement riLE = readyIndicator.AddComponent<LayoutElement>();
        riLE.preferredWidth = 14;
        riLE.preferredHeight = 14;
        
        // Ready Text
        GameObject readyTextGO = new GameObject("ReadyText");
        readyTextGO.transform.SetParent(entry.transform, false);
        readyTextGO.AddComponent<RectTransform>().sizeDelta = new Vector2(80, 24);
        TextMeshProUGUI readyTmp = readyTextGO.AddComponent<TextMeshProUGUI>();
        readyTmp.text = "NOT READY";
        readyTmp.fontSize = 11;
        readyTmp.color = Color.red;
        readyTmp.alignment = TextAlignmentOptions.Right;
        LayoutElement rtLE = readyTextGO.AddComponent<LayoutElement>();
        rtLE.preferredWidth = 80;
        
        return entry;
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
    
    private static GameObject CreateButton(Transform parent, string name, string text, Color color, float width = 250, float height = 45)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        
        RectTransform rect = btnGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);
        
        Image img = btnGO.AddComponent<Image>();
        img.color = color;
        
        Button btn = btnGO.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = color * 1.2f;
        colors.pressedColor = color * 0.8f;
        btn.colors = colors;
        
        LayoutElement le = btnGO.AddComponent<LayoutElement>();
        le.preferredWidth = width;
        le.preferredHeight = height;
        
        // Add text child
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = height > 35 ? 18 : 14; // Smaller font for smaller buttons
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
        rect.sizeDelta = new Vector2(350, fontSize + 15);
        
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        LayoutElement le = textGO.AddComponent<LayoutElement>();
        le.preferredHeight = fontSize + 15;
        
        return textGO;
    }
}
#endif
