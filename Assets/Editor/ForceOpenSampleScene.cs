using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class ForceOpenSampleScene
{
    static ForceOpenSampleScene()
    {
        EditorApplication.delayCall += OpenMainScene;
    }

    private static void OpenMainScene()
    {
        if (EditorApplication.isPlaying) return;
        if (SessionState.GetBool("SampleSceneForcedOpen2", false)) return;

        string mainScenePath = "Assets/Scenes/SampleScene.unity";

        // Kullanıcı zaten SampleScene'de değilse
        if (EditorSceneManager.GetActiveScene().path != mainScenePath)
        {
            // Eger oylesine acik bozuk bir sahne varsa kaydetmeden gec
            EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);
            Debug.Log("[SceneFix] Baska bir sahnede kalinmis, orijinal SampleScene basariyla geri yuklendi.");
        }

        SessionState.SetBool("SampleSceneForcedOpen2", true);
    }
}
