using UnityEngine;
using TMPro;

public class BlinkText : MonoBehaviour
{
    [Header("Blink Ayarlari")]
    public float blinkSpeed = 3f;

    private TextMeshProUGUI textMeshPro;

    void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (textMeshPro != null)
        {
            // Oyun durdugunda (Time.timeScale = 0) da calismasi icin Time.unscaledTime kullanilmalidir.
            float alpha = (Mathf.Sin(Time.unscaledTime * blinkSpeed) + 1f) / 2f;
            Color textCol = textMeshPro.color;
            textMeshPro.color = new Color(textCol.r, textCol.g, textCol.b, alpha);
        }
    }
}
