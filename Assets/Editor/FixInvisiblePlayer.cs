using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class FixInvisiblePlayer
{
    static FixInvisiblePlayer()
    {
        EditorApplication.delayCall += FixPlayer;
        EditorApplication.playModeStateChanged += state => 
        {
            if (state == PlayModeStateChange.EnteredEditMode) FixPlayer();
        };
    }

    private static void FixPlayer()
    {
        if (EditorApplication.isPlaying) return;
        if (SessionState.GetBool("PlayerVisibleFixed", false)) return;

        PlayerMovement player = GameObject.FindAnyObjectByType<PlayerMovement>(FindObjectsInactive.Include);
        if (player == null) return;

        bool changed = false;

        MeshRenderer playerRenderer = player.GetComponent<MeshRenderer>();
        if (playerRenderer != null && !playerRenderer.enabled)
        {
            playerRenderer.enabled = true;
            changed = true;
            Debug.Log("[FixPlayer] Kapsul tekrar gorunur hale getirildi.");
        }

        if (changed)
        {
            EditorUtility.SetDirty(player);
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        SessionState.SetBool("PlayerVisibleFixed", true);
    }
}
