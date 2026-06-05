using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public void TriggerShake(float duration, float magnitude)
    {
        // Oyun durdugunda (Time.timeScale = 0) da sarsilabilmesi icin Coroutine baslatiyoruz.
        StartCoroutine(Shake(duration, magnitude));
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Zaman olceginden bagimsiz (unscaledDeltaTime) olarak ilerleme
            elapsed += Time.unscaledDeltaTime;

            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
