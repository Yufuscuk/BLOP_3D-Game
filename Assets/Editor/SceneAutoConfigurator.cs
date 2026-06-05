using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

[InitializeOnLoad]
public class SceneAutoConfigurator
{
    static SceneAutoConfigurator()
    {
        EditorApplication.delayCall += ConfigureScene;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            ConfigureScene();
        }
    }

    private static void ConfigureScene()
    {
        if (EditorApplication.isPlaying)
            return;

        AssetDatabase.Refresh();

        // Her degisiklikte versiyonu artir
        if (SessionState.GetBool("SceneConfiguredCleanFinal", false))
            return;

        Debug.Log("[AutoConfig] Temiz sahne yapilandirmasi baslatiliyor (Asset'siz Saf Sürüm)...");

        string scenePath = "Assets/Scenes/SampleScene.unity";
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != scenePath)
        {
            Debug.LogWarning("[AutoConfig] SampleScene aktif sahne degil, atlaniyor.");
            return;
        }

        PlayerMovement player = GameObject.FindAnyObjectByType<PlayerMovement>(FindObjectsInactive.Include);
        if (player == null)
        {
            Debug.LogError("[AutoConfig] PlayerMovement bileseni sahnede bulunamadi!");
            return;
        }

        bool changed = false;

        // Kamera ayarlarını değiştirmeyi bıraktık (eski haline dönmesi için)
        Camera mainCam = Camera.main;

        // 2. BlinkText bileseni ekle
        TextMeshProUGUI restartTextComponent = null;

        if (player.gameOverPanel != null)
        {
            TextMeshProUGUI[] panelTexts = player.gameOverPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var txt in panelTexts)
            {
                string txtLower = txt.text.ToLower();
                if (txtLower.Contains("game over") || txtLower.Contains("gameover"))
                    continue;

                if (txt.text.Contains("R") || txtLower.Contains("restart") || txtLower.Contains("press"))
                {
                    restartTextComponent = txt;
                    break;
                }
            }
        }

        if (restartTextComponent != null)
        {
            GameObject restartTextGO = restartTextComponent.gameObject;
            if (restartTextGO.GetComponent<BlinkText>() == null)
            {
                restartTextGO.AddComponent<BlinkText>();
                Debug.Log("[AutoConfig] BlinkText bileseni eklendi: " + restartTextGO.name);
                changed = true;
            }
        }

        // 3. HighScoreText olustur (yoksa)
        if (player.scoreText != null && player.highScoreText == null)
        {
            GameObject scoreGO = player.scoreText.gameObject;
            GameObject existingHigh = GameObject.Find("HighScoreText");
            if (existingHigh == null)
            {
                GameObject highGO = GameObject.Instantiate(scoreGO, scoreGO.transform.parent);
                highGO.name = "HighScoreText";

                RectTransform rect = highGO.GetComponent<RectTransform>();
                TextMeshProUGUI highTextComp = highGO.GetComponent<TextMeshProUGUI>();

                if (rect != null && highTextComp != null)
                {
                    rect.anchorMin = new Vector2(1, 1);
                    rect.anchorMax = new Vector2(1, 1);
                    rect.pivot = new Vector2(1, 1);
                    rect.anchoredPosition = new Vector2(-30f, -20f);
                    highTextComp.alignment = TextAlignmentOptions.TopRight;
                    highTextComp.color = new Color(1f, 0.65f, 0f, 1f);
                    highTextComp.text = "Best: " + PlayerPrefs.GetInt("HighScore", 0);
                    player.highScoreText = highTextComp;
                    Debug.Log("[AutoConfig] HighScoreText olusturuldu.");
                    changed = true;
                }
            }
        }

        // 4. CameraShake bileseni ekle
        if (mainCam != null)
        {
            if (mainCam.GetComponent<CameraShake>() == null)
            {
                mainCam.gameObject.AddComponent<CameraShake>();
                Debug.Log("[AutoConfig] CameraShake bileseni eklendi.");
                changed = true;
            }
        }

        // 5. Mavi Kapsul Prefabini olustur
        CreateBlueCapsulePrefab();

        // 6. Spawner'a kapsul prefabini ata
        Spawner spawner = GameObject.FindAnyObjectByType<Spawner>(FindObjectsInactive.Include);
        if (spawner != null && spawner.capsulePrefab == null)
        {
            GameObject capsuleAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BlueCapsule.prefab");
            if (capsuleAsset != null)
            {
                spawner.capsulePrefab = capsuleAsset;
                EditorUtility.SetDirty(spawner);
                changed = true;
                Debug.Log("[AutoConfig] Spawner'a capsulePrefab atandi.");
            }
        }

        // 7. Neon Trail Renderer ekle
        TrailRenderer trail = player.GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = player.gameObject.AddComponent<TrailRenderer>();
            changed = true;
        }

        if (trail != null)
        {
            trail.time = 0.35f;
            trail.startWidth = 0.45f;
            trail.endWidth = 0f;
            trail.minVertexDistance = 0.05f;
            trail.alignment = LineAlignment.View;

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0f, 0.8f, 1f), 0.0f),
                    new GradientColorKey(new Color(0f, 0.3f, 1f), 1.0f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.7f, 0.0f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            trail.colorGradient = gradient;

            string trailMatPath = "Assets/Materials/NeonTrailMat.mat";
            Material trailMat = AssetDatabase.LoadAssetAtPath<Material>(trailMatPath);
            if (trailMat == null)
            {
                if (!System.IO.Directory.Exists("Assets/Materials"))
                    System.IO.Directory.CreateDirectory("Assets/Materials");

                trailMat = new Material(Shader.Find("Sprites/Default"));
                AssetDatabase.CreateAsset(trailMat, trailMatPath);
            }

            if (trail.sharedMaterial != trailMat)
            {
                trail.sharedMaterial = trailMat;
                changed = true;
            }
        }

        // 8. Orijinal Player Gorunumunu Garantiye Al
        MeshRenderer playerRenderer = player.GetComponent<MeshRenderer>();
        if (playerRenderer != null && !playerRenderer.enabled)
        {
            playerRenderer.enabled = true;
            changed = true;
        }

        // Eklenen Saru veya baska modeller varsa temizle
        Transform existingSaru = player.transform.Find("SaruModel");
        if (existingSaru != null)
        {
            GameObject.DestroyImmediate(existingSaru.gameObject);
            changed = true;
            Debug.Log("[AutoConfig] SaruModel temizlendi.");
        }

        Transform existingSlime = player.transform.Find("Slime");
        if (existingSlime != null)
        {
            GameObject.DestroyImmediate(existingSlime.gameObject);
            changed = true;
            Debug.Log("[AutoConfig] Slime temizlendi.");
        }

        if (changed)
        {
            EditorUtility.SetDirty(player);
            if (mainCam != null) EditorUtility.SetDirty(mainCam.gameObject);
            if (trail != null) EditorUtility.SetDirty(trail);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[AutoConfig] Sahne kaydedildi.");
        }

        SessionState.SetBool("SceneConfiguredCleanFinal", true);
    }

    private static void CreateBlueCapsulePrefab()
    {
        string folderPath = "Assets/Prefabs";
        if (!System.IO.Directory.Exists(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }

        string prefabPath = folderPath + "/BlueCapsule.prefab";

        if (System.IO.File.Exists(prefabPath))
            return;

        GameObject tempCapsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        tempCapsule.name = "BlueCapsule";
        tempCapsule.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        string matPath = "Assets/Materials/BlueCapsuleMat.mat";
        Material capMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (capMat == null)
        {
            if (!System.IO.Directory.Exists("Assets/Materials"))
                System.IO.Directory.CreateDirectory("Assets/Materials");

            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            capMat = new Material(urpShader != null ? urpShader : Shader.Find("Standard"));
            capMat.SetColor("_BaseColor", new Color(0f, 0.7f, 1f));
            capMat.SetFloat("_Metallic", 0.1f);
            capMat.SetFloat("_Smoothness", 0.9f);
            capMat.EnableKeyword("_EMISSION");
            capMat.SetColor("_EmissionColor", new Color(0f, 0.5f, 1f) * 2f);
            AssetDatabase.CreateAsset(capMat, matPath);
        }

        MeshRenderer mr = tempCapsule.GetComponent<MeshRenderer>();
        if (mr != null) mr.sharedMaterial = capMat;

        tempCapsule.AddComponent<SlowMotionCapsule>();
        tempCapsule.tag = "Untagged";

        bool success;
        PrefabUtility.SaveAsPrefabAsset(tempCapsule, prefabPath, out success);
        GameObject.DestroyImmediate(tempCapsule);

        if (success)
        {
            AssetDatabase.Refresh();
            Debug.Log("[AutoConfig] BlueCapsule prefabi olusturuldu.");
        }
    }
}
