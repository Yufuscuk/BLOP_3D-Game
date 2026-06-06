using UnityEngine;
using UnityEditor;

public class RevertObstacle
{
    [MenuItem("Tools/Obstacles/Revert to Red Cube")]
    public static void RevertNow()
    {
        // 1. Basit bir Kırmızı URP materyali oluştur
        string matPath = "Assets/RedCubeMat.mat";
        Material redMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (redMat == null)
        {
            redMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            redMat.SetColor("_BaseColor", Color.red);
            AssetDatabase.CreateAsset(redMat, matPath);
        }

        // 2. Obstacle.prefab'i bul
        string prefabPath = "Assets/Obstacle.prefab";
        GameObject obstaclePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (obstaclePrefab != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(obstaclePrefab);
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
            {
                GameObject prefabRoot = editingScope.prefabContentsRoot;

                // Geçiçi bir küp oluşturup Mesh'ini alalım
                GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Mesh cubeMesh = tempCube.GetComponent<MeshFilter>().sharedMesh;
                GameObject.DestroyImmediate(tempCube);

                // Mesh ve Material degisimi (Eski Küp stiline dönüş)
                MeshFilter mf = prefabRoot.GetComponent<MeshFilter>();
                if (mf != null) mf.sharedMesh = cubeMesh;

                MeshRenderer mr = prefabRoot.GetComponent<MeshRenderer>();
                if (mr != null) mr.sharedMaterial = redMat;

                // Boyutu eski orijinal haline getir (1, 1, 1)
                prefabRoot.transform.localScale = Vector3.one;

                // Hatalı/Yeniden eklenen colliderleri ve scriptleri sil
                SphereCollider sphereColl = prefabRoot.GetComponent<SphereCollider>();
                if (sphereColl != null) GameObject.DestroyImmediate(sphereColl);

                RotateMine rotScript = prefabRoot.GetComponent<RotateMine>();
                if (rotScript != null) GameObject.DestroyImmediate(rotScript);

                // Kutu Collider'i geri ekle (Küp için)
                BoxCollider boxColl = prefabRoot.GetComponent<BoxCollider>();
                if (boxColl == null) 
                {
                    boxColl = prefabRoot.AddComponent<BoxCollider>();
                }
                boxColl.isTrigger = true;

                Debug.Log("[RevertObstacle] Engel orijinal Küp haline donduruldu, sadece rengi kirmizi yapildi!");
            }
        }
        else
        {
            Debug.LogError("[RevertObstacle] Obstacle.prefab bulunamadi.");
        }
    }
}
