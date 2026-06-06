using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class ApplyHourglassToCapsule
{
    static ApplyHourglassToCapsule()
    {
        EditorApplication.update += RunOnce;
    }

    private static void RunOnce()
    {
        EditorApplication.update -= RunOnce;
        Apply();
    }

    [MenuItem("Tools/Environment/Apply Hourglass Prefab")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) return;

        string prefabPath = "Assets/Prefabs/BlueCapsule.prefab";
        GameObject capsulePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (capsulePrefab == null)
        {
            Debug.LogError("[ApplyHourglass] BlueCapsule.prefab bulunamadı!");
            return;
        }

        // Prefab üzerinde güvenli işlem yapmak için geçici olarak sahneye çıkarıyoruz
        GameObject capsuleInst = (GameObject)PrefabUtility.InstantiatePrefab(capsulePrefab);
        bool changed = false;

        // 1. Kapsülün orijinal dış görünüşünü (pembe/mavi rengi) kapat
        MeshRenderer mr = capsuleInst.GetComponent<MeshRenderer>();
        if (mr != null && mr.enabled)
        {
            mr.enabled = false;
            changed = true;
        }

        // Eğer önceden eklenmiş hatalı kum saatleri varsa temizle
        for (int i = capsuleInst.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = capsuleInst.transform.GetChild(i);
            if (child.name == "HourglassModel")
            {
                GameObject.DestroyImmediate(child.gameObject);
                changed = true;
            }
        }

        // 2. İndirilen Kum Saati modelini yükle
        string modelPath = "Assets/hourglass/source/exp.fbx";
        GameObject hourglassModelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (hourglassModelPrefab != null)
        {
            GameObject hourglassObj = (GameObject)PrefabUtility.InstantiatePrefab(hourglassModelPrefab);
            hourglassObj.name = "HourglassModel";
            hourglassObj.transform.SetParent(capsuleInst.transform);

            // Bounding box hesaplaması ile tam merkeze ve ideal boyuta getir
            // (Tıpkı ana karakterdeki AutoFit sistemi gibi)
            hourglassObj.transform.localPosition = Vector3.zero;
            hourglassObj.transform.localRotation = Quaternion.identity;
            hourglassObj.transform.localScale = Vector3.one;

            Renderer[] renderers = hourglassObj.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                
                float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                if (maxDim < 0.0001f) maxDim = 1f;
                float targetScale = 1.3f / maxDim; // Kapsül boyutuna uygun bir scale (1.3 birim)

                hourglassObj.transform.localScale = new Vector3(targetScale, targetScale, targetScale);
                
                float targetY = -(bounds.center.y * targetScale);
                float targetX = -(bounds.center.x * targetScale);
                float targetZ = -(bounds.center.z * targetScale);
                hourglassObj.transform.localPosition = new Vector3(targetX, targetY, targetZ);
            }
            else
            {
                hourglassObj.transform.localPosition = Vector3.zero;
                hourglassObj.transform.localScale = Vector3.one * 0.5f;
            }

            // Sürekli dönmesini sağlayacak scripti ekle
            RotateObject rotator = hourglassObj.AddComponent<RotateObject>();
            rotator.rotationSpeed = new Vector3(0f, 150f, 0f);

            changed = true;
        }
        else
        {
            Debug.LogError("[ApplyHourglass] Kum saati modeli bulunamadı: " + modelPath);
        }

        if (changed)
        {
            // Değişiklikleri asıl Prefab'a (dosyaya) kaydet
            PrefabUtility.SaveAsPrefabAsset(capsuleInst, prefabPath);
            Debug.Log("[ApplyHourglass] Kum saati başarıyla kapsüle entegre edildi ve döndürme efekti verildi!");
        }

        // Geçici objeyi sil
        GameObject.DestroyImmediate(capsuleInst);
    }
}
