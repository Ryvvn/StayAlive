using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StayAlive.Network
{
    public class GameNetworkManager : NetworkBehaviour
    {
        public static GameNetworkManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();
            LoadGameplayScene();
        }

        public void StartClient()
        {
            NetworkManager.Singleton.StartClient();
        }

        private void LoadGameplayScene()
        {
            // Only the host loads the scene, creating a networked scene transition
            NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
        }
    }
}
