using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player interaction with world objects (resource nodes, items, etc).
/// Press E to interact with object in center of screen.
/// </summary>
public class PlayerInteraction : NetworkBehaviour
{
    #region Configuration
    [Header("Interaction Settings")]
    [SerializeField] private float _interactionRange = 3f;
    [SerializeField] private LayerMask _interactionMask = ~0;
    [SerializeField] private KeyCode _interactKey = KeyCode.E;
    
    [Header("UI References")]
    [SerializeField] private GameObject _interactionPrompt;
    [SerializeField] private TMPro.TextMeshProUGUI _interactionText;
    #endregion

    #region State
    private Camera _playerCamera;
    private IInteractable _currentTarget;
    private bool _isInteracting;
    private float _interactHoldTime;
    #endregion

    #region Unity Lifecycle
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
        
        // Find player camera
        _playerCamera = GetComponentInChildren<Camera>();
        if (_playerCamera == null)
        {
            _playerCamera = Camera.main;
        }
        
        // Hide prompt initially
        SetPromptVisible(false);
    }

    private void Update()
    {
        if (!IsOwner || _playerCamera == null) return;
        
        CheckForInteractable();
        HandleInteraction();
    }
    #endregion

    #region Interaction Detection
    private void CheckForInteractable()
    {
        // Raycast from center of screen
        Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, _interactionRange, _interactionMask))
        {
            // Try to get interactable component
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable == null)
            {
                // Also check parent
                interactable = hit.collider.GetComponentInParent<IInteractable>();
            }
            
            if (interactable != null && interactable.CanInteract)
            {
                if (_currentTarget != interactable)
                {
                    _currentTarget = interactable;
                    ShowPrompt(interactable.InteractionPrompt);
                }
                return;
            }
        }
        
        // No valid target
        if (_currentTarget != null)
        {
            _currentTarget = null;
            SetPromptVisible(false);
        }
    }
    #endregion

    #region Interaction Handling
    private void HandleInteraction()
    {
        if (_currentTarget == null) return;
        
        // Check for interact input
        bool interactPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool interactHeld = Keyboard.current != null && Keyboard.current.eKey.isPressed;
        
        if (interactPressed)
        {
            // Instant interaction
            if (_currentTarget.InteractionTime <= 0)
            {
                PerformInteraction();
            }
            else
            {
                _isInteracting = true;
                _interactHoldTime = 0f;
            }
        }
        
        if (_isInteracting && interactHeld)
        {
            // Hold interaction
            _interactHoldTime += Time.deltaTime;
            
            // Update prompt with progress
            float progress = _interactHoldTime / _currentTarget.InteractionTime;
            UpdatePromptProgress(progress);
            
            if (_interactHoldTime >= _currentTarget.InteractionTime)
            {
                PerformInteraction();
                _isInteracting = false;
            }
        }
        else if (_isInteracting && !interactHeld)
        {
            // Cancelled
            _isInteracting = false;
            ShowPrompt(_currentTarget.InteractionPrompt);
        }
    }

    private void PerformInteraction()
    {
        if (_currentTarget == null) return;
        
        Debug.Log($"[PlayerInteraction] Interacting with {_currentTarget.InteractionPrompt}");
        _currentTarget.Interact(this);
        
        // Re-check if still valid after interaction
        if (!_currentTarget.CanInteract)
        {
            _currentTarget = null;
            SetPromptVisible(false);
        }
    }
    #endregion

    #region UI
    private void ShowPrompt(string text)
    {
        SetPromptVisible(true);
        if(_interactionText == null)
        {
            _interactionText = GameObject.Find("NotificationText").GetComponent<TMPro.TextMeshProUGUI>();
        }
        if (_interactionText != null)
        {
            _interactionText.text = $"[E] {text}";
        }
    }

    private void UpdatePromptProgress(float progress)
    {
        if (_interactionText != null)
        {
            int percentage = Mathf.RoundToInt(progress * 100f);
            _interactionText.text = $"Gathering... {percentage}%";
        }
    }

    private void SetPromptVisible(bool visible)
    {
        if(_interactionPrompt == null)
        {
            _interactionPrompt = GameObject.Find("NotificationText");
        }
        if (_interactionPrompt != null)
        {
            _interactionPrompt.SetActive(visible);
        }
    }
    #endregion

    #region Debug
    private void OnDrawGizmosSelected()
    {
        if (_playerCamera == null) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(_playerCamera.transform.position, 
            _playerCamera.transform.forward * _interactionRange);
    }
    #endregion
}
