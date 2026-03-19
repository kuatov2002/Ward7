using System.Collections;
using UnityEngine;

/// <summary>
/// Procedural detective/noir soundtrack. 24-bar, D minor, 85 BPM.
/// Dark, tense, investigative mood. Piano motif + bass + brushed drums + pad + vibraphone.
/// A(investigation) → B(tension) → C(revelation) → C2(intensity) → D(reflection) → A2(return)
/// </summary>
public class ProceduralMusic : MonoBehaviour
{
    public static ProceduralMusic Instance { get; private set; }

    [SerializeField] float masterVolume = 0.22f;
    [SerializeField] float bpm = 95f; // groovy detective pace

    AudioSource _subSource, _bassSource, _padSource, _pianoSource, _vibSource, _drumSource;

    bool _playing;
    float _intensity = 0.5f;
    int _currentBar;
    float _sidechain = 1f;
    float _filterOpenness;
    float _lastPianoFreq;

    // D minor: D3(0) E3(1) F3(2) G3(3) A3(4) Bb3(5) C4(6) D4(7) E4(8) F4(9) G4(10) A4(11) Bb4(12) C5(13) D5(14)
    static readonly float[] N = {
        146.83f, 164.81f, 174.61f, 196.00f, 220.00f, 233.08f, 261.63f,
        293.66f, 329.63f, 349.23f, 392.00f, 440.00f, 466.16f, 523.25f, 587.33f
    };

    // Chord roots: Dm(0) - Bb(5) - Gm(3) - A(4) — classic noir tension
    static readonly int[] Roots = {
        0,0,5,4, 0,0,5,4, 0,5,3,4, 0,5,3,4, 0,0,5,4, 0,0,5,4
    };

    // ═══════════════════════════════════════════
    // THE HOOK: D-F-A-G motif (rise then unresolved fall)
    // This 4-note motif is the identity of the soundtrack
    // It appears in piano, gets echoed by vibraphone, and the bass answers it
    // ═══════════════════════════════════════════

    // Bass riff (8ths) — syncopated, groovy, MEMORABLE
    static readonly int[] X = {-1,-1,-1,-1,-1,-1,-1,-1};
    static readonly int[][] Bass = {
        // A: the bass riff teaser — just the rhythm, sparse
        new[]{0,-1,-1,-1,-1,-1,-1,-1}, new[]{0,-1,-1,0,-1,-1,-1,-1},
        new[]{5,-1,-1,-1,-1,-1,-1,-1}, new[]{4,-1,-1,-1,3,-1,0,-1},
        // B: THE BASS RIFF — this is catchy, head-bobbing
        // D-_-D-F-_-G-A-_ (the hook in bass form)
        new[]{0,-1,0,2,-1,3,4,-1}, new[]{0,-1,0,2,-1,3,4,-1},
        new[]{5,-1,5,3,-1,4,0,-1}, new[]{4,-1,4,3,-1,0,4,-1},
        // C: bass keeps the riff but with octave jumps
        new[]{0,-1,0,2,-1,3,4,-1}, new[]{0,7,0,2,-1,3,4,-1},
        new[]{5,-1,5,3,-1,4,0,-1}, new[]{4,-1,4,3,-1,0,7,-1},
        // C2: bass doubles down, more insistent
        new[]{0,0,-1,0,2,3,4,-1}, new[]{0,0,-1,0,2,3,7,-1},
        new[]{5,5,-1,5,3,4,0,-1}, new[]{4,4,-1,4,3,0,4,-1},
        // D: stripped to just roots, tension
        new[]{0,-1,-1,-1,-1,-1,-1,-1}, X,
        new[]{5,-1,-1,-1,-1,-1,-1,-1}, new[]{4,-1,-1,-1,-1,-1,0,-1},
        // A2: riff returns
        new[]{0,-1,0,2,-1,3,4,-1}, new[]{0,-1,0,2,-1,3,4,-1},
        new[]{5,-1,5,3,-1,4,0,-1}, new[]{4,-1,4,3,-1,0,4,-1},
    };

