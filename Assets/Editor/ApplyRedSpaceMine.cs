using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class ApplyRedSpaceMine
{
    static ApplyRedSpaceMine()
    {
        EditorApplication.delayCall += SetupSpaceMine;
        EditorApplication.playModeStateChanged += state => 
        {
            if (state == PlayModeStateChange.EnteredEditMode) SetupSpaceMine();
        };
    }

    private static void SetupSpaceMine()
    {
        if (EditorApplication.isPlaying) return;
        if (SessionState.GetBool("RedSpaceMineSetup_2", false)) return;

        // 1. Kırmızı, parlayan bir URP materyali olustur
        string matPath = "Assets/RedSpaceMineMat.mat";
        Material redMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (redMat == null)
        {
            redMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            
            // Koyu gri metalik taban rengi
            redMat.SetColor("_BaseColor", new Color(0.2f, 0.2f, 0.2f));
            redMat.SetFloat("_Metallic", 0.9f);
            redMat.SetFloat("_Smoothness", 0.6f);

            // GÜÇLÜ KIRMIZI EMISSION
            redMat.EnableKeyword("_EMISSION");
            redMat.SetColor("_EmissionColor", Color.red * 3.5f); // Glow efekti icin carpani yuksek tut
            
            AssetDatabase.CreateAsset(redMat, matPath);
        }

        // 2. Uzay Mayini Mesh'ini yukle
        string meshPath = "Assets/ImageToStl.com_futuristic_space_mine/futuristic_space_mine.obj";
        Mesh mineMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if (mineMesh == null)
        {
            Debug.LogError("[SpaceMine] Mesh bulunamadi! Yolu kontrol edin: " + meshPath);
            return;
        }

        // 3. Obstacle.prefab'i bul ve degistir
        string prefabPath = "Assets/Obstacle.prefab";
        GameObject obstaclePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (obstaclePrefab != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(obstaclePrefab);
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
            {
                GameObject prefabRoot = editingScope.prefabContentsRoot;

                // Mesh ve Material degisimi
                MeshFilter mf = prefabRoot.GetComponent<MeshFilter>();
                if (mf != null) mf.sharedMesh = mineMesh;

                MeshRenderer mr = prefabRoot.GetComponent<MeshRenderer>();
                if (mr != null) mr.sharedMaterial = redMat;

                // Boyutunu ayarla (Modelin orijinal buyuklugune gore kucultebiliriz, x0.5 yapalim)
                prefabRoot.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                // BoxCollider'i silip SphereCollider ekleyelim (Mayina daha uygun)
                BoxCollider boxColl = prefabRoot.GetComponent<BoxCollider>();
                if (boxColl != null) GameObject.DestroyImmediate(boxColl);

                SphereCollider sphereColl = prefabRoot.GetComponent<SphereCollider>();
                if (sphereColl == null) 
                {
                    sphereColl = prefabRoot.AddComponent<SphereCollider>();
                    // Mesh boyutlarina gore yaricapi otomatik ayarlar
                }

                // Donme scripti ekle (Yavasca donsun gercekci olsun)
                RotateMine rotScript = prefabRoot.GetComponent<RotateMine>();
                if (rotScript == null)
                {
                    prefabRoot.AddComponent<RotateMine>();
                }

                Debug.Log("[SpaceMine] Kirmizi uzay mayini prefab'a basariyla uygulandi!");
            }
        }
        else
        {
            Debug.LogError("[SpaceMine] Obstacle.prefab bulunamadi.");
        }

        SessionState.SetBool("RedSpaceMineSetup_2", true);
    }
}
