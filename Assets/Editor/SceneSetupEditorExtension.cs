using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SceneSetupEditor 확장
/// [FIX] AddGalleryToMainMenu — stopButton SerializedProperty에 null 직접 대입 불가 → Skip
/// [추가] AddStageProgressUI — GameScene에 HUD 자동 생성
/// </summary>
public static class SceneSetupEditorExtension
{
    [MenuItem("BridgeRhythm/Add Missing Components/BGMManager")]
    public static void AddBGMManager()
    {
        var managers = GameObject.Find("--- MANAGERS ---");
        if (managers == null) { Debug.LogError("[Setup] --- MANAGERS --- 없음"); return; }
        if (Object.FindObjectOfType<BGMManager>() != null) { Debug.Log("[Setup] BGMManager 이미 존재"); return; }

        var go = new GameObject("BGMManager");
        go.transform.SetParent(managers.transform);
        go.AddComponent<BGMManager>();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[Setup] BGMManager 추가");
    }

    [MenuItem("BridgeRhythm/Add Missing Components/Obstacle VFX to All Prefabs")]
    public static void AddObstacleVFXToPrefabs()
    {
        string[] names = { "WindZone","Laser","GravityZone","TimedGate","IceZone",
                           "MovingObstacle","FallingObstacle","Spike","Bouncer" };
        int count = 0;
        foreach (var n in names)
        {
            string path = $"Assets/Prefabs/{n}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;
            using var scope = new PrefabUtility.EditPrefabContentsScope(path);
            var root = scope.prefabContentsRoot;
            if (root.GetComponent<ObstacleVisualEffect>() == null)
            { root.AddComponent<ObstacleVisualEffect>(); count++; }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[Setup] ObstacleVisualEffect {count}개 추가");
    }

    [MenuItem("BridgeRhythm/Add Missing Components/PlayerVisual to Player Prefab")]
    public static void AddPlayerVisual()
    {
        string path = "Assets/Prefabs/Player.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
        { Debug.LogError("[Setup] Player.prefab 없음"); return; }
        using var scope = new PrefabUtility.EditPrefabContentsScope(path);
        var root = scope.prefabContentsRoot;
        if (root.GetComponent<PlayerVisual>() == null)
            root.AddComponent<PlayerVisual>();
        AssetDatabase.SaveAssets();
        Debug.Log("[Setup] PlayerVisual 추가");
    }

    // ── [추가] StageProgressUI HUD ────────────────────────

    [MenuItem("BridgeRhythm/Add Missing Components/StageProgressUI HUD")]
    public static void AddStageProgressUI()
    {
        var canvas = GameObject.Find("GameUICanvas");
        if (canvas == null) { Debug.LogError("[Setup] GameUICanvas 없음"); return; }

        if (Object.FindObjectOfType<StageProgressUI>() != null)
        { Debug.Log("[Setup] StageProgressUI 이미 존재"); return; }

        // HUD 루트 (우상단)
        var hud = new GameObject("StageProgressHUD");
        hud.transform.SetParent(canvas.transform, false);
        var hudRt = hud.AddComponent<RectTransform>();
        hudRt.anchorMin = new Vector2(0, 1);
        hudRt.anchorMax = new Vector2(1, 1);
        hudRt.pivot     = new Vector2(0.5f, 1f);
        hudRt.sizeDelta = new Vector2(0, 90);
        hudRt.anchoredPosition = Vector2.zero;
        var hudImg = hud.AddComponent<Image>();
        hudImg.color = new Color(0, 0, 0, 0.35f);

        var spUI = hud.AddComponent<StageProgressUI>();

        // 스테이지 라벨
        var stageLbl  = CreateText(hud.transform, "StageLabel",    "Stage 1 / 15", new Vector2(-350, -22), new Vector2(220, 30), 18);
        var stageNm   = CreateText(hud.transform, "StageName",     "Tutorial",     new Vector2(-350, -52), new Vector2(220, 26), 14);
        var budgetTxt = CreateText(hud.transform, "BudgetText",    "조각 남음: 30", new Vector2(0,   -22), new Vector2(200, 30), 16);
        var instTxt   = CreateText(hud.transform, "SelectedInst",  "♪ Piano",      new Vector2(0,   -52), new Vector2(200, 26), 14);
        var gradeTxt  = CreateText(hud.transform, "GradeText",     "S",            new Vector2(350, -35), new Vector2(60,  50), 36);

        // 효율 슬라이더
        var effSlider = CreateSlider(hud.transform, "EfficiencySlider", new Vector2(160, -22), new Vector2(180, 16));
        var lineSlider= CreateSlider(hud.transform, "RhythmLineSlider", new Vector2(160, -52), new Vector2(180, 12));

        // StageProgressUI 필드 연결
        var so = new SerializedObject(spUI);
        so.FindProperty("stageLabel").objectReferenceValue        = stageLbl;
        so.FindProperty("stageName").objectReferenceValue         = stageNm;
        so.FindProperty("budgetText").objectReferenceValue        = budgetTxt;
        so.FindProperty("selectedInstName").objectReferenceValue  = instTxt;
        so.FindProperty("gradeText").objectReferenceValue         = gradeTxt;
        so.FindProperty("efficiencySlider").objectReferenceValue  = effSlider;
        so.FindProperty("rhythmLineSlider").objectReferenceValue  = lineSlider;
        // efficiencyFill: 슬라이더 Fill Image 찾기
        var fillImg = effSlider.fillRect?.GetComponent<Image>();
        if (fillImg != null)
            so.FindProperty("efficiencyFill").objectReferenceValue = fillImg;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[Setup] StageProgressUI HUD 추가");
    }

    // ── Gallery ───────────────────────────────────────────

    [MenuItem("BridgeRhythm/Add Missing Components/Gallery to MainMenu")]
    public static void AddGalleryToMainMenu()
    {
        var canvas = GameObject.Find("MainMenuCanvas");
        if (canvas == null) { Debug.LogError("[Setup] MainMenuCanvas 없음"); return; }

        if (GameObject.Find("GalleryButton") == null)
            CreateButton(canvas.transform, "GalleryButton", "음악 갤러리", new Vector2(0, -240));

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
            var closeBtn  = CreateButton(galleryRoot.transform, "CloseButton", "닫기", new Vector2(0, -300));
            var stopBtn   = CreateButton(galleryRoot.transform, "StopButton",  "정지", new Vector2(180, -300));

            var tabParent  = CreateScrollParent(galleryRoot.transform, "StageTabs",
                new Vector2(0, 0.85f), new Vector2(1, 1f), true);
            var listParent = CreateScrollParent(galleryRoot.transform, "RecordList",
                new Vector2(0, 0.1f),  new Vector2(1, 0.85f), false);

            var nowPlaying = CreateText(galleryRoot.transform, "NowPlayingText", "", new Vector2(0, -260), new Vector2(600, 36), 18);

            var so = new SerializedObject(galleryUI);
            so.FindProperty("root").objectReferenceValue              = galleryRoot;
            so.FindProperty("stageTabParent").objectReferenceValue    = tabParent.transform;
            so.FindProperty("recordListParent").objectReferenceValue  = listParent.transform;
            so.FindProperty("nowPlayingText").objectReferenceValue    = nowPlaying;
            // [FIX] stopButton 및 closeButton을 Component로 직접 연결
            so.FindProperty("stopButton").objectReferenceValue        = stopBtn.GetComponent<Button>();
            so.FindProperty("closeButton").objectReferenceValue       = closeBtn.GetComponent<Button>();
            so.FindProperty("stageTabPrefab").objectReferenceValue    =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/StageTab.prefab");
            so.FindProperty("recordEntryPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/RecordEntry.prefab");
            so.ApplyModifiedProperties();
            galleryRoot.SetActive(false);
        }

        var ctrl = Object.FindObjectOfType<MainMenuController>();
        if (ctrl != null)
        {
            var gui = Object.FindObjectOfType<MusicGalleryUI>(true);
            var btn = GameObject.Find("GalleryButton")?.GetComponent<Button>();
            var so  = new SerializedObject(ctrl);
            if (gui != null) so.FindProperty("galleryUI").objectReferenceValue     = gui;
            if (btn != null) so.FindProperty("galleryButton").objectReferenceValue = btn;
            so.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[Setup] Gallery to MainMenu 완료");
    }

    [MenuItem("BridgeRhythm/Add Missing Components/EndingVisual to EndingScene")]
    public static void AddEndingVisual()
    {
        var ctrl = Object.FindObjectOfType<EndingController>();
        if (ctrl == null) { Debug.LogError("[Setup] EndingController 없음"); return; }

        if (ctrl.GetComponent<EndingVisual>() == null)
        {
            var ev = ctrl.gameObject.AddComponent<EndingVisual>();
            var pr = new GameObject("ParticleRoot");
            pr.transform.SetParent(ctrl.transform, false);

            var so = new SerializedObject(ev);
            var bgImg = Object.FindObjectOfType<Image>();
            if (bgImg != null) so.FindProperty("backgroundImage").objectReferenceValue = bgImg;
            so.FindProperty("particleRoot").objectReferenceValue = pr.transform;

            foreach (var t in Object.FindObjectsOfType<TextMeshProUGUI>())
            {
                if (t.name.Contains("Title") || t.name == "Label")
                { so.FindProperty("endingTitleText").objectReferenceValue = t; break; }
            }
            so.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[Setup] EndingVisual 추가");
    }

    [MenuItem("BridgeRhythm/Add ALL Missing TODO Items", priority = 1)]
    public static void AddAllTodoItems()
    {
        AddBGMManager();
        AddPlayerVisual();
        AddObstacleVFXToPrefabs();
        AddStageProgressUI();
        Debug.Log("[Setup] 전체 TODO 적용 완료");
    }

    // ── UI 헬퍼 ───────────────────────────────────────────

    private static GameObject CreateButton(Transform parent, string name, string label, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = new Color(0.25f, 0.25f, 0.3f, 0.92f);
        go.AddComponent<Button>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(200, 55);
        var tgo = new GameObject("Label");
        tgo.transform.SetParent(go.transform, false);
        var tmp = tgo.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 22;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var trt = tgo.GetComponent<RectTransform>();
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

    private static Slider CreateSlider(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        var go  = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt  = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = size;

        var bg  = new GameObject("Background");  bg.transform.SetParent(go.transform, false);
        bg.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        var brt = bg.GetComponent<RectTransform>();
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
        brt.offsetMin = brt.offsetMax = Vector2.zero;

        var fa  = new GameObject("Fill Area"); fa.transform.SetParent(go.transform, false);
        var fart= fa.GetComponent<RectTransform>();
        if (fart == null) fart = fa.AddComponent<RectTransform>();
        fart.anchorMin = Vector2.zero; fart.anchorMax = Vector2.one;
        fart.offsetMin = fart.offsetMax = Vector2.zero;

        var fill= new GameObject("Fill");      fill.transform.SetParent(fa.transform, false);
        fill.AddComponent<Image>().color = new Color(0.3f, 0.8f, 0.4f);
        var frt = fill.GetComponent<RectTransform>();
        frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
        frt.offsetMin = frt.offsetMax = Vector2.zero;

        var slider = go.AddComponent<Slider>();
        slider.fillRect = frt;
        slider.targetGraphic = bg.GetComponent<Image>();
        slider.minValue = 0f; slider.maxValue = 1f; slider.value = 0f;
        slider.interactable = false;
        return slider;
    }

    private static GameObject CreateScrollParent(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, bool horizontal)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        if (horizontal)
        {
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 5f;
            hlg.childForceExpandWidth = false;
        }
        else
        {
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5f;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childForceExpandHeight = false;
        }
        return go;
    }
}