    // Piano: THE HOOK — D4-F4-A4-G4 (can you hum it?)
    // Bar 1: state the hook. Bar 2: echo/answer. Bar 3: develop higher. Bar 4: resolve.
    static readonly int[][] PianoC = {
        // Hook: D4...F4..A4.G4.. (long-short-short-long rhythm via rests)
        new[]{7,-1,-1,9,-1,11,10,-1},     // D4---F4--A4.G4-- THE HOOK
        new[]{11,-1,10,9,-1,7,-1,-1},     // A4--G4.F4--D4--- (answer: descending)
        new[]{7,-1,-1,9,-1,11,12,-1},     // D4---F4--A4.Bb4- (hook +1: reaches higher!)
        new[]{11,-1,10,9,-1,7,-1,-1},     // A4--G4.F4--D4--- (same answer: anchor)
    };

    // Piano C2: hook goes UP — climax, the "revelation" moment
    static readonly int[][] PianoC2 = {
        new[]{14,-1,-1,12,-1,11,10,-1},   // D5---Bb4--A4.G4- (hook from the top)
        new[]{11,-1,12,14,-1,11,-1,-1},   // A4--Bb4.D5--A4-- (reaching upward)
        new[]{14,-1,-1,12,-1,14,14,-1},   // D5---Bb4--D5!D5! (PEAK: double D5, triumphant)
        new[]{12,11,9,-1,7,-1,7,-1},      // Bb4.A4.F4--D4--D4 (cascading resolve home)
    };

    // Vibraphone: echoes the hook motif in arpeggiated form (16ths)
    // Based on the D-F-A-G intervals woven into chord tones
    static readonly int[][] Vib = {
        new[]{0,4,7,9, 11,10,7,4,  0,4,7,9, 11,10,7,0},   // Dm: weaves D-F-A-G hook
        new[]{0,4,7,9, 11,10,7,4,  0,7,11,9, 7,4,0,4},    // echo variation
        new[]{5,9,12,9, 5,3,7,10,  5,9,12,9, 10,7,3,5},   // Bb: darker
        new[]{3,7,10,11, 4,7,0,4,  3,7,10,7, 0,4,7,0},    // Gm→Dm: resolve
    };

    // Drums: noir groove — confident, swinging detective walk
    // 0=rest 1=kick 2=brush 3=ride 4=kick+ride 5=brush+ride 6=bell 7=kick+bell
    static readonly int[][] Dr = {
        // A: just ride, breathing
        new[]{0,0,3,0, 0,0,3,0, 0,0,3,0, 0,0,3,0},
        new[]{0,0,3,0, 0,0,3,0, 0,0,3,0, 0,0,3,6},
        new[]{0,0,3,0, 0,0,3,0, 0,0,3,0, 0,0,3,0},
        new[]{0,0,3,0, 0,0,6,0, 0,0,3,0, 1,0,3,0},
        // B: groove kicks in — head-bobbing pattern
        new[]{4,0,3,0, 2,0,3,0, 0,0,3,0, 2,0,3,0},
        new[]{4,0,3,0, 2,0,3,0, 4,0,3,0, 2,0,3,6},
        new[]{4,0,3,0, 2,0,3,0, 0,0,3,0, 2,0,6,0},
        new[]{4,0,3,0, 5,0,3,0, 1,0,1,0, 2,0,2,0},
        // C: full groove with the snare hitting the hook rhythm
        new[]{4,0,3,3, 5,0,3,0, 0,0,3,3, 5,0,3,0},
        new[]{4,0,3,3, 5,0,3,0, 4,0,3,3, 5,0,6,0},
        new[]{4,0,3,3, 5,0,3,0, 0,0,3,3, 5,0,3,0},
        new[]{4,0,3,6, 5,0,3,0, 4,0,4,0, 5,6,5,0},
        // C2: peak intensity, double kicks
        new[]{7,0,3,3, 5,0,3,3, 4,0,3,3, 5,0,3,0},
        new[]{4,0,3,3, 5,0,3,0, 7,0,3,3, 5,0,6,0},
        new[]{7,0,3,3, 5,0,3,3, 4,0,3,3, 5,0,3,0},
        new[]{4,0,4,0, 5,0,4,0, 1,2,1,2, 1,5,1,0},
        // D: breakdown, just heartbeat kick
        new[]{1,0,0,0, 0,0,0,0, 0,0,0,0, 0,0,0,0},
        new[]{0,0,3,0, 0,0,0,0, 0,0,3,0, 0,0,0,0},
        new[]{1,0,3,0, 0,0,3,0, 1,0,3,0, 0,0,3,0},
        new[]{4,0,3,0, 2,0,3,0, 4,0,3,0, 2,0,3,6},
        // A2: groove returns, familiarity
        new[]{4,0,3,0, 2,0,3,0, 0,0,3,0, 2,0,3,0},
        new[]{4,0,3,0, 2,0,3,0, 0,0,3,0, 2,0,3,6},
        new[]{4,0,3,0, 2,0,3,0, 0,0,3,0, 2,0,3,0},
        new[]{4,0,3,0, 2,0,6,0, 0,0,3,0, 1,0,1,0},
    };

