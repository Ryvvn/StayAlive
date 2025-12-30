using UnityEngine;

/// <summary>
/// Controls lighting with Minecraft-style sun/moon rotation.
/// Sun rises in the east, arcs across the sky, sets in the west.
/// Moon does the same during night.
/// </summary>
public class LightingController : MonoBehaviour
{
    #region Singleton
    public static LightingController Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    #region Configuration
    [Header("Light Reference")]
    [SerializeField] private Light _directionalLight;
    
    [Header("Day Settings")]
    [SerializeField] private Color _dayLightColor = new Color(1f, 0.95f, 0.84f);
    [SerializeField] private float _dayIntensity = 1.2f;
    [SerializeField] private Color _dayAmbientColor = new Color(0.5f, 0.6f, 0.7f);
    [SerializeField] private Gradient _daySkyGradient;
    
    [Header("Night Settings")]
    [SerializeField] private Color _nightLightColor = new Color(0.4f, 0.5f, 0.7f);
    [SerializeField] private float _nightIntensity = 0.25f;
    [SerializeField] private Color _nightAmbientColor = new Color(0.08f, 0.08f, 0.15f);
    
    [Header("Sunrise/Sunset Colors")]
    [SerializeField] private Color _sunriseColor = new Color(1f, 0.6f, 0.3f);
    [SerializeField] private Color _sunsetColor = new Color(1f, 0.4f, 0.2f);
    
    [Header("Rotation Settings")]
    [SerializeField] private float _rotationYAxis = 170f; // Direction sun travels
    #endregion

    #region State
    private bool _isDay = true;
    private float _currentRotation = 0f; // 0-360 degrees
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        // Find directional light if not assigned
        if (_directionalLight == null)
        {
            _directionalLight = FindObjectOfType<Light>();
        }
        
        // Setup default gradient if not configured
        if (_daySkyGradient == null || _daySkyGradient.colorKeys.Length == 0)
        {
            SetupDefaultGradient();
        }
        
