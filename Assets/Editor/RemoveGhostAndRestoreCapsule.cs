using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class RemoveGhostAndRestoreCapsule
{
    static RemoveGhostAndRestoreCapsule()
    {
        EditorApplication.update += RunOnce;
    }

    private static void RunOnce()
    {
        EditorApplication.update -= RunOnce;
        ApplySlime();
    }

    [MenuItem("Tools/Player/Force Apply Cute Slime")]
    public static void ApplySlime()
    {
        if (EditorApplication.isPlaying) return;

        GameObject player = GameObject.Find("Player");
        if (player == null) 
        {
            Debug.LogWarning("[FixCuteSlime] Player nesnesi sahnede bulunamadı!");
            return;
        }

        bool changed = false;

        // 1. Kapsül MeshRenderer'ı kapat
        MeshRenderer mr = player.GetComponent<MeshRenderer>();
        if (mr != null && mr.enabled)
        {
            mr.enabled = false;
            changed = true;
        }

        // 2. Eski hatalı modelleri temizle
        for (int i = player.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = player.transform.GetChild(i);
            if (child.name != "SlimeModel")
            {
                GameObject.DestroyImmediate(child.gameObject);
                changed = true;
            }
        }

        // 3. Cute Slime modelini ekle ve otomatik boyutlandır
        Transform slimeTransform = player.transform.Find("SlimeModel");
        if (slimeTransform == null)
        {
            // Yeni dönüştürülmüş OBJ dosyasının yolu
            string modelPathObj = "Assets/ImageToStl.com_cute-slime/cute-slime.obj";
            
            GameObject slimePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPathObj);
            if (slimePrefab != null)
            {
                GameObject slimeObj = (GameObject)PrefabUtility.InstantiatePrefab(slimePrefab);
                slimeObj.name = "SlimeModel";
                
                AutoFitModel(slimeObj, player.transform);
                
                changed = true;
                Debug.Log("[FixCuteSlime] Cute Slime modeli eklendi ve boyutu/konumu otomatik ayarlandı!");
            }
            else
            {
                Debug.LogError("[FixCuteSlime] HATA: " + modelPathObj + " bulunamadı!");
                if (mr != null) 
                {
                    mr.enabled = true; // Karakterin görünmez olmaması için kapsülü geri aç
                    changed = true;
                }
            }
        }
        else
        {
            // Varsa bile konumunu/boyutunu tekrar düzelt (Kullanıcı veya eski kod bozmuş olabilir)
            AutoFitModel(slimeTransform.gameObject, player.transform);
            changed = true;
        }

        if (changed)
        {
            EditorUtility.SetDirty(player);
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }

    private static void AutoFitModel(GameObject modelObj, Transform playerTransform)
    {
        // Dünya koordinatlarında doğru bounds hesaplamak için geçici olarak boşa çıkar
        modelObj.transform.SetParent(null);
        modelObj.transform.position = Vector3.zero;
        modelObj.transform.rotation = Quaternion.identity;
        modelObj.transform.localScale = Vector3.one;

        Renderer[] renderers = modelObj.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (maxDim < 0.0001f) maxDim = 1f;

            // Modelin boyutunu 1.5 birime eşitle
            float targetScale = 1.5f / maxDim;

            // Tekrar Player'ın çocuğu yap
            modelObj.transform.SetParent(playerTransform);
            
            // Ölçeği ayarla
            modelObj.transform.localScale = new Vector3(targetScale, targetScale, targetScale);

            // Orijinal bounds'un merkezini ve altını bulup ofsetle
            float bottomY = bounds.min.y * targetScale;
            float targetY = -1f - bottomY; // Kapsülün tabanı Y = -1
            
            float targetX = -(bounds.center.x * targetScale);
            float targetZ = -(bounds.center.z * targetScale);

            modelObj.transform.localPosition = new Vector3(targetX, targetY, targetZ);
            
            // Yüzünü sağa dönmesi için
            modelObj.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        }
        else
        {
            modelObj.transform.SetParent(playerTransform);
            modelObj.transform.localPosition = new Vector3(0f, -1f, 0f);
            modelObj.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            modelObj.transform.localScale = Vector3.one * 1.5f;
        }
    }
}
