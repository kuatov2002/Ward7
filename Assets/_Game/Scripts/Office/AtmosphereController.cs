using UnityEngine;

public class AtmosphereController : MonoBehaviour
{
    public static AtmosphereController Instance { get; private set; }

    Light _dirLight;
    Light _deskLamp;
    ParticleSystem _dust;

    void Awake() => Instance = this;

    public void Init(Light dirLight, Light deskLamp, Transform cam)
    {
        _dirLight = dirLight;
        _deskLamp = deskLamp;
        CreateDustParticles();
    }

    // No camera sway in PSX style — camera is static

    public void SetDayLighting(int day)
    {
        if (_dirLight == null || _deskLamp == null) return;

        switch (day)
        {
            case 0:
            case 1: // Monday - cold morning, greenish
                _dirLight.color = new Color(0.6f, 0.65f, 0.55f);
                _dirLight.intensity = 0.4f;
                _deskLamp.intensity = 1.2f;
                _deskLamp.color = new Color(1f, 0.85f, 0.5f);
                break;
            case 2: // Tuesday - slightly warmer
                _dirLight.color = new Color(0.65f, 0.6f, 0.5f);
                _dirLight.intensity = 0.35f;
                _deskLamp.intensity = 1.0f;
                _deskLamp.color = new Color(1f, 0.9f, 0.6f);
                break;
            case 3: // Wednesday - amber afternoon
                _dirLight.color = new Color(0.7f, 0.55f, 0.35f);
                _dirLight.intensity = 0.3f;
                _deskLamp.intensity = 1.4f;
                _deskLamp.color = new Color(1f, 0.8f, 0.45f);
                break;
            case 4: // Thursday - dark, oppressive
                _dirLight.color = new Color(0.5f, 0.4f, 0.3f);
                _dirLight.intensity = 0.2f;
                _deskLamp.intensity = 1.8f;
                _deskLamp.color = new Color(1f, 0.75f, 0.35f);
                break;
            case 5: // Friday - near dark, only lamp
                _dirLight.color = new Color(0.35f, 0.3f, 0.25f);
                _dirLight.intensity = 0.1f;
                _deskLamp.intensity = 2.2f;
                _deskLamp.color = new Color(1f, 0.7f, 0.3f);
                break;
        }
    }

    void CreateDustParticles()
    {
        var dustGo = new GameObject("DustParticles");
        dustGo.transform.position = new Vector3(0f, 1.5f, 0.6f);
        _dust = dustGo.AddComponent<ParticleSystem>();

        var main = _dust.main;
        main.maxParticles = 25;
        main.startLifetime = 12f;
        main.startSpeed = 0.008f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.003f, 0.008f);
        main.startColor = new Color(0.8f, 0.9f, 0.6f, 0.12f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.002f;

        var emission = _dust.emission;
        emission.rateOverTime = 3f;

        var shape = _dust.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(2f, 1f, 1.5f);

        var pRenderer = dustGo.GetComponent<ParticleSystemRenderer>();
        pRenderer.material = new Material(pRenderer.material);
        pRenderer.material.color = new Color(0.7f, 0.8f, 0.5f, 0.15f);
    }
}
