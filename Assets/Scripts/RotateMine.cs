using UnityEngine;

public class RotateMine : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(45f, 60f, 30f);

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime, Space.World);
    }
}
