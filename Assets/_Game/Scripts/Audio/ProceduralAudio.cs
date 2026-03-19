using UnityEngine;

/// <summary>
/// Generates simple sound effects procedurally (no audio files needed).
/// </summary>
public class ProceduralAudio : MonoBehaviour
{
    public static ProceduralAudio Instance { get; private set; }

    AudioSource _sfxSource;
    AudioSource _ambientSource;

    // Cached clips
    AudioClip _typeClick;
    AudioClip _phoneRing;
    AudioClip _paperFlip;
    AudioClip _stampHit;
    AudioClip _clockTick;
    AudioClip _ambientHum;

    void Awake()
    {
        Instance = this;
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _sfxSource.volume = 0.3f;

        _ambientSource = gameObject.AddComponent<AudioSource>();
        _ambientSource.playOnAwake = false;
        _ambientSource.loop = true;
        _ambientSource.volume = 0.08f;

        GenerateClips();
    }

    void GenerateClips()
    {
        _typeClick = GenerateClick(0.02f, 4000f, 0.6f);
        _phoneRing = GenerateRing(0.4f, 800f, 1200f);
        _paperFlip = GenerateNoise(0.15f, 0.4f);
        _stampHit = GenerateThud(0.12f, 120f);
        _clockTick = GenerateClick(0.015f, 2500f, 0.3f);
        _ambientHum = GenerateHum(4f, 80f);
    }

    // ─── PUBLIC API ───
    public void PlayType() => PlayClip(_typeClick, Random.Range(0.8f, 1.2f));
    public void PlayPhoneRing() => PlayClip(_phoneRing, 1f, 0.5f);
    public void PlayPaperFlip() => PlayClip(_paperFlip, Random.Range(0.9f, 1.1f));
    public void PlayStamp() => PlayClip(_stampHit, 1f, 0.7f);
    public void PlayClockTick() => PlayClip(_clockTick, Random.Range(0.95f, 1.05f), 0.15f);

    public void StartAmbient()
    {
        if (_ambientSource.isPlaying) return;
        _ambientSource.clip = _ambientHum;
        _ambientSource.Play();
    }

    public void StopAmbient() => _ambientSource.Stop();

    void PlayClip(AudioClip clip, float pitch = 1f, float vol = -1f)
    {
        if (clip == null) return;
        _sfxSource.pitch = pitch;
        if (vol >= 0f) _sfxSource.volume = vol;
        else _sfxSource.volume = 0.3f;
        _sfxSource.PlayOneShot(clip);
    }

    // ─── GENERATORS ───
    static AudioClip GenerateClick(float duration, float freq, float decay)
    {
        int samples = (int)(44100 * duration);
        var data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = i / 44100f;
            float env = Mathf.Exp(-t * decay * 100f);
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.5f;
        }
        return MakeClip("click", data);
    }

    static AudioClip GenerateRing(float duration, float f1, float f2)
    {
        int samples = (int)(44100 * duration);
        var data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = i / 44100f;
            float env = Mathf.Sin(Mathf.PI * t / duration); // bell envelope
            data[i] = (Mathf.Sin(2f * Mathf.PI * f1 * t) + Mathf.Sin(2f * Mathf.PI * f2 * t)) * 0.25f * env;
        }
        return MakeClip("ring", data);
    }

    static AudioClip GenerateNoise(float duration, float decay)
    {
        int samples = (int)(44100 * duration);
        var data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = i / 44100f;
            float env = Mathf.Exp(-t * decay * 20f);
            data[i] = (Random.value * 2f - 1f) * env * 0.3f;
        }
        return MakeClip("noise", data);
    }

    static AudioClip GenerateThud(float duration, float freq)
    {
        int samples = (int)(44100 * duration);
        var data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = i / 44100f;
            float env = Mathf.Exp(-t * 40f);
            float f = freq * Mathf.Exp(-t * 20f); // pitch drops
            data[i] = Mathf.Sin(2f * Mathf.PI * f * t) * env * 0.6f;
        }
        return MakeClip("thud", data);
    }

    static AudioClip GenerateHum(float duration, float freq)
    {
        int samples = (int)(44100 * duration);
        var data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = i / 44100f;
            data[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.3f
                     + (Random.value * 2f - 1f) * 0.05f) * 0.15f;
        }
        return MakeClip("hum", data);
    }

    static AudioClip MakeClip(string name, float[] data)
    {
        var clip = AudioClip.Create(name, data.Length, 1, 44100, false);
        clip.SetData(data, 0);
        return clip;
    }
}
