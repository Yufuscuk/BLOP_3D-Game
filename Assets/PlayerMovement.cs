using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public float laneDistance = 2f;
    private int currentLane = 1;

    public GameObject gameOverPanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public GameObject starEffect;

    private float score = 0f;
    private int loadedHighScore = 0;

    public float scoreMultiplier = 5f;
    public float gameSpeed = 1f;

    private bool isGameOver = false;
    private CameraShake cameraShake;
    private Coroutine slowMoCoroutine;

    [Header("Jelly (Squash & Stretch) Ayarlari")]
    public float springStiffness = 180f;
    public float springDamping = 12f;
    public float laneChangeStretchKick = 12f;

    private float scaleY = 1f;
    private float scaleYVelocity = 0f;

    void Start()
    {
        Time.timeScale = 1f; // Sahne yeniden yuklendiginde zamanin akmasini garanti altina alir

        // Kamera sarsinti bilesenini bul
        if (Camera.main != null)
        {
            cameraShake = Camera.main.GetComponent<CameraShake>();
        }

        // En yuksek skoru hafizadan yukle ve ekranda goster
        loadedHighScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
        {
            highScoreText.text = "Best: " + loadedHighScore;
        }
    }

    void Update()
    {
        // JELLY SPRING UPDATE
        // Fizik tabanli yay simulasyonu: kuvvet = -k * x - c * v
        float springForce = -springStiffness * (scaleY - 1f) - springDamping * scaleYVelocity;
        scaleYVelocity += springForce * Time.deltaTime;
        scaleY += scaleYVelocity * Time.deltaTime;

        // Ölçeğin aşırı bozulmaması için güvenli sınırlarda tutuyoruz
        scaleY = Mathf.Clamp(scaleY, 0.4f, 1.8f);

        // Hacmi korumak için Y uzadıkça X ve Z daralır, Y basıldıkça X ve Z genişler
        float scaleXZ = 1f - (scaleY - 1f) * 0.5f;
        transform.localScale = new Vector3(scaleXZ, scaleY, scaleXZ);

        // SCORE SYSTEM
        if (!isGameOver)
        {
            // Zamanla hızlanma hızını azalttık (0.015f) ve maksimum 2.5f limit koyduk (Dengeli zorlasma)
            gameSpeed += Time.deltaTime * 0.015f;
            gameSpeed = Mathf.Min(gameSpeed, 2.5f);

            score += Time.deltaTime * scoreMultiplier * gameSpeed;

            int currentScore = Mathf.FloorToInt(score);
            scoreText.text = "Score: " + currentScore;

            // Eger mevcut skor yuksek skoru gectiyse ekranda anlik guncelle
            if (currentScore > loadedHighScore)
            {
                loadedHighScore = currentScore;
                if (highScoreText != null)
                {
                    highScoreText.text = "Best: " + loadedHighScore;
                }
            }
        }

        // MOVEMENT
        if (!isGameOver)
        {
            int oldLane = currentLane;
            if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && currentLane < 2)
                currentLane++;

            if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && currentLane > 0)
                currentLane--;

            if (oldLane != currentLane)
            {
                // Şerit değiştiğinde yay sistemine dikey uzama ivmesi (kick) veriyoruz
                scaleYVelocity = laneChangeStretchKick;
            }

            float targetY = (currentLane - 1) * laneDistance;

            Vector3 targetPosition = new Vector3(-6, targetY, 0);

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
        }

        // RESTART
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // OBSTACLE
        if (other.CompareTag("Obstacle"))
        {
            isGameOver = true;

            Time.timeScale = 0f;

            // Guclu Kamera Sarsintisi tetikle (0.4 sn, 0.3 siddet)
            if (cameraShake != null)
            {
                cameraShake.TriggerShake(0.4f, 0.3f);
            }

            // Oyun bittiginde eger yeni rekor kirildiysa diskte guncelle
            int finalScore = Mathf.FloorToInt(score);
            int savedHighScore = PlayerPrefs.GetInt("HighScore", 0);
            if (finalScore > savedHighScore)
            {
                PlayerPrefs.SetInt("HighScore", finalScore);
                PlayerPrefs.Save();
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
        }

        // STAR
        if (other.CompareTag("Star"))
        {
            // Cok hafif ve yumusak Kamera Sarsintisi (0.08 sn, 0.02 siddet)
            if (cameraShake != null)
            {
                cameraShake.TriggerShake(0.08f, 0.02f);
            }

            // Dinamik olarak uretilen yildiz toplama sesini cal
            SoundManager.Instance.PlayStarSound();

            // Yıldızın taban puanı (Örn: 50) ile anlık oyun hızını (zorluğu) çarpıyoruz
            float kazanilanPuan = 40f * gameSpeed;

            // Hesaplanmış dinamik puanı skora ekle
            score += kazanilanPuan;

            int currentScore = Mathf.FloorToInt(score);
            scoreText.text = "Score: " + currentScore;

            // Yildiz toplayinca da yuksek skoru kontrol et
            if (currentScore > loadedHighScore)
            {
                loadedHighScore = currentScore;
                if (highScoreText != null)
                {
                    highScoreText.text = "Best: " + loadedHighScore;
                }
            }

            Instantiate(starEffect, other.transform.position, Quaternion.identity);

            Destroy(other.gameObject);
        }

        // CAPSULE (Zamani yavaslatma)
        SlowMotionCapsule capsule = other.GetComponent<SlowMotionCapsule>();
        if (capsule != null)
        {
            // Zamanı yavaşlat (2.5 saniyeliğine 0.6x hız - daha dengeli ve akıcı bir oyun hissi için)
            ActivateSlowMotion(2.5f, 0.6f);

            // Hafif sarsıntı tetikle
            if (cameraShake != null)
            {
                cameraShake.TriggerShake(0.12f, 0.04f);
            }

            // Yavaslama sesini çal
            SoundManager.Instance.PlaySlowMoSound();

            Destroy(other.gameObject);
        }
    }

    public void ActivateSlowMotion(float duration, float slowFactor)
    {
        if (slowMoCoroutine != null)
        {
            StopCoroutine(slowMoCoroutine);
        }
        slowMoCoroutine = StartCoroutine(SlowMoRoutine(duration, slowFactor));
    }

    private System.Collections.IEnumerator SlowMoRoutine(float duration, float slowFactor)
    {
        // Zamanı yavaşlat
        Time.timeScale = slowFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // Gerçek dünya zamanıyla bekle (zaman yavaşladığı için unscaled)
        yield return new WaitForSecondsRealtime(duration);

        // Oyun bitmediyse zamanı normale döndür
        if (!isGameOver)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }

        slowMoCoroutine = null;
    }
}