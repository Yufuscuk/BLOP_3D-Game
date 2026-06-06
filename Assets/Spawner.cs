using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public GameObject starPrefab;
    public GameObject capsulePrefab;

    [Header("Temel Spawn Sureleri")]
    public float baseObstacleInterval = 1.8f;
    public float baseStarInterval = 3.5f;
    public float baseCapsuleInterval = 15f;

    private float obstacleTimer = 0f;
    private float starTimer = 1f; // Yıldızlar engellerden bağımsız başlasın diye ön yükleme
    private float capsuleTimer = 5f; // İlk kapsül biraz daha geç çıksın

    // Çakışmayı (üst üste binmeyi) önlemek için her şeridin son obje üretme zamanını tutuyoruz
    private float[] lastSpawnTimePerLane = new float[3] { -10f, -10f, -10f };
    private float minTimeBetweenSpawnsOnSameLane = 0.7f;

    void Update()
    {
        // Oyun durmuşsa spawn yapma (Örn: Game Over veya Pause)
        if (Time.timeScale == 0f)
            return;

        // Global oyun hızına (gameSpeed) ulaşalım
        float currentSpeed = 1f;
        if (PlayerMovement.Instance != null)
        {
            currentSpeed = PlayerMovement.Instance.gameSpeed;
        }

        // 1. ENGEL SPAWN SİSTEMİ (Hızlandıkça engellerin gelme süresi sıklaşır)
        obstacleTimer += Time.deltaTime;
        float dynamicObstacleInterval = baseObstacleInterval / Mathf.Sqrt(currentSpeed);
        if (obstacleTimer >= dynamicObstacleInterval)
        {
            SpawnObstacle();
            obstacleTimer = 0f;
        }

        // 2. YILDIZ SPAWN SİSTEMİ
        starTimer += Time.deltaTime;
        float dynamicStarInterval = baseStarInterval / Mathf.Sqrt(currentSpeed);
        if (starTimer >= dynamicStarInterval)
        {
            SpawnStar();
            starTimer = 0f;
        }

        // 3. MAVİ KAPSÜL SPAWN SİSTEMİ
        capsuleTimer += Time.deltaTime;
        if (capsuleTimer >= baseCapsuleInterval)
        {
            SpawnCapsule();
            capsuleTimer = 0f;
        }
    }

    void SpawnObstacle()
    {
        float currentScore = PlayerMovement.Instance != null ? PlayerMovement.Instance.score : 0f;
        
        bool spawnDouble = false;
        if (currentScore >= 1000f)
        {
            // 1000 puandan sonra temel ihtimal %20. Her 1000 puanda %10 artar.
            float extraChance = ((currentScore - 1000f) / 1000f) * 0.10f;
            float doubleChance = 0.20f + extraChance;
            
            // Oyunun imkansızlaşmaması için ihtimal maksimum %65'te kalır
            doubleChance = Mathf.Min(doubleChance, 0.65f);

            if (Random.value < doubleChance)
            {
                spawnDouble = true;
            }
        }

        // Birinci engeli üret
        int lane1 = GetSafeLane();
        SpawnObstacleAtLane(lane1);

        // Şartlar sağlandıysa ikinci engeli üret (GetSafeLane dolu şeridi seçmeyecektir)
        if (spawnDouble)
        {
            int lane2 = GetSafeLane();
            SpawnObstacleAtLane(lane2);
        }
    }

    void SpawnObstacleAtLane(int lane)
    {
        float yPos = (lane - 1) * (PlayerMovement.Instance != null ? PlayerMovement.Instance.laneDistance : 2f);
        Vector3 spawnPos = new Vector3(9f, yPos, 0f);
        Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
    }

    void SpawnStar()
    {
        int lane = GetSafeLane();
        
        float yPos = (lane - 1) * (PlayerMovement.Instance != null ? PlayerMovement.Instance.laneDistance : 2f);
        Vector3 spawnPos = new Vector3(9f, yPos, 0f);
        Instantiate(starPrefab, spawnPos, Quaternion.identity);
    }

    void SpawnCapsule()
    {
        int lane = GetSafeLane();
        
        float yPos = (lane - 1) * (PlayerMovement.Instance != null ? PlayerMovement.Instance.laneDistance : 2f);
        Vector3 spawnPos = new Vector3(9f, yPos, 0f);
        Instantiate(capsulePrefab, spawnPos, Quaternion.identity);
    }

    // Üst üste binmeyi engellemek için güvenli (boş) bir şerit bulur
    int GetSafeLane()
    {
        int lane = Random.Range(0, 3);
        int attempts = 0;
        
        // Eğer seçilen şeritte çok yakın zamanda bir obje çıktıysa, farklı bir şerit dene
        while (Time.time - lastSpawnTimePerLane[lane] < minTimeBetweenSpawnsOnSameLane && attempts < 10)
        {
            lane = Random.Range(0, 3);
            attempts++;
        }
        
        // O şeride obje yerleştirildiğini kaydet
        lastSpawnTimePerLane[lane] = Time.time;
        return lane;
    }
}