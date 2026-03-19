using UnityEngine;

/// <summary>
/// Manages atmosphere: dust particles, day-based lighting, camera idle sway.
/// </summary>
public class AtmosphereController : MonoBehaviour
{
    public static AtmosphereController Instance { get; private set; }

    Light _dirLight;
    Light _deskLamp;
    ParticleSystem _dust;
    Transform _camTransform;

    // Idle sway
    float _swayTime;
    Vector3 _camBasePos;
    Quaternion _camBaseRot;

    void Awake() => Instance = this;

    public void Init(Light dirLight, Light deskLamp, Transform cam)
    {
        _dirLight = dirLight;
        _deskLamp = deskLamp;
        _camTransform = cam;
        _camBasePos = cam.position;
        _camBaseRot = cam.rotation;
        CreateDustParticles();
    }

    void Update()
    {
        if (_camTransform == null) return;
        if (FirstPersonLook.Instance != null && UIManager.Instance != null && UIManager.Instance.IsPanelOpen)
            return; // no sway when reading

        _swayTime += Time.deltaTime;
        float swayX = Mathf.Sin(_swayTime * 0.3f) * 0.002f;
        float swayY = Mathf.Sin(_swayTime * 0.5f) * 0.001f;
        _camTransform.position = _camBasePos + new Vector3(swayX, swayY, 0f);
    }

    public void SetDayLighting(int day)
    {
        if (_dirLight == null || _deskLamp == null) return;

        switch (day)
        {
            case 0: // Monday morning intro
            case 1: // Monday
                _dirLight.color = new Color(0.95f, 0.9f, 0.8f);
                _dirLight.intensity = 0.7f;
                _deskLamp.intensity = 0.8f;
                _deskLamp.color = new Color(1f, 0.95f, 0.8f);
                break;
            case 2: // Tuesday - daytime
                _dirLight.color = new Color(0.9f, 0.9f, 0.9f);
                _dirLight.intensity = 0.8f;
                _deskLamp.intensity = 0.6f;
                break;
            case 3: // Wednesday - afternoon
                _dirLight.color = new Color(1f, 0.85f, 0.7f);
                _dirLight.intensity = 0.6f;
                _deskLamp.intensity = 1f;
                _deskLamp.color = new Color(1f, 0.9f, 0.7f);
                break;
            case 4: // Thursday - late afternoon
                _dirLight.color = new Color(0.9f, 0.7f, 0.5f);
                _dirLight.intensity = 0.4f;
                _deskLamp.intensity = 1.4f;
                _deskLamp.color = new Color(1f, 0.85f, 0.6f);
                break;
            case 5: // Friday - evening, tense
                _dirLight.color = new Color(0.6f, 0.5f, 0.45f);
                _dirLight.intensity = 0.25f;
                _deskLamp.intensity = 1.8f;
                _deskLamp.color = new Color(1f, 0.8f, 0.5f);
                break;
        }
    }

    void CreateDustParticles()
    {
        var dustGo = new GameObject("DustParticles");
        dustGo.transform.position = new Vector3(0f, 1.5f, 0.6f);
        _dust = dustGo.AddComponent<ParticleSystem>();

        var main = _dust.main;
        main.maxParticles = 40;
        main.startLifetime = 10f;
        main.startSpeed = 0.01f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.002f, 0.005f);
        main.startColor = new Color(1f, 1f, 0.9f, 0.15f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.003f;

        var emission = _dust.emission;
        emission.rateOverTime = 6f;

        var shape = _dust.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(2f, 1f, 1.5f);

        // Use default particle material — leave as is, Unity assigns a default
        var pRenderer = dustGo.GetComponent<ParticleSystemRenderer>();
        pRenderer.material = new Material(pRenderer.material);
        pRenderer.material.color = new Color(1f, 1f, 0.9f, 0.3f);
    }
}
