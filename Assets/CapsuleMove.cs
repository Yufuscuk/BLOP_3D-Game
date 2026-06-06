using UnityEngine;

public class CapsuleMove : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float speed = 5f;
    public float rotateSpeed = 120f;

    [Header("Süzülme Ayarları")]
    public float floatSpeed = 4f;
    public float floatHeight = 0.15f;

    private float startY;

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        // 1. Sola hareket (Global oyun hızına göre ölçekli)
        float currentSpeed = speed;
        if (PlayerMovement.Instance != null)
        {
            currentSpeed *= PlayerMovement.Instance.gameSpeed;
        }
        transform.Translate(Vector3.left * currentSpeed * Time.deltaTime, Space.World);

        // 2. Havali donus efekti
        transform.Rotate(new Vector3(0.5f, 1f, 0.2f).normalized * rotateSpeed * Time.deltaTime);

        // 3. Hafif dalgalanma efekti ve Z=0 kilidi
        float newY = startY + (Mathf.Sin(Time.time * floatSpeed) * floatHeight);
        transform.position = new Vector3(transform.position.x, newY, 0f);

        // 4. Ekrandan cikinca yok et
        if (transform.position.x < -10f)
        {
            Destroy(gameObject);
        }
    }
}
