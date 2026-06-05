using UnityEngine;

public class Star : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float speed = 5f;
    public float rotateSpeed = 150f;

    [Header("Süzülme (Float) Ayarları")]
    public float floatSpeed = 3f;
    public float floatHeight = 0.2f;

    private float startY; // Sadece Y eksenini tutmamız yeterli

    void Start()
    {
        // Yıldızın doğduğu andaki Y pozisyonunu hafızaya alıyoruz
        startY = transform.position.y;
    }

    void Update()
    {
        // 1. SOLA HAREKET (Global oyun hızına göre ölçekli)
        float currentSpeed = speed;
        if (PlayerMovement.Instance != null)
        {
            currentSpeed *= PlayerMovement.Instance.gameSpeed;
        }
        transform.Translate(Vector3.left * currentSpeed * Time.deltaTime, Space.World);

        // 2. DÖNME EFEKTİ
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        // 3. YUKARI AŞAĞI SÜZÜLME (Dalgalanma Efekti)
        float newY = startY + (Mathf.Sin(Time.time * floatSpeed) * floatHeight);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // 4. EKRANDAN ÇIKINCA YOK ET
        if (transform.position.x < -10f)
        {
            Destroy(gameObject);
        }
    }
}