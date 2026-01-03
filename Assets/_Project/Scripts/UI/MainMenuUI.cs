using UnityEngine;
using UnityEngine.UI;
using StayAlive.Network;
using TMPro;

namespace StayAlive.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _joinButton;
        [SerializeField] private TMP_InputField _joinCodeInput; // For future use with Relay

        private void Start()
        {
            _hostButton.onClick.AddListener(OnHostClicked);
            _joinButton.onClick.AddListener(OnJoinClicked);
        }

        private void OnHostClicked()
        {
            Debug.Log("[MainMenuUI] Starting Host...");
            GameNetworkManager.Instance.StartHost();
        }

        private void OnJoinClicked()
        {
            Debug.Log("[MainMenuUI] Starting Client...");
            // TODO: Parse join code if using Relay
            GameNetworkManager.Instance.StartClient();
        }
    }
}
