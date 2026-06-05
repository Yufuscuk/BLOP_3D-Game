using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class FixObstacleTrigger
{
    static FixObstacleTrigger()
    {
        EditorApplication.delayCall += FixTrigger;
        EditorApplication.playModeStateChanged += state => 
        {
            if (state == PlayModeStateChange.EnteredEditMode) FixTrigger();
        };
    }

    private static void FixTrigger()
    {
        if (EditorApplication.isPlaying) return;
        if (SessionState.GetBool("ObstacleTriggerFixed", false)) return;

        string prefabPath = "Assets/Obstacle.prefab";
        GameObject obstaclePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (obstaclePrefab != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(obstaclePrefab);
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
            {
                GameObject prefabRoot = editingScope.prefabContentsRoot;

                BoxCollider boxColl = prefabRoot.GetComponent<BoxCollider>();
                if (boxColl != null && !boxColl.isTrigger)
                {
                    boxColl.isTrigger = true;
                    Debug.Log("[FixTrigger] Engel BoxCollider isTrigger = true olarak duzeltildi!");
                }
            }
        }

        SessionState.SetBool("ObstacleTriggerFixed", true);
    }
}
