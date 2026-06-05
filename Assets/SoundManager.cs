using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    private AudioClip starCollectClip;
    private AudioClip slowMotionClip;
    private AudioSource audioSource;

    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Eger sahnede yoksa dinamik olarak yeni bir SoundManager olusturuyoruz
                GameObject go = new GameObject("SoundManager");
                instance = go.AddComponent<SoundManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Ses calmak icin AudioSource bileseni ekliyoruz
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 0.4f; // Ses seviyesi

        // Matematiksel olarak retro 8-bit yildiz toplama sesi uretiyoruz
        starCollectClip = GenerateStarSound();

        // Zamani yavaslatma kapsulu sesini uretiyoruz
        slowMotionClip = GenerateSlowMotionSound();
    }

    public void PlayStarSound()
    {
        if (audioSource != null && starCollectClip != null)
        {
            audioSource.PlayOneShot(starCollectClip);
        }
    }

    public void PlaySlowMoSound()
    {
        if (audioSource != null && slowMotionClip != null)
        {
            audioSource.PlayOneShot(slowMotionClip);
        }
    }

    // Zamani yavaslatma kapsulu alindiginda calacak bas-dusumu (pitch drop) ses efekti
    private AudioClip GenerateSlowMotionSound()
    {
        int samplerate = 44100;
        float duration = 0.48f; // Ses suresi
        int sampleCount = Mathf.RoundToInt(samplerate * duration);
        float[] samples = new float[sampleCount];

        double phase = 0;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / samplerate;
            
            // Frekans 450Hz'den baslayip yavasca 120Hz'e duser (pitch drop yavaslama hissi)
            float freq = Mathf.Lerp(450f, 120f, t / duration);
            
            phase += 2.0 * Mathf.PI * freq / samplerate;
            
            // Tok bir bass tınısı: Temel sinus + 0.5 oraninda alt oktav (daha kalin ses)
            float wave = Mathf.Sin((float)phase) + 0.5f * Mathf.Sin((float)phase * 0.5f);
            wave /= 1.5f;

            float envelope = 1f;
            if (t < 0.01f) // Citirti engelleme
            {
                envelope = t / 0.01f;
            }
            else if (t > 0.32f) // Sonda yumusatarak kapatma
            {
                envelope = Mathf.Lerp(1f, 0f, (t - 0.32f) / (duration - 0.32f));
            }

            samples[i] = wave * 0.35f * envelope;
        }

        AudioClip clip = AudioClip.Create("SlowMotionSound", sampleCount, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // Sihirli ve zengin bir 8-bit "yildiz/para toplama" arpeji ureten gelismis synthesizer
    private AudioClip GenerateStarSound()
    {
        int samplerate = 44100;
        float duration = 0.38f; // Ses suresi (daha uzun arpej ve kuyruk tinisi icin)
        int sampleCount = Mathf.RoundToInt(samplerate * duration);
        float[] samples = new float[sampleCount];

        // Yukari dogru kayan major arpej notalari (C5, E5, G5, C6) - Klasik yildiz/para efekti
        float[] noteFrequencies = { 523.25f, 659.25f, 783.99f, 1046.50f };
        float noteDuration = 0.05f; // Her notanin baslangici arasi 50ms gecikme var

        double phase = 0;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / samplerate;

            // Notalar arasi gecis sirasi
            int noteIndex = Mathf.FloorToInt(t / noteDuration);
            noteIndex = Mathf.Clamp(noteIndex, 0, noteFrequencies.Length - 1);
            float baseFreq = noteFrequencies[noteIndex];

            // Sihirli bir pirilti (sparkle) vermesi icin hafif hizli vibrato ekliyoruz (12Hz)
            float vibrato = Mathf.Sin(2f * Mathf.PI * 12f * t) * 10f;
            float currentFreq = baseFreq + vibrato;

            // Citirti/kesinti (click) olmamasi icin fazi oransal olarak biriktiriyoruz
            phase += 2.0 * Mathf.PI * currentFreq / samplerate;

            // Zili andiran zengin tını: Temel dalga + 1. oktav armoni + 2. oktav armoni
            float wave = Mathf.Sin((float)phase) 
                       + 0.4f * Mathf.Sin((float)phase * 2f) 
                       + 0.15f * Mathf.Sin((float)phase * 3f);
            
            // Genligi dengeliyoruz
            wave /= 1.55f;

            // Ses zarfi (envelope): Baslangicta yumusat, sonda yavasca kapat
            float envelope = 1f;
            if (t < 0.005f) // Ilk giriste citirti engelleme
            {
                envelope = t / 0.005f;
            }
            else if (t > 0.20f) // 200. milisaniyeden sonra yumusak sonumlenme
            {
                envelope = Mathf.Lerp(1f, 0f, (t - 0.20f) / (duration - 0.20f));
            }

            // Sesi olustur
            samples[i] = wave * 0.22f * envelope;
        }

        AudioClip clip = AudioClip.Create("StarCollectSound", sampleCount, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