    AudioClip _kickClip, _brushClip, _rideClip, _rideBellClip;

    void Awake()
    {
        Instance = this;
        _subSource = Src("Sub", 0.3f);
        _bassSource = Src("Bass", 0.2f);
        _padSource = Src("Pad", 0.12f);
        _pianoSource = Src("Piano", 0.18f);
        _vibSource = Src("Vib", 0.05f);
        _drumSource = Src("Drums", 0.25f);
        GenDrums();
    }

    AudioSource Src(string n, float v)
    {
        var go = new GameObject($"M_{n}");
        go.transform.SetParent(transform);
        var s = go.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.volume = v * masterVolume;
        s.spatialBlend = 0f;
        return s;
    }

    public void StartMusic() { if (_playing) return; _playing = true; _currentBar = 0; _lastPianoFreq = 0; StartCoroutine(Loop()); StartCoroutine(SidechainDecay()); }
    public void StopMusic() { _playing = false; StopAllCoroutines(); }

    public void SetIntensity(float t)
    {
        _intensity = Mathf.Clamp01(t);
        _subSource.volume = 0.3f * masterVolume;
        _padSource.volume = Mathf.Lerp(0.12f, 0.06f, t) * masterVolume;
        _bassSource.volume = Mathf.Lerp(0.06f, 0.2f, t) * masterVolume;
        _drumSource.volume = Mathf.Lerp(0.06f, 0.25f, t) * masterVolume;
        _vibSource.volume = Mathf.Lerp(0.02f, 0.05f, t) * masterVolume;
        _pianoSource.volume = Mathf.Lerp(0f, 0.18f, t) * masterVolume;
    }

    IEnumerator SidechainDecay()
    {
        while (_playing)
        {
            _sidechain = Mathf.MoveTowards(_sidechain, 1f, Time.deltaTime * 6f);
            if (_bassSource) _bassSource.volume = Mathf.Lerp(0.06f, 0.2f, _intensity) * masterVolume * _sidechain;
            if (_padSource) _padSource.volume = Mathf.Lerp(0.12f, 0.06f, _intensity) * masterVolume * _sidechain;
            yield return null;
        }
    }

