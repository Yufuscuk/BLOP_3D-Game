using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class ZoomBackground
{
    static ZoomBackground()
    {
        EditorApplication.update += RunOnce;
    }

    private static void RunOnce()
    {
        EditorApplication.update -= RunOnce;
        ApplyZoom();
    }

    [MenuItem("Tools/Environment/Zoom Background")]
    public static void ApplyZoom()
    {
        if (EditorApplication.isPlaying) return;

        bool changed = false;
        Camera mainCam = Camera.main;
        
        if (mainCam != null)
        {
            float oldFov = mainCam.fieldOfView;
            // Eğer daha önce zoom yapılmadıysa (varsayılan genelde 60'tır)
            if (oldFov > 45f)
            {
                float newFov = 40f; // FOV'u düşürerek arka planı (Skybox) büyüt
                
                // Oyundaki karakterlerin ve engellerin ekrandaki boyutunun değişmemesi için
                // kamerayı FOV oranında geriye çekiyoruz (Dolly Zoom / Vertigo Efekti mantığı).
                float currentDistance = Mathf.Abs(mainCam.transform.position.z);
                if (currentDistance < 1f) currentDistance = 10f; // Güvenlik kontrolü
                
                float oldHeight = currentDistance * Mathf.Tan(oldFov * 0.5f * Mathf.Deg2Rad);
                float newDistance = oldHeight / Mathf.Tan(newFov * 0.5f * Mathf.Deg2Rad);
                
                // Yeni değerleri uygula
                mainCam.fieldOfView = newFov;
                Vector3 pos = mainCam.transform.position;
                pos.z = -newDistance;
                mainCam.transform.position = pos;
                
                EditorUtility.SetDirty(mainCam.gameObject);
                changed = true;
                
                Debug.Log($"[ZoomBackground] Arka plan yakınlaştırıldı! Eski FOV: {oldFov}, Yeni FOV: {newFov}. Kamera uzaklığı {currentDistance}'den {newDistance}'e çekilerek oyun alanı boyutu korundu.");
            }
            
            // Eğer Skybox scriptinin hızı ayarlanmamışsa onu da zorla güncelleyelim (Daha belirgin akış için)
            RotateSkybox rotator = mainCam.GetComponent<RotateSkybox>();
            if (rotator != null && rotator.rotationSpeed < 7.9f)
            {
                rotator.rotationSpeed = 8f;
                EditorUtility.SetDirty(rotator);
                changed = true;
            }
        }

        if (changed)
        {
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
