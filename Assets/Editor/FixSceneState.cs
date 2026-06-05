using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class FixSceneState
{
    static FixSceneState()
    {
        EditorApplication.delayCall += FixNow;
    }

    private static void FixNow()
    {
        if (EditorApplication.isPlaying) return;
        if (SessionState.GetBool("FixSceneStateRun_2", false)) return;

        bool changed = false;

        // 1. Fix Spawner missing obstacle prefab
        Spawner spawner = GameObject.FindAnyObjectByType<Spawner>(FindObjectsInactive.Include);
        if (spawner != null)
        {
            // obstaclePrefab might be missing
            GameObject obstacleAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Obstacle.prefab");
            if (obstacleAsset != null)
            {
                spawner.obstaclePrefab = obstacleAsset;
                EditorUtility.SetDirty(spawner);
                changed = true;
                Debug.Log("[Fix] Spawner obstaclePrefab restored to Assets/Obstacle.prefab");
            }
        }

        // 2. Fix Camera Background (restore to Skybox)
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            if (mainCam.clearFlags != CameraClearFlags.Skybox)
            {
                mainCam.clearFlags = CameraClearFlags.Skybox;
                EditorUtility.SetDirty(mainCam.gameObject);
                changed = true;
                Debug.Log("[Fix] Camera clearFlags restored to Skybox.");
            }
        }

        if (changed)
        {
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        SessionState.SetBool("FixSceneStateRun_2", true);
    }
}