    IEnumerator Loop()
    {
        double nextBar = AudioSettings.dspTime + 0.1;

        while (_playing)
        {
            int bar = _currentBar % 24;
            int sec = bar / 4;
            float s16 = 60f / bpm / 4f;
            float barDur = s16 * 16f;

            float[] ft = { 0.15f, 0.35f, 0.65f, 0.85f, 0.2f, 0.3f };
            _filterOpenness = Mathf.Lerp(_filterOpenness, ft[sec], 0.12f);

            double t = nextBar;

            // Pad chord
            if (sec != 4 || bar % 4 == 0)
                SchedulePad(Roots[bar], barDur, t);

            // Sub
            Schedule(_subSource, N[Roots[bar]] * 0.25f, s16 * 10f, Wave.Sine, 0.5f, 0.5f, t);

            for (int s = 0; s < 16; s++)
            {
                double st = t + s * s16;
                int dc = Dr[bar][s];
                if (dc == 1 || dc == 4 || dc == 7) _sidechain = 0.2f;
                ScheduleDrum(dc, st);

                float vel = 0.88f + Random.Range(0f, 0.12f);

                // Bass (8ths)
                if (s % 2 == 0 && bar < Bass.Length)
                {
                    int bn = Bass[bar][s / 2];
                    if (bn >= 0)
                        Schedule(_bassSource, N[bn] * 0.5f, s16 * 2.5f, Wave.FiltSaw, 0.28f * vel, 0.5f, st);
                }

                // Vibraphone arp (16ths, sections B,C,C2)
                if (sec >= 1 && sec <= 3)
                {
                    int vn = Vib[bar % 4][s];
                    if (vn >= 0 && vn < N.Length)
                    {
                        float pan = 0.3f + (s % 4) * 0.13f;
                        Schedule(_vibSource, N[vn] * 2f, s16 * 1.2f, Wave.Vibraphone, 0.15f * vel, pan, st);
                    }
                }

                // Piano C
                if (sec == 2 && s % 2 == 0)
                {
                    int pn = PianoC[bar % 4][s / 2];
                    if (pn >= 0 && pn < N.Length)
                    {
                        float freq = N[pn];
                        if (_lastPianoFreq > 0) freq = Mathf.Lerp(_lastPianoFreq, freq, 0.8f);
                        _lastPianoFreq = N[pn];
                        Schedule(_pianoSource, freq, s16 * 3f, Wave.Piano, 0.35f * vel, 0.5f, st);
                    }
                }

                // Piano C2
                if (sec == 3 && s % 2 == 0)
                {
                    int pn = PianoC2[bar % 4][s / 2];
                    if (pn >= 0 && pn < N.Length)
                    {
                        float freq = N[pn];
                        if (_lastPianoFreq > 0) freq = Mathf.Lerp(_lastPianoFreq, freq, 0.8f);
                        _lastPianoFreq = N[pn];
                        Schedule(_pianoSource, freq, s16 * 2.8f, Wave.Piano, 0.32f * vel, 0.5f, st);
                    }
                }

                // A2: ghost piano echo
                if (sec == 5 && bar % 4 < 2 && s % 2 == 0)
                {
                    int pn = PianoC[bar % 4][s / 2];
                    if (pn >= 0 && pn < N.Length)
                        Schedule(_pianoSource, N[pn], s16 * 4f, Wave.Pad, 0.08f, 0.5f, st);
                }
            }

            nextBar += barDur;
            _currentBar++;
            while (AudioSettings.dspTime < nextBar - barDur * 0.5 && _playing)
                yield return null;
        }
    }

    void SchedulePad(int root, float dur, double dsp)
    {
        int t3 = Mathf.Min(root + 3, N.Length - 1);
        int t5 = Mathf.Min(root + 7, N.Length - 1);
        Schedule(_padSource, N[root], dur, Wave.Pad, 0.12f, 0.35f, dsp);
        Schedule(_padSource, N[t3], dur, Wave.Pad, 0.08f, 0.65f, dsp);
        Schedule(_padSource, N[t5], dur, Wave.Pad, 0.08f, 0.5f, dsp);
    }

    void Schedule(AudioSource parent, float freq, float dur, Wave w, float amp, float pan, double dsp)
    {
        var clip = Render(freq, dur, w, amp, pan);
        var src = Pool(parent);
        src.clip = clip;
        src.PlayScheduled(dsp);
    }

    void ScheduleDrum(int code, double dsp)
    {
        if (code == 0) return;
        if (code == 1 || code == 4 || code == 7) DrumAt(_kickClip, dsp);
        if (code == 2 || code == 5) DrumAt(_brushClip, dsp);
        if (code == 3 || code == 4 || code == 5) DrumAt(_rideClip, dsp);
        if (code == 6 || code == 7) DrumAt(_rideBellClip, dsp);
    }

    void DrumAt(AudioClip clip, double dsp) { var s = Pool(_drumSource); s.clip = clip; s.PlayScheduled(dsp); }

    // ─── SYNTH ───
    enum Wave { Sine, FiltSaw, Piano, Vibraphone, Pad }

