using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// FPS player controller with network support.
/// Handles movement, looking, and basic interactions.
/// Client authoritative for movement (with server validation).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    #region Configuration
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private float _sprintMultiplier = 1.5f;
    [SerializeField] private float _jumpHeight = 1.5f;
    [SerializeField] private float _gravity = -15f;
    
    [Header("Look")]
    [SerializeField] private float _lookSensitivity = 2f;
    [SerializeField] private float _maxLookAngle = 85f;
    [SerializeField] private Transform _cameraHolder;
    
    [Header("Ground Check")]
    [SerializeField] private float _groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask _groundMask;
    #endregion

    #region Components
    private CharacterController _characterController;
    private PlayerInput _playerInput;
    private Camera _playerCamera;
    #endregion

    #region State
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _sprintInput;
    private bool _jumpInput;
    
    private Vector3 _velocity;
    private float _cameraPitch;
    private bool _isGrounded;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsOwner)
        {
            // Enable input and camera for local player only
            SetupLocalPlayer();
            Debug.Log($"[PlayerController] Local player spawned: {OwnerClientId}");
        }
        else
        {
            // Disable input for non-local players
            if (_playerInput != null)
                _playerInput.enabled = false;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        
        HandleGroundCheck();
        HandleMovement();
        HandleLook();
    }
    #endregion

    #region Setup
    private void SetupLocalPlayer()
    {
        // Find or create camera
        if (_cameraHolder != null)
        {
            _playerCamera = _cameraHolder.GetComponentInChildren<Camera>();
            if (_playerCamera == null)
            {
                // Create camera if not present
                var cameraObj = new GameObject("PlayerCamera");
                cameraObj.transform.SetParent(_cameraHolder);
                cameraObj.transform.localPosition = Vector3.zero;
                cameraObj.transform.localRotation = Quaternion.identity;
                _playerCamera = cameraObj.AddComponent<Camera>();
                cameraObj.AddComponent<AudioListener>();
            }
            _playerCamera.gameObject.SetActive(true);
        }
        
        // Lock cursor for FPS controls
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Enable input
        if (_playerInput != null)
        {
            _playerInput.enabled = true;
        }
    }
    #endregion

    #region Input Handlers (called by PlayerInput component)
    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        _lookInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        _sprintInput = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        _jumpInput = value.isPressed;
    }
    #endregion

    #region Movement
    private void HandleGroundCheck()
    {
        _isGrounded = Physics.CheckSphere(
            transform.position + Vector3.down * (_characterController.height / 2f),
            _groundCheckDistance,
            _groundMask
        );
        
        // Reset vertical velocity when grounded
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // Small negative to keep grounded
        }
    }

    private void HandleMovement()
    {
        // Calculate move direction relative to player rotation
        float speed = _sprintInput ? _moveSpeed * _sprintMultiplier : _moveSpeed;
        Vector3 moveDirection = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        
        // Apply horizontal movement
        _characterController.Move(moveDirection * speed * Time.deltaTime);
        
        // Handle jumping
        if (_jumpInput && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            _jumpInput = false;
        }
        
        // Apply gravity
        _velocity.y += _gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        // Horizontal rotation (rotate player)
        float yawDelta = _lookInput.x * _lookSensitivity;
        transform.Rotate(Vector3.up, yawDelta);
        
        // Vertical rotation (tilt camera)
        _cameraPitch -= _lookInput.y * _lookSensitivity;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -_maxLookAngle, _maxLookAngle);
        
        if (_cameraHolder != null)
        {
            _cameraHolder.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
        }
    }

    private void HandleJump()
    {
        if (_jumpInput && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            _jumpInput = false;
        }
    }

    private void HandleSprint()
    {
        if (_sprintInput)
        {
            _moveSpeed = _sprintMultiplier;
        }
        else
        {
            _moveSpeed = _moveSpeed / _sprintMultiplier;
        }
    }
    #endregion

    #region Public API
    public bool IsGrounded => _isGrounded;
    public bool IsSprinting => _sprintInput && _moveInput.magnitude > 0.1f;
    public Vector3 Velocity => _characterController.velocity;
    #endregion
}