        // Subscribe to events
        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.OnDayStart += OnDayStart;
            DayNightManager.Instance.OnNightStart += OnNightStart;
            DayNightManager.Instance.OnTimeUpdated += OnTimeUpdated;
        }
        
        // Initialize
        _isDay = true;
        UpdateLighting(0f);
    }

    private void OnDestroy()
    {
        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.OnDayStart -= OnDayStart;
            DayNightManager.Instance.OnNightStart -= OnNightStart;
            DayNightManager.Instance.OnTimeUpdated -= OnTimeUpdated;
        }
    }
    #endregion

    #region Event Handlers
    private void OnDayStart()
    {
        Debug.Log("[LightingController] Day started - Sun rising");
        _isDay = true;
    }

    private void OnNightStart()
    {
        Debug.Log("[LightingController] Night started - Moon rising");
        _isDay = false;
    }

    private void OnTimeUpdated(float normalizedTime)
    {
        UpdateLighting(normalizedTime);
    }
    #endregion

    #region Lighting Update
    private void UpdateLighting(float normalizedTime)
    {
        if (_directionalLight == null) return;
        
        // Calculate sun/moon rotation (0 = horizon rise, 0.5 = zenith, 1 = horizon set)
        // Angle: -90 (below horizon) -> 0 (horizon) -> 90 (zenith) -> 180 (horizon) -> 270 (below)
        float rotationAngle;
        
        if (_isDay)
        {
            // Day: Sun rises from -10° to peaks at 80°, then sets to 170°
            // normalizedTime 0 = sunrise, 0.5 = noon, 1 = sunset
            rotationAngle = Mathf.Lerp(-10f, 170f, normalizedTime);
            ApplyDayLighting(normalizedTime, rotationAngle);
        }
        else
        {
            // Night: Moon rises from 170° to peaks at 260°, then sets to 350°
            rotationAngle = Mathf.Lerp(170f, 350f, normalizedTime);
            ApplyNightLighting(normalizedTime, rotationAngle);
        }
        
        // Apply rotation
        _directionalLight.transform.rotation = Quaternion.Euler(rotationAngle, _rotationYAxis, 0f);
        _currentRotation = rotationAngle;
    }

    private void ApplyDayLighting(float t, float sunAngle)
    {
        // Intensity curve: lower at sunrise/sunset, highest at noon
        float intensityCurve = Mathf.Sin(t * Mathf.PI); // 0->1->0
        float intensity = Mathf.Lerp(0.4f, _dayIntensity, intensityCurve);
        _directionalLight.intensity = intensity;
        
        // Color: orange at sunrise/sunset, white at noon
        Color lightColor;
        if (t < 0.15f)
        {
            // Sunrise (0 - 0.15)
            float sunriseT = t / 0.15f;
            lightColor = Color.Lerp(_sunriseColor, _dayLightColor, sunriseT);
        }
        else if (t > 0.85f)
        {
            // Sunset (0.85 - 1)
            float sunsetT = (t - 0.85f) / 0.15f;
            lightColor = Color.Lerp(_dayLightColor, _sunsetColor, sunsetT);
        }
        else
        {
            // Midday
            lightColor = _dayLightColor;
        }
        _directionalLight.color = lightColor;
        
        // Ambient
        float ambientIntensity = Mathf.Lerp(0.3f, 1f, intensityCurve);
        RenderSettings.ambientLight = _dayAmbientColor * ambientIntensity;
        RenderSettings.ambientIntensity = ambientIntensity;
    }

    private void ApplyNightLighting(float t, float moonAngle)
    {
        // Intensity curve: slightly brighter when moon is high
        float intensityCurve = Mathf.Sin(t * Mathf.PI);
        float intensity = Mathf.Lerp(0.1f, _nightIntensity, intensityCurve);
        _directionalLight.intensity = intensity;
        
        // Night color stays consistent (moonlight)
        _directionalLight.color = _nightLightColor;
        
        // Dark ambient
        RenderSettings.ambientLight = _nightAmbientColor;
        RenderSettings.ambientIntensity = 0.5f;
    }
    #endregion

    #region Setup
    private void SetupDefaultGradient()
    {
        _daySkyGradient = new Gradient();
        
        GradientColorKey[] colorKeys = new GradientColorKey[4];
        colorKeys[0] = new GradientColorKey(new Color(0.8f, 0.5f, 0.3f), 0f);    // Sunrise
        colorKeys[1] = new GradientColorKey(new Color(0.5f, 0.7f, 1f), 0.25f);   // Morning
        colorKeys[2] = new GradientColorKey(new Color(0.5f, 0.7f, 1f), 0.75f);   // Afternoon
        colorKeys[3] = new GradientColorKey(new Color(0.9f, 0.5f, 0.3f), 1f);    // Sunset
        
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1f, 0f);
        alphaKeys[1] = new GradientAlphaKey(1f, 1f);
        
        _daySkyGradient.SetKeys(colorKeys, alphaKeys);
    }
    #endregion

    #region Debug
    [Header("Debug")]
    [SerializeField] private bool _debugMode;
    [Range(0f, 1f)]
    [SerializeField] private float _debugTime;
    [SerializeField] private bool _debugIsDay = true;
    
    private void OnValidate()
    {
        if (_debugMode && _directionalLight != null)
        {
            _isDay = _debugIsDay;
            UpdateLighting(_debugTime);
        }
    }
    
    [ContextMenu("Test Sunrise")]
    private void TestSunrise() { _isDay = true; UpdateLighting(0f); }
    
    [ContextMenu("Test Noon")]
    private void TestNoon() { _isDay = true; UpdateLighting(0.5f); }
    
    [ContextMenu("Test Sunset")]
    private void TestSunset() { _isDay = true; UpdateLighting(0.95f); }
    
    [ContextMenu("Test Midnight")]
    private void TestMidnight() { _isDay = false; UpdateLighting(0.5f); }
    #endregion
}