    static readonly float[] Uni3 = { 0.997f, 1f, 1.003f };
    static readonly float[] Pan3 = { 0.35f, 0.5f, 0.65f };
    static readonly float[] Uni5 = { 0.993f, 0.997f, 1f, 1.003f, 1.007f };
    static readonly float[] Pan5 = { 0.2f, 0.35f, 0.5f, 0.65f, 0.8f };

    AudioClip Render(float freq, float dur, Wave w, float amp, float pan)
    {
        int len = Mathf.Max((int)(44100 * dur), 200);
        var bL = new float[len];
        var bR = new float[len];
        float filt = _filterOpenness;

        int voices; float[] det, pans; float vAmp;
        switch (w)
        {
            case Wave.Pad: voices = 5; det = Uni5; pans = Pan5; vAmp = amp / 2.5f; break;
            case Wave.Piano: voices = 3; det = Uni3; pans = Pan3; vAmp = amp / 1.6f; break;
            case Wave.FiltSaw: voices = 3; det = Uni3; pans = Pan3; vAmp = amp / 1.6f; break;
            default: voices = 1; det = new[]{1f}; pans = new[]{pan}; vAmp = amp; break;
        }

        var ph = new float[voices];
        var dr = new float[voices];
        var da = new float[voices];
        for (int v = 0; v < voices; v++) { ph[v] = Random.value; dr[v] = Random.Range(1.5f, 4f); da[v] = Random.Range(0.0003f, 0.0015f); }

        for (int i = 0; i < len; i++)
        {
            float t = i / 44100f;
            float atk, rel, env;

            switch (w)
            {
                case Wave.Piano:
                    atk = Mathf.Clamp01(t * 300f); // sharp piano attack
                    rel = Mathf.Exp(-t * 2.5f); // long piano decay
                    env = atk * rel;
                    break;
                case Wave.Vibraphone:
                    atk = Mathf.Clamp01(t * 500f);
                    rel = Mathf.Exp(-t * 1.8f);
                    env = atk * rel * (1f + Mathf.Sin(t * 6f * Mathf.PI) * 0.15f); // tremolo
                    break;
                case Wave.Pad:
                    env = (1f - Mathf.Exp(-t * 1.2f)) * Mathf.Clamp01((dur - t) * 8f);
                    break;
                default:
                    atk = 1f - Mathf.Exp(-t * 20f);
                    rel = Mathf.Clamp01((dur - t) * 10f);
                    env = atk * rel;
                    break;
            }

            float sL = 0f, sR = 0f;
            for (int v = 0; v < voices; v++)
            {
                float drift = 1f + Mathf.Sin(t * dr[v] * Mathf.PI) * da[v];
                float vf = freq * det[v] * drift;
                float p = (vf * t + ph[v]) % 1f;
                float val = 0f;

                switch (w)
                {
                    case Wave.Sine:
                        val = Mathf.Sin(2f * Mathf.PI * p);
                        break;
                    case Wave.FiltSaw:
                        val = (2f * p - 1f) * (0.3f + filt * 0.15f)
                            + Mathf.Sin(2f * Mathf.PI * p) * (0.5f - filt * 0.15f)
                            + Mathf.Sin(4f * Mathf.PI * p) * filt * 0.1f;
                        val += (Random.value - 0.5f) * 0.008f;
                        break;
                    case Wave.Piano:
                        // Piano: fundamental + inharmonic partials + hammer noise
                        val = Mathf.Sin(2f * Mathf.PI * vf * t) * 0.4f
                            + Mathf.Sin(2f * Mathf.PI * vf * 2.01f * t) * 0.2f * Mathf.Exp(-t * 4f)
                            + Mathf.Sin(2f * Mathf.PI * vf * 3.02f * t) * 0.1f * Mathf.Exp(-t * 6f)
                            + Mathf.Sin(2f * Mathf.PI * vf * 4.05f * t) * 0.05f * Mathf.Exp(-t * 8f);
                        // Hammer click
                        if (t < 0.005f) val += (Random.value - 0.5f) * 0.3f * (1f - t / 0.005f);
                        val += (Random.value - 0.5f) * 0.003f;
                        break;
                    case Wave.Vibraphone:
                        // Vibraphone: bell-like, clean harmonics
                        val = Mathf.Sin(2f * Mathf.PI * vf * t) * 0.4f
                            + Mathf.Sin(2f * Mathf.PI * vf * 3f * t) * 0.15f * Mathf.Exp(-t * 3f)
                            + Mathf.Sin(2f * Mathf.PI * vf * 5f * t) * 0.06f * Mathf.Exp(-t * 5f);
                        break;
                    case Wave.Pad:
                        float d2 = 1.005f;
                        val = Mathf.Sin(2f * Mathf.PI * vf * t) * 0.25f
                            + Mathf.Sin(2f * Mathf.PI * vf * d2 * t) * 0.2f
                            + Mathf.Sin(2f * Mathf.PI * vf / d2 * t) * 0.2f
                            + Mathf.Sin(4f * Mathf.PI * vf * t) * 0.06f;
                        val += (Random.value - 0.5f) * 0.003f;
                        break;
                }

                float vp = voices > 1 ? pans[v] : pan;
                sL += val * Mathf.Sqrt(1f - vp);
                sR += val * Mathf.Sqrt(vp);
            }

            bL[i] = sL * env * vAmp * Mathf.Sqrt(1f - pan);
            bR[i] = sR * env * vAmp * Mathf.Sqrt(pan);
        }

        var stereo = new float[len * 2];
        for (int i = 0; i < len; i++) { stereo[i*2] = bL[i]; stereo[i*2+1] = bR[i]; }
        var clip = AudioClip.Create("n", len, 2, 44100, false);
        clip.SetData(stereo, 0);
        return clip;
    }

