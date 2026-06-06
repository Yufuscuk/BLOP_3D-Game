using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class ApplySkyboxFlow
{
    static ApplySkyboxFlow()
    {
        EditorApplication.update += RunOnce;
    }

    private static void RunOnce()
    {
        EditorApplication.update -= RunOnce;
        Apply();
    }

    [MenuItem("Tools/Environment/Apply Epic Blue Sunset")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) return;

        bool changed = false;

        // 1. Skybox Materyalini Ayarla
        string skyboxPath = "Assets/AllSkyFree/Night MoonBurst/Night Moon Burst.mat";
        Material skyboxMat = AssetDatabase.LoadAssetAtPath<Material>(skyboxPath);
        if (skyboxMat != null)
        {
            // Eğer seçilen materyalde dönüş (Rotation) özelliği yoksa, bunu destekleyen standart shader'a çevir
            if (!skyboxMat.HasProperty("_Rotation"))
            {
                Shader sixSided = Shader.Find("Skybox/6 Sided");
                if (sixSided != null)
                {
                    skyboxMat.shader = sixSided;
                    EditorUtility.SetDirty(skyboxMat);
                    changed = true;
                    Debug.Log("[ApplySkyboxFlow] Skybox shader'ı dönüş desteklemesi için Skybox/6 Sided olarak güncellendi.");
                }
            }

            if (RenderSettings.skybox != skyboxMat)
            {
                RenderSettings.skybox = skyboxMat;
                changed = true;
                Debug.Log("[ApplySkyboxFlow] Skybox başarıyla Night Moon Burst olarak ayarlandı.");
            }
        }
        else
        {
            Debug.LogError("[ApplySkyboxFlow] Skybox materyali bulunamadı: " + skyboxPath);
            return;
        }

        // 2. Kameranın arkaplanını Skybox yap
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            if (mainCam.clearFlags != CameraClearFlags.Skybox)
            {
                mainCam.clearFlags = CameraClearFlags.Skybox;
                changed = true;
            }

            // 3. Akış animasyonu için RotateSkybox scriptini ekle
            RotateSkybox rotator = mainCam.GetComponent<RotateSkybox>();
            if (rotator == null)
            {
                mainCam.gameObject.AddComponent<RotateSkybox>();
                changed = true;
                Debug.Log("[ApplySkyboxFlow] RotateSkybox (Akış efekti) kameraya eklendi.");
            }
        }
        else
        {
            Debug.LogWarning("[ApplySkyboxFlow] Sahnede Main Camera bulunamadı, akış efekti eklenemedi.");
        }

        if (changed)
        {
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
