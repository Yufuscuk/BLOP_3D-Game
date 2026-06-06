using UnityEngine;

public class ObstacleMove : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float baseSpeed = 5.5f;

    private float startY;

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        // Global oyun hızına (gameSpeed) göre anlık hareket hızı hesaplanır
        float currentSpeed = baseSpeed;
        if (PlayerMovement.Instance != null)
        {
            currentSpeed *= PlayerMovement.Instance.gameSpeed;
        }

        // Hareket (Dünya koordinatlarında sola doğru)
        transform.Translate(Vector3.left * currentSpeed * Time.deltaTime, Space.World);

        // Şeritte ve Z=0 düzleminde kalmasını garanti et (Kaymaları önler)
        transform.position = new Vector3(transform.position.x, startY, 0f);

        // Ekrandan çıkınca sil
        if (transform.position.x < -10f)
        {
            Destroy(gameObject);
        }
    }
}