using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SceneSetupEditor 확장 — TODO 완료 항목 자동 세팅 추가
///
/// [추가] BridgeRhythm → Setup All Scenes (한 방에 3개 씬 전부)
/// [추가] BridgeRhythm → Add TODO Items → BGMManager, VFX, Gallery 등
/// </summary>
public static class SceneSetupEditorExtension
{
    // ── BGMManager 추가 ───────────────────────────────────

    [MenuItem("BridgeRhythm/Add Missing Components/BGMManager")]
    public static void AddBGMManager()
    {
        var managers = GameObject.Find("--- MANAGERS ---");
        if (managers == null)
        {
            Debug.LogError("[Setup] --- MANAGERS --- 오브젝트 없음. Setup GameScene 먼저 실행하세요.");
            return;
        }

        if (Object.FindObjectOfType<BGMManager>() != null)
        {
            Debug.Log("[Setup] BGMManager 이미 존재.");
            return;
        }

        var go = new GameObject("BGMManager");
        go.transform.SetParent(managers.transform);
        go.AddComponent<BGMManager>();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[Setup] BGMManager 추가 완료.");
    }

    // ── ObstacleVisualEffect 프리팹에 추가 ────────────────

    [MenuItem("BridgeRhythm/Add Missing Components/Obstacle VFX to All Prefabs")]
    public static void AddObstacleVFXToPrefabs()
    {
        string[] prefabNames = {
            "WindZone", "Laser", "GravityZone", "TimedGate", "IceZone",
            "MovingObstacle", "FallingObstacle", "Spike", "Bouncer"
        };

        int count = 0;
        foreach (var name in prefabNames)
        {
            string path = $"Assets/Prefabs/{name}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var root = scope.prefabContentsRoot;
                if (root.GetComponent<ObstacleVisualEffect>() == null)
                {
                    root.AddComponent<ObstacleVisualEffect>();
                    count++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[Setup] ObstacleVisualEffect {count}개 프리팹에 추가 완료.");
    }

    // ── PlayerVisual 플레이어 프리팹에 추가 ─────────────

    [MenuItem("BridgeRhythm/Add Missing Components/PlayerVisual to Player Prefab")]
    public static void AddPlayerVisual()
    {
        string path = "Assets/Prefabs/Player.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError("[Setup] Player.prefab 없음.");
            return;
        }

        using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
        {
            var root = scope.prefabContentsRoot;
            if (root.GetComponent<PlayerVisual>() == null)
            {
                root.AddComponent<PlayerVisual>();
                Debug.Log("[Setup] PlayerVisual 추가 완료.");
            }
            else Debug.Log("[Setup] PlayerVisual 이미 존재.");
        }
        AssetDatabase.SaveAssets();
    }

    // ── MainMenu에 Gallery 버튼 + MusicGalleryUI 추가 ────

    [MenuItem("BridgeRhythm/Add Missing Components/Gallery to MainMenu")]
    public static void AddGalleryToMainMenu()
    {
        var canvas = GameObject.Find("MainMenuCanvas");
        if (canvas == null)
        {
            Debug.LogError("[Setup] MainMenuCanvas 없음. Setup MainMenu 먼저.");
            return;
        }

        // 갤러리 버튼 추가
        if (GameObject.Find("GalleryButton") == null)
        {
            var btn = CreateButton(canvas.transform, "GalleryButton", "음악 갤러리", new Vector2(0, -240));
            Debug.Log("[Setup] GalleryButton 추가.");
        }

        // MusicGalleryUI 패널 추가 (숨김 상태)
        if (Object.FindObjectOfType<MusicGalleryUI>(true) == null)
        {
            var galleryRoot = new GameObject("MusicGalleryPanel");
            galleryRoot.transform.SetParent(canvas.transform, false);

            var img = galleryRoot.AddComponent<Image>();
            img.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);

            var rt = galleryRoot.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0.1f);
            rt.anchorMax = new Vector2(0.9f, 0.9f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var galleryUI = galleryRoot.AddComponent<MusicGalleryUI>();

            // Close 버튼
            var closeBtn = CreateButton(galleryRoot.transform, "CloseButton", "닫기", new Vector2(0, -300));

            // 스테이지 탭 영역
            var tabParent = new GameObject("StageTabs", typeof(RectTransform));
            tabParent.transform.SetParent(galleryRoot.transform, false);
            var tabRt = tabParent.GetComponent<RectTransform>();
            tabRt.anchorMin = new Vector2(0, 0.85f);
            tabRt.anchorMax = new Vector2(1, 1f);
            tabRt.offsetMin = tabRt.offsetMax = Vector2.zero;
            tabParent.AddComponent<HorizontalLayoutGroup>().spacing = 5f;

            // 레코드 목록 영역
            var listParent = new GameObject("RecordList", typeof(RectTransform));
            listParent.transform.SetParent(galleryRoot.transform, false);
            var listRt = listParent.GetComponent<RectTransform>();
            listRt.anchorMin = new Vector2(0, 0.1f);
            listRt.anchorMax = new Vector2(1, 0.85f);
            listRt.offsetMin = listRt.offsetMax = Vector2.zero;
            var vl = listParent.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 5f;
            vl.padding = new RectOffset(10, 10, 10, 10);

            // 재생 중 텍스트
            var nowPlaying = CreateText(galleryRoot.transform, "NowPlayingText", "", new Vector2(0, -260), new Vector2(600, 36), 18);

            // GalleryUI 필드 연결
            var so = new SerializedObject(galleryUI);
            so.FindProperty("root").objectReferenceValue             = galleryRoot;
            so.FindProperty("stageTabParent").objectReferenceValue   = tabParent.transform;
            so.FindProperty("recordListParent").objectReferenceValue = listParent.transform;
            so.FindProperty("nowPlayingText").objectReferenceValue   = nowPlaying;
            so.FindProperty("stopButton").objectReferenceValue       = null;
            so.FindProperty("closeButton").objectReferenceValue      = closeBtn.GetComponent<Button>();
            so.FindProperty("stageTabPrefab").objectReferenceValue   =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/StageTab.prefab");
            so.FindProperty("recordEntryPrefab").objectReferenceValue=
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/RecordEntry.prefab");
            so.ApplyModifiedProperties();

            galleryRoot.SetActive(false);
            Debug.Log("[Setup] MusicGalleryUI 패널 추가 완료.");
        }

        // MainMenuController에 galleryUI 연결
        var ctrl = Object.FindObjectOfType<MainMenuController>();
        if (ctrl != null)
        {
            var galleryUI = Object.FindObjectOfType<MusicGalleryUI>(true);
            var galBtn    = GameObject.Find("GalleryButton")?.GetComponent<Button>();
            var so = new SerializedObject(ctrl);
            if (galleryUI != null) so.FindProperty("galleryUI").objectReferenceValue = galleryUI;
            if (galBtn    != null) so.FindProperty("galleryButton").objectReferenceValue = galBtn;
            so.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    // ── EndingVisual 추가 ────────────────────────────────

    [MenuItem("BridgeRhythm/Add Missing Components/EndingVisual to EndingScene")]
    public static void AddEndingVisual()
    {
        var ctrl = Object.FindObjectOfType<EndingController>();
        if (ctrl == null)
        {
            Debug.LogError("[Setup] EndingController 없음. Setup EndingScene 먼저.");
            return;
        }

        if (ctrl.GetComponent<EndingVisual>() == null)
        {
            var ev = ctrl.gameObject.AddComponent<EndingVisual>();

            // 파티클 루트
            var pr = new GameObject("ParticleRoot");
            pr.transform.SetParent(ctrl.transform, false);

            var so = new SerializedObject(ev);

            // 배경 Image 탐색
            var bgImg = GameObject.Find("EndingCanvas")?.GetComponent<Image>()
                     ?? Object.FindObjectOfType<Image>();
            so.FindProperty("backgroundImage").objectReferenceValue = bgImg;
            so.FindProperty("particleRoot").objectReferenceValue    = pr.transform;

            // 타이틀 텍스트 탐색
            var tmpAll = Object.FindObjectsOfType<TextMeshProUGUI>();
            foreach (var t in tmpAll)
            {
                if (t.name == "Label" || t.name.Contains("Title"))
                {
                    so.FindProperty("endingTitleText").objectReferenceValue = t;
                    break;
                }
            }
            so.ApplyModifiedProperties();

            Debug.Log("[Setup] EndingVisual 추가 완료.");
        }
        else Debug.Log("[Setup] EndingVisual 이미 존재.");

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    // ── 전체 한 방에 ─────────────────────────────────────

    [MenuItem("BridgeRhythm/Add ALL Missing TODO Items", priority = 1)]
    public static void AddAllTodoItems()
    {
        Debug.Log("[Setup] TODO 전체 적용 시작...");
        AddBGMManager();
        AddPlayerVisual();
        AddObstacleVFXToPrefabs();
        Debug.Log("[Setup] TODO 전체 적용 완료! (Gallery/EndingVisual은 해당 씬에서 별도 실행)");
    }

    // ── UI 헬퍼 ───────────────────────────────────────────

    private static GameObject CreateButton(Transform parent, string name, string label, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.25f, 0.25f, 0.3f, 0.92f);
        go.AddComponent<Button>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(200, 55);

        var textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 22;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        return go;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text,
        Vector2 pos, Vector2 size, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        return tmp;
    }
}
