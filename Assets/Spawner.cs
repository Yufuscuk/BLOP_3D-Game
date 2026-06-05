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
        int lane = Random.Range(0, 3);
        float yPos = (lane - 1) * 2f; // lane 0 -> -2, 1 -> 0, 2 -> 2

        Vector3 spawnPos = new Vector3(9f, yPos, 0f);
        Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
    }

    void SpawnStar()
    {
        int lane = Random.Range(0, 3);
        float yPos = (lane - 1) * 2f;

        Vector3 spawnPos = new Vector3(9f, yPos, 0f);
        Instantiate(starPrefab, spawnPos, Quaternion.identity);
    }

    void SpawnCapsule()
    {
        int lane = Random.Range(0, 3);
        float yPos = (lane - 1) * 2f;

        Vector3 spawnPos = new Vector3(9f, yPos, 0f);
        Instantiate(capsulePrefab, spawnPos, Quaternion.identity);
    }
}