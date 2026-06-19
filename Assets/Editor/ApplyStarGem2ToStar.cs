using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class ApplyStarGem2ToStar
{
    static ApplyStarGem2ToStar()
    {
        EditorApplication.delayCall += Apply;
    }

    [MenuItem("Tools/Environment/Apply StarGem2 to Star Prefab")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) return;

        string starPrefabPath = "Assets/Star.prefab";
        GameObject starPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(starPrefabPath);
        if (starPrefab == null)
        {
            Debug.LogError("[ApplyStarGem2] Assets/Star.prefab bulunamadı!");
            return;
        }

        string gemPrefabPath = "Assets/BTM_Assets/BTM_Items_Gems/Prefabs/StarGem2.prefab";
        GameObject gemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(gemPrefabPath);
        if (gemPrefab == null)
        {
            Debug.LogError("[ApplyStarGem2] StarGem2.prefab bulunamadı: " + gemPrefabPath);
            return;
        }

        // Prefab üzerinde işlem yapmak için geçici olarak sahneye çıkarıyoruz
        GameObject starInst = (GameObject)PrefabUtility.InstantiatePrefab(starPrefab);
        
        // KRİTİK DÜZELTME: Dünya koordinatlarındaki kaymayı önlemek için geçici olarak orjine çekiyoruz
        Vector3 originalPosition = starInst.transform.position;
        starInst.transform.position = Vector3.zero;
        starInst.transform.rotation = Quaternion.identity;
        starInst.transform.localScale = Vector3.one;

        bool changed = false;

        // 1. Orijinal Kürenin (Sphere) MeshRenderer component'ini kapat
        MeshRenderer mr = starInst.GetComponent<MeshRenderer>();
        if (mr != null && mr.enabled)
        {
            mr.enabled = false;
            changed = true;
        }

        // Eski eklenmiş modelleri temizle
        for (int i = starInst.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = starInst.transform.GetChild(i);
            if (child.name == "StarGemModel")
            {
                GameObject.DestroyImmediate(child.gameObject);
                changed = true;
            }
        }

        // 2. StarGem2 modelini ekle
        GameObject gemObj = (GameObject)PrefabUtility.InstantiatePrefab(gemPrefab);
        gemObj.name = "StarGemModel";
        gemObj.transform.SetParent(starInst.transform);

        // Konum ve rotasyonu sıfırla
        gemObj.transform.localPosition = Vector3.zero;
        gemObj.transform.localRotation = Quaternion.identity;
        gemObj.transform.localScale = Vector3.one;

        // BİLEŞEN SİLME İÇİN ÖNEMLİ: Prefab Instance'ı tamamen çöz (Unpack)
        if (PrefabUtility.IsPartOfPrefabInstance(gemObj))
        {
            PrefabUtility.UnpackPrefabInstance(gemObj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        // 3. Çocuk üzerindeki tüm özel animasyon/hareket/salınım scriptlerini kaldır
        MonoBehaviour[] childScripts = gemObj.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var script in childScripts)
        {
            if (script != null)
            {
                GameObject.DestroyImmediate(script);
                changed = true;
            }
        }
        
        // Bounding box hesaplaması ile tam merkeze ve ideal boyuta getir
        Renderer[] renderers = gemObj.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            
            float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (maxDim < 0.0001f) maxDim = 1f;
            
            // YILDIZI BÜYÜTME: Çapını 1.0 birimden 1.45 birime yükseltiyoruz (şeridi taşmayacak maksimum ideal boy)
            float targetScale = 1.45f / maxDim;

            gemObj.transform.localScale = new Vector3(targetScale, targetScale, targetScale);
            
            // Orijin kaymasını (pivot ofsetini) gidererek yıldızı collider'ın tam ortasına alıyoruz
            float targetX = -bounds.center.x * targetScale;
            float targetY = -bounds.center.y * targetScale;
            float targetZ = -bounds.center.z * targetScale;
            
            gemObj.transform.localPosition = new Vector3(targetX, targetY, targetZ);

            // 4. ALTIN PARILTI (GLOW) EFEKTİ
            foreach (var r in renderers)
            {
                Material mat = r.sharedMaterial;
                if (mat != null)
                {
                    mat.EnableKeyword("_EMISSION");
                    // Altın sarısı yumuşak bir parıltı rengi veriyoruz
                    mat.SetColor("_EmissionColor", new Color(1.0f, 0.78f, 0.15f) * 0.9f);
                    EditorUtility.SetDirty(mat);
                }
            }

            changed = true;
            Debug.Log($"[ApplyStarGem2] Yıldız büyütüldü ve parlatıldı. Ölçek: {targetScale}, Konum: {gemObj.transform.localPosition}");
        }
        else
        {
            gemObj.transform.localScale = Vector3.one * 0.7f;
            gemObj.transform.localPosition = Vector3.zero;
            changed = true;
        }

        if (changed)
        {
            // Pozisyonu eski haline getirmeden kaydedersek prefabın orijinal pozisyonu bozulabilir,
            // bu yüzden eski pozisyonuna alıp kaydediyoruz.
            starInst.transform.position = originalPosition;
            
            // Değişiklikleri asıl Prefab'a (dosyaya) kaydet
            PrefabUtility.SaveAsPrefabAsset(starInst, starPrefabPath);
            Debug.Log("[ApplyStarGem2] StarGem2 başarıyla Star prefabına entegre edildi, boyutu büyütüldü ve parlatıldı!");
        }

        // Geçici objeyi sil
        GameObject.DestroyImmediate(starInst);
    }
}
