using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class RemoveGhostAndRestoreCapsule
{
    static RemoveGhostAndRestoreCapsule()
    {
        EditorApplication.delayCall += RestoreCapsule;
        EditorApplication.playModeStateChanged += state => 
        {
            if (state == PlayModeStateChange.EnteredEditMode) RestoreCapsule();
        };
    }

    private static void RestoreCapsule()
    {
        if (EditorApplication.isPlaying) return;
        if (SessionState.GetBool("GhostRestoredToCapsule", false)) return;

        PlayerMovement player = GameObject.FindAnyObjectByType<PlayerMovement>(FindObjectsInactive.Include);
        if (player == null) return;

        bool changed = false;

        // 1. Hayaleti sil
        Transform ghost = player.transform.Find("GhostModel");
        if (ghost != null)
        {
            GameObject.DestroyImmediate(ghost.gameObject);
            changed = true;
        }

        // 2. Kapsül görünümünü geri aç
        MeshRenderer playerRenderer = player.GetComponent<MeshRenderer>();
        if (playerRenderer != null && !playerRenderer.enabled)
        {
            playerRenderer.enabled = true;
            changed = true;
        }

        if (changed)
        {
            EditorUtility.SetDirty(player);
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[CapsuleRestore] Hayalet silindi, orijinal kapsül geri getirildi.");
        }

        SessionState.SetBool("GhostRestoredToCapsule", true);
    }
}
