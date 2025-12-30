// TestUI.cs - put in Scripts/UI/
using UnityEngine;
using UnityEngine.UI;

public class TestUI : MonoBehaviour
{
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _startButton;

    private void Start()
    {
        _hostButton.onClick.AddListener(() => {
            NetworkGameManager.Instance.StartHost();
        });
        
        _startButton.onClick.AddListener(() => {
            Debug.Log("[TestUI] Starting game...");
            GameManager.Instance.StartGameServerRpc();
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("[TestUI] Starting game...");
            GameManager.Instance.StartGameServerRpc();
        }
    }
}