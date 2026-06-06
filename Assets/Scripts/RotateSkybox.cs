using UnityEngine;

public class RotateSkybox : MonoBehaviour
{
    public float rotationSpeed = 8.0f;
    private float currentRotation = 0f;

    void Start()
    {
        if (RenderSettings.skybox != null && RenderSettings.skybox.HasProperty("_Rotation"))
        {
            currentRotation = RenderSettings.skybox.GetFloat("_Rotation");
        }
    }

    void Update()
    {
        if (RenderSettings.skybox != null && RenderSettings.skybox.HasProperty("_Rotation"))
        {
            // Oyun hızını al (PlayerMovement Singleton'ı üzerinden)
            float speedMultiplier = 1f;
            if (PlayerMovement.Instance != null)
            {
                speedMultiplier = PlayerMovement.Instance.gameSpeed;
            }

            // Oyun hızlandıkça akış da orantılı olarak hızlanacak
            currentRotation += rotationSpeed * speedMultiplier * Time.deltaTime;
            currentRotation %= 360f;
            RenderSettings.skybox.SetFloat("_Rotation", currentRotation);
        }
    }
}