    // ─── DRUMS: noir/jazz ───
    delegate float SF(float t, float d);

    void GenDrums()
    {
        // Kick: soft, muffled jazz kick
        _kickClip = GC("k", 0.18f, (t, d) => {
            float f = 120f * Mathf.Exp(-t * 20f) + 35f;
            return Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Exp(-t * 12f) * 0.7f;
        });
        // Brush snare: shhhh texture
        _brushClip = GC("br", 0.2f, (t, d) => {
            float e = Mathf.Exp(-t * 8f);
            float brush = (Random.value * 2f - 1f) * 0.4f;
            float body = Mathf.Sin(2f * Mathf.PI * 160f * t) * 0.15f;
            return (brush + body) * e * 0.35f;
        });
        // Ride cymbal: sustained shimmer
        _rideClip = GC("rd", 0.12f, (t, d) => {
            float e = Mathf.Exp(-t * 15f);
            float hi = Mathf.Sin(2f * Mathf.PI * 4200f * t) * 0.12f;
            float hi2 = Mathf.Sin(2f * Mathf.PI * 6800f * t) * 0.06f;
            return ((Random.value * 2f - 1f) * 0.08f + hi + hi2) * e * 0.2f;
        });
        // Ride bell: brighter ping
        _rideBellClip = GC("rb", 0.25f, (t, d) => {
            float e = Mathf.Exp(-t * 5f);
            float bell = Mathf.Sin(2f * Mathf.PI * 3000f * t) * 0.2f;
            float bell2 = Mathf.Sin(2f * Mathf.PI * 4500f * t) * 0.1f;
            return (bell + bell2) * e * 0.25f;
        });
    }

    static AudioClip GC(string n, float dur, SF fn)
    {
        int l = (int)(44100 * dur); var b = new float[l];
        for (int i = 0; i < l; i++) b[i] = fn(i / 44100f, dur);
        var c = AudioClip.Create(n, l, 1, 44100, false); c.SetData(b, 0); return c;
    }

    // ─── POOL ───
    readonly System.Collections.Generic.List<AudioSource> _pool = new();
    int _pi;
    AudioSource Pool(AudioSource parent)
    {
        const int sz = 64;
        while (_pool.Count < sz)
        {
            var go = new GameObject("_t"); go.transform.SetParent(transform);
            var s = go.AddComponent<AudioSource>(); s.playOnAwake = false; s.spatialBlend = 0f;
            _pool.Add(s);
        }
        var src = _pool[_pi % sz];
        src.volume = parent.volume;
        _pi++;
        return src;
    }
}
