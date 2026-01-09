using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

/// <summary>
/// Displays game over screen (Victory or Defeat).
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _victoryPanel;
    [SerializeField] private GameObject _defeatPanel;
    
    [Header("Victory Elements")]
    [SerializeField] private TextMeshProUGUI _victoryWaveText;
    [SerializeField] private TextMeshProUGUI _victoryStatsText;
    [SerializeField] private Button _victoryRestartButton;
    [SerializeField] private Button _victoryMainMenuButton;
    
    [Header("Defeat Elements")]
    [SerializeField] private TextMeshProUGUI _defeatWaveText;
    [SerializeField] private TextMeshProUGUI _defeatStatsText;
    [SerializeField] private Button _defeatRestartButton;
    [SerializeField] private Button _defeatMainMenuButton;

    private void Start()
    {
        HideAll();
        
        if (_victoryRestartButton != null)
            _victoryRestartButton.onClick.AddListener(OnRestartClicked);
        if (_victoryMainMenuButton != null)
            _victoryMainMenuButton.onClick.AddListener(OnMainMenuClicked);
        if (_defeatRestartButton != null)
            _defeatRestartButton.onClick.AddListener(OnRestartClicked);
        if (_defeatMainMenuButton != null)
            _defeatMainMenuButton.onClick.AddListener(OnMainMenuClicked);
        
        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnVictory += ShowVictory;
            GameManager.Instance.OnDefeat += ShowDefeat;
        }
        else
        {
            // Wait for GameManager
            StartCoroutine(WaitForGameManager());
        }
    }

    private System.Collections.IEnumerator WaitForGameManager()
    {
        while (GameManager.Instance == null)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        GameManager.Instance.OnVictory += ShowVictory;
        GameManager.Instance.OnDefeat += ShowDefeat;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnVictory -= ShowVictory;
            GameManager.Instance.OnDefeat -= ShowDefeat;
        }
    }

    private void HideAll()
    {
        if (_victoryPanel != null) _victoryPanel.SetActive(false);
        if (_defeatPanel != null) _defeatPanel.SetActive(false);
    }

    public void ShowVictory()
    {
        HideAll();
        if (_victoryPanel != null) _victoryPanel.SetActive(true);
        
        int wave = GameManager.Instance?.CurrentWave.Value ?? 0;
        if (_victoryWaveText != null)
            _victoryWaveText.text = $"You survived {wave} waves!";
        
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowDefeat()
    {
        HideAll();
        if (_defeatPanel != null) _defeatPanel.SetActive(true);
        
        int wave = GameManager.Instance?.CurrentWave.Value ?? 0;
        if (_defeatWaveText != null)
            _defeatWaveText.text = $"Survived until wave {wave}";
        
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnRestartClicked()
    {
        Debug.Log("[GameOverUI] Restart clicked");
        
        // Restart game on server
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGameServerRpc();
        }
        
        HideAll();
    }

    private void OnMainMenuClicked()
    {
        Debug.Log("[GameOverUI] Main Menu clicked");
        
        // Disconnect and return to menu
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        // Load main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
