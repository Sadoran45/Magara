using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Applies smooth Perlin noise displacement over time (position).
/// Supports Odin Inspector preview in Edit Mode.
/// </summary>
[DisallowMultipleComponent]
public class PerlinNoiseMovement : MonoBehaviour
{
    public enum SpaceMode { Local, World }

    [Header("Target Space")]
    public SpaceMode space = SpaceMode.Local;

    [Header("Motion Shape")]
    public Vector3 amplitude = new Vector3(0.25f, 0.25f, 0.25f);
    public Vector3 frequency = new Vector3(0.35f, 0.40f, 0.30f);
    public float speed = 1.0f;

    [Header("Options")]
    public bool turbulence = false;
    public bool rebaseOnEnable = true;

    [Header("Noise Phase (seed offsets)")]
    public int seed = 12345;
    public bool randomizePhase = true;

    // Odin inspector: preview toggle
    [ShowInInspector, LabelText("Preview in Edit Mode"), GUIColor(0.7f, 1f, 0.7f)]
    [ToggleLeft]
    public bool Preview
    {
        get => _preview;
        set
        {
            if (_preview == value) return;
            _preview = value;
#if UNITY_EDITOR
            if (_preview)
            {
                CacheBase();
                EditorApplication.update += EditorUpdate;
            }
            else
            {
                EditorApplication.update -= EditorUpdate;
                RestoreBase();
            }
#endif
        }
    }

    // --- internals ---
    private Vector3 _baseLocal;
    private Vector3 _baseWorld;
    private bool _preview = false;

    private Vector2 _ox, _oy, _oz;

    void Awake()
    {
        InitOffsets();
        CacheBase();
    }

    void OnEnable()
    {
        if (rebaseOnEnable) CacheBase();
    }

    void CacheBase()
    {
        _baseLocal = transform.localPosition;
        _baseWorld = transform.position;
    }

    void RestoreBase()
    {
        if (space == SpaceMode.Local) transform.localPosition = _baseLocal;
        else transform.position = _baseWorld;
    }

    void InitOffsets()
    {
        System.Random rng = randomizePhase ? new System.Random() : new System.Random(seed);
        _ox = new Vector2((float)rng.NextDouble() * 1000f, (float)rng.NextDouble() * 1000f);
        _oy = new Vector2((float)rng.NextDouble() * 1000f, (float)rng.NextDouble() * 1000f);
        _oz = new Vector2((float)rng.NextDouble() * 1000f, (float)rng.NextDouble() * 1000f);
    }

    void Update()
    {
        if (!Application.isPlaying) return; // only runtime
        ApplyDisplacement(Time.time);
    }

#if UNITY_EDITOR
    private void EditorUpdate()
    {
        if (!_preview || Application.isPlaying) return;
        ApplyDisplacement((float)EditorApplication.timeSinceStartup);
        // Scene not marked dirty because transform changes are transient
        if (space == SpaceMode.Local)
            transform.hasChanged = false;
    }
#endif

    private void ApplyDisplacement(float time)
    {
        float t = time * Mathf.Max(speed, 0f);

        Vector3 disp = new Vector3(
            amplitude.x * Noise1D(t * frequency.x, _ox),
            amplitude.y * Noise1D(t * frequency.y, _oy),
            amplitude.z * Noise1D(t * frequency.z, _oz)
        );

        if (space == SpaceMode.Local)
            transform.localPosition = _baseLocal + disp;
        else
            transform.position = _baseWorld + disp;
    }

    private float Noise1D(float tScaled, Vector2 offset)
    {
        float sample = Mathf.PerlinNoise(offset.x + tScaled, offset.y + tScaled);
        sample = sample * 2f - 1f; // [-1,1]
        return turbulence ? Mathf.Abs(sample) : sample;
    }
}
