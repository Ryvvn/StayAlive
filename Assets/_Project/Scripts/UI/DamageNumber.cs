using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays damage numbers floating above enemies/players when hit.
/// Create as prefab and instantiate via DamageNumberSpawner.
/// </summary>
public class DamageNumber : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _lifetime = 1f;
    [SerializeField] private float _floatSpeed = 1f;
    [SerializeField] private float _fadeSpeed = 1f;
    [SerializeField] private AnimationCurve _scaleCurve;
    
    [Header("Colors")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _criticalColor = Color.yellow;
    [SerializeField] private Color _healColor = Color.green;
    
    private float _timer;
    private Vector3 _startScale;
    private CanvasGroup _canvasGroup;
    
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        _startScale = transform.localScale;
    }
    
    public void Initialize(float damage, bool isCritical = false, bool isHeal = false)
    {
        if (_text != null)
        {
            _text.text = isHeal ? $"+{Mathf.RoundToInt(damage)}" : Mathf.RoundToInt(damage).ToString();
            _text.color = isHeal ? _healColor : (isCritical ? _criticalColor : _normalColor);
        }
        
        _timer = 0f;
    }
    
    private void Update()
    {
        _timer += Time.deltaTime;
        
        // Float up
        transform.position += Vector3.up * _floatSpeed * Time.deltaTime;
        
        // Scale animation
        if (_scaleCurve != null && _scaleCurve.length > 0)
        {
            float scaleMultiplier = _scaleCurve.Evaluate(_timer / _lifetime);
            transform.localScale = _startScale * scaleMultiplier;
        }
        
        // Fade out
        float fadeProgress = _timer / _lifetime;
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f - fadeProgress;
        }
        
        // Destroy when done
        if (_timer >= _lifetime)
        {
            Destroy(gameObject);
        }
        
        // Always face camera
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}

/// <summary>
/// Spawns damage numbers at world positions.
/// Singleton - attach to a permanent object.
/// </summary>
public class DamageNumberSpawner : MonoBehaviour
{
    public static DamageNumberSpawner Instance { get; private set; }
    
    [SerializeField] private GameObject _damageNumberPrefab;
    [SerializeField] private Canvas _worldCanvas;
    
    private void Awake()
    {
        Instance = this;
        
        // Create world canvas if not assigned
        if (_worldCanvas == null)
        {
            var canvasObj = new GameObject("DamageNumberCanvas");
            _worldCanvas = canvasObj.AddComponent<Canvas>();
            _worldCanvas.renderMode = RenderMode.WorldSpace;
            canvasObj.AddComponent<CanvasScaler>();
        }
    }
    
    public void SpawnDamageNumber(Vector3 worldPosition, float damage, bool isCritical = false, bool isHeal = false)
    {
        if (_damageNumberPrefab == null) return;
        
        // Add slight random offset
        Vector3 offset = new Vector3(
            Random.Range(-0.3f, 0.3f),
            Random.Range(0f, 0.5f),
            Random.Range(-0.3f, 0.3f)
        );
        
        var instance = Instantiate(_damageNumberPrefab, worldPosition + offset, Quaternion.identity, _worldCanvas.transform);
        
        var damageNumber = instance.GetComponent<DamageNumber>();
        if (damageNumber != null)
        {
            damageNumber.Initialize(damage, isCritical, isHeal);
        }
    }
}
