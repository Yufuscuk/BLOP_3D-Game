using UnityEngine;

public class ObstacleMove : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float baseSpeed = 5.5f;

    void Update()
    {
        // Global oyun hızına (gameSpeed) göre anlık hareket hızı hesaplanır
        float currentSpeed = baseSpeed;
        if (PlayerMovement.Instance != null)
        {
            currentSpeed *= PlayerMovement.Instance.gameSpeed;
        }

        // Hareket
        transform.Translate(Vector3.left * currentSpeed * Time.deltaTime);

        // Ekrandan çıkınca sil
        if (transform.position.x < -10f)
        {
            Destroy(gameObject);
        }
    }
}