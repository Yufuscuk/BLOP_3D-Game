using UnityEngine;
using UnityEditor;

public class FixStarPrefab
{
    [MenuItem("Tools/Environment/Fix and Center Star Model")]
    public static void FixStar()
    {
        if (EditorApplication.isPlaying) return;

        string starPrefabPath = "Assets/Star.prefab";
        GameObject starPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(starPrefabPath);
        if (starPrefab == null)
        {
            Debug.LogError("[FixStar] Assets/Star.prefab bulunamadı!");
            return;
        }

        // Prefab üzerinde güvenli işlem yapmak için geçici olarak sahneye çıkarıyoruz
        GameObject starInst = (GameObject)PrefabUtility.InstantiatePrefab(starPrefab);
        bool changed = false;

        // 1. Orijinal Kürenin (Sphere) MeshRenderer component'ini kapat
        MeshRenderer mr = starInst.GetComponent<MeshRenderer>();
        if (mr != null && mr.enabled)
        {
            mr.enabled = false;
            changed = true;
        }

        // 2. Çocukları bul ve hizala
        if (starInst.transform.childCount == 0)
        {
            Debug.LogError("[FixStar] Star.prefab altında çocuk model bulunamadı! Lütfen önce yıldız modelini Star prefab'ı altına sürükleyip bırakın.");
            GameObject.DestroyImmediate(starInst);
            return;
        }

        // Tüm çocukların pozisyonlarını sıfırla ve boyutlarını otomatik fit et
        for (int i = 0; i < starInst.transform.childCount; i++)
        {
            Transform child = starInst.transform.GetChild(i);
            
            // Düzenli hesaplama için önce transformu sıfırlayalım
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;

            // Bounding box hesaplaması ile tam merkeze ve ideal boyuta getir
            Renderer[] renderers = child.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                // İlk renderer'ın bounds değerini alıp diğerlerini birleştiriyoruz
                Bounds bounds = renderers[0].bounds;
                for (int j = 1; j < renderers.Length; j++)
                {
                    bounds.Encapsulate(renderers[j].bounds);
                }

                float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                if (maxDim < 0.0001f) maxDim = 1f;
                
                // Orijinal gri kürenin çapı 1.0 birimdi (yarıçap: 0.5). Yıldızın da çapını ~1.1 yapıyoruz.
                float targetScale = 1.1f / maxDim;

                child.localScale = new Vector3(targetScale, targetScale, targetScale);
                
                // Orijin kaymasını (pivot ofsetini) gidererek yıldızı collider'ın tam ortasına alıyoruz
                float targetX = -bounds.center.x * targetScale;
                float targetY = -bounds.center.y * targetScale;
                float targetZ = -bounds.center.z * targetScale;
                
                child.localPosition = new Vector3(targetX, targetY, targetZ);
                
                changed = true;
                Debug.Log($"[FixStar] '{child.name}' başarıyla merkezlendi ve ölçeklendi. Ölçek: {targetScale}, Konum Ofseti: {child.localPosition}");
            }
            else
            {
                child.localScale = Vector3.one;
                child.localPosition = Vector3.zero;
                changed = true;
            }
        }

        if (changed)
        {
            // Değişiklikleri asıl Prefab'a (dosyaya) kaydet
            PrefabUtility.SaveAsPrefabAsset(starInst, starPrefabPath);
            Debug.Log("[FixStar] Düzeltmeler Star.prefab dosyasına başarıyla kaydedildi!");
        }

        // Geçici objeyi sil
        GameObject.DestroyImmediate(starInst);
    }
}
