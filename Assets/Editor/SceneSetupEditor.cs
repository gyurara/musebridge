using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 유니티 에디터 메뉴에서 씬 계층구조를 자동 생성하는 에디터 툴
/// 상단 메뉴 → BridgeRhythm → Setup [씬이름] 클릭
/// </summary>
public class SceneSetupEditor : EditorWindow
{
    [MenuItem("BridgeRhythm/Setup GameScene")]
    public static void SetupGameScene()
    {
        SetupManagers();
        SetupRhythmLine();
        SetupCamera();
        SetupDeathZone();
        SetupBackground();
        SetupGameUI();
        SetupLevelGeometry();
        AutoWireAssets();
        MarkSceneDirty();
        Debug.Log("[SceneSetup] GameScene 설정 완료! Ctrl+S 로 저장하세요.");
    }

    [MenuItem("BridgeRhythm/Setup MainMenu")]
    public static void SetupMainMenu()
    {
        var canvas = CreateCanvas("MainMenuCanvas");
        var root = canvas.gameObject;

        CreateText(root.transform, "TitleText", "Bridge Melody",
            new Vector2(0, 100), new Vector2(600, 100), 60);
        CreateText(root.transform, "SubtitleText", "Make a bridge with music",
            new Vector2(0, 20), new Vector2(600, 50), 24);
        CreateButton(root.transform, "StartButton", "START", new Vector2(0, -80));
        CreateButton(root.transform, "QuitButton",  "QUIT",  new Vector2(0, -160));

        var ctrl = new GameObject("MainMenuController");
        ctrl.AddComponent<MainMenuController>();

        MarkSceneDirty();
        Debug.Log("[SceneSetup] MainMenu 설정 완료! Ctrl+S 로 저장하세요.");
    }

    [MenuItem("BridgeRhythm/Setup EndingScene")]
    public static void SetupEndingScene()
    {
        var canvas = CreateCanvas("EndingCanvas");
        var root = canvas.gameObject;

        CreateText(root.transform, "TotalScoreText", "Total Score: 0",
            new Vector2(0, 100), new Vector2(600, 80), 40);

        var bestUI = new GameObject("BestEndingUI");
        bestUI.transform.SetParent(root.transform, false);
        CreateText(bestUI.transform, "Label", "BEST ENDING - True Musician!", Vector2.zero, new Vector2(700, 80), 36);

        var normalUI = new GameObject("NormalEndingUI");
        normalUI.transform.SetParent(root.transform, false);
        CreateText(normalUI.transform, "Label", "You crossed the bridge with music.", Vector2.zero, new Vector2(700, 80), 36);

        var badUI = new GameObject("BadEndingUI");
        badUI.transform.SetParent(root.transform, false);
        CreateText(badUI.transform, "Label", "Noise covered the world...", Vector2.zero, new Vector2(700, 80), 36);

        var retBtn = CreateButton(root.transform, "ReturnButton", "TITLE", new Vector2(0, -150));

        var ctrlObj = new GameObject("EndingController");
        var ctrl = ctrlObj.AddComponent<EndingController>();

        // EndingController 자동 연결
        var so = new SerializedObject(ctrl);
        so.FindProperty("bestEndingUI").objectReferenceValue = bestUI;
        so.FindProperty("normalEndingUI").objectReferenceValue = normalUI;
        so.FindProperty("badEndingUI").objectReferenceValue = badUI;
        so.FindProperty("totalScoreText").objectReferenceValue =
            GameObject.Find("TotalScoreText")?.GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedProperties();

        // ReturnButton → OnReturnToMainMenuClicked 연결
        var btnComp = retBtn.GetComponent<Button>();
        if (btnComp != null)
        {
            var serialBtn = new SerializedObject(btnComp);
            // 에디터에서 직접 연결은 Inspector에서 하거나, AddListener는 런타임 전용
            // → 간단히 EndingController.OnReturnToMainMenuClicked을 Inspector에서 연결할 것을 주석으로 안내
            Debug.Log("[SceneSetup] ReturnButton의 onClick에 EndingController.OnReturnToMainMenuClicked을 Inspector에서 연결하세요.");
        }

        MarkSceneDirty();
        Debug.Log("[SceneSetup] EndingScene 설정 완료! Ctrl+S 로 저장하세요.");
    }

    // ── 개별 설정 메서드 ──────────────────────────────────

    private static void SetupManagers()
    {
        var managers = new GameObject("--- MANAGERS ---");

        AddTo<GameManager>(managers);
        var audioManagerComp = AddTo<AudioManager>(managers);
        AddTo<StageManager>(managers);
        AddTo<UIManager>(managers);
        AddTo<BuildPhaseManager>(managers);
        AddTo<PlayPhaseManager>(managers);
        AddTo<BridgeBuilder>(managers);
        AddTo<EfficiencyScoreManager>(managers);
        AddTo<StageBuilder>(managers);

        // AudioSource 전용 자식
        var sfxObj = new GameObject("SfxSource");
        sfxObj.transform.SetParent(managers.transform);
        var sfxSrc = sfxObj.AddComponent<AudioSource>();
        sfxSrc.playOnAwake = false;

        var musicObj = new GameObject("MusicSource");
        musicObj.transform.SetParent(managers.transform);
        var musicSrc = musicObj.AddComponent<AudioSource>();
        musicSrc.playOnAwake = false;

        // AudioManager 자동 연결
        var so = new SerializedObject(audioManagerComp);
        so.FindProperty("sfxSource").objectReferenceValue = sfxSrc;
        so.FindProperty("musicSource").objectReferenceValue = musicSrc;
        so.ApplyModifiedProperties();
    }

    private static void SetupRhythmLine()
    {
        var lineObj = new GameObject("RhythmLine");
        lineObj.AddComponent<RhythmLineController>();
        lineObj.AddComponent<RhythmLineVisual>();
    }

    private static void SetupCamera()
    {
        var cam = Camera.main?.gameObject ?? new GameObject("Main Camera");
        if (cam.GetComponent<CameraFollow>() == null)
            cam.AddComponent<CameraFollow>();
    }

    private static void SetupDeathZone()
    {
        var dz = new GameObject("DeathZone");
        dz.tag = "DeathZone";
        var col = dz.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(100f, 1f);
        dz.transform.position = new Vector3(0, -12f, 0);
        dz.AddComponent<DeathZone>();
    }

    private static void SetupBackground()
    {
        var bg = new GameObject("SheetMusicBackground");
        var canvas = bg.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        bg.AddComponent<SheetMusicBackground>();
    }

    private static void SetupGameUI()
    {
        var canvas = CreateCanvas("GameUICanvas");
        var root = canvas.gameObject;

        // 빌드 페이즈 패널 (왼쪽 사이드바)
        var buildPanel = new GameObject("BuildPhasePanel");
        buildPanel.transform.SetParent(root.transform, false);
        var buildImg = buildPanel.AddComponent<Image>();
        buildImg.color = new Color(0, 0, 0, 0.3f);
        var buildRt = buildPanel.GetComponent<RectTransform>();
        buildRt.anchorMin = new Vector2(0, 0);
        buildRt.anchorMax = new Vector2(0, 1);
        buildRt.pivot = new Vector2(0, 0.5f);
        buildRt.sizeDelta = new Vector2(200, 0);
        buildRt.anchoredPosition = Vector2.zero;

        // 악기 사이드바
        var sidebar = new GameObject("InstrumentSidebar");
        sidebar.transform.SetParent(buildPanel.transform, false);
        var sidebarRt = sidebar.AddComponent<RectTransform>();
        sidebarRt.anchorMin = Vector2.zero;
        sidebarRt.anchorMax = Vector2.one;
        sidebarRt.offsetMin = new Vector2(10, 10);
        sidebarRt.offsetMax = new Vector2(-10, -10);
        var layout = sidebar.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(5, 5, 10, 10);
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // 플레이 페이즈 패널 (상단바)
        var playPanel = new GameObject("PlayPhasePanel");
        playPanel.transform.SetParent(root.transform, false);
        var playImg = playPanel.AddComponent<Image>();
        playImg.color = new Color(0, 0, 0, 0.2f);
        var playRt = playPanel.GetComponent<RectTransform>();
        playRt.anchorMin = new Vector2(0, 1);
        playRt.anchorMax = new Vector2(1, 1);
        playRt.pivot = new Vector2(0.5f, 1f);
        playRt.sizeDelta = new Vector2(0, 50);
        playRt.anchoredPosition = Vector2.zero;

        // 효율 점수 텍스트
        var effText = CreateText(root.transform, "EfficiencyScoreText", "Score: 0",
            new Vector2(-10, -10), new Vector2(200, 40), 20);

        // 음악 감상 패널 (뮤직 리뷰 UI용)
        var musicPanel = new GameObject("MusicReviewPanel");
        musicPanel.transform.SetParent(root.transform, false);
        var musicImg = musicPanel.AddComponent<Image>();
        musicImg.color = new Color(0f, 0f, 0f, 0.5f);
        var musicRt = musicPanel.GetComponent<RectTransform>();
        musicRt.anchorMin = Vector2.zero;
        musicRt.anchorMax = Vector2.one;
        musicRt.offsetMin = musicRt.offsetMax = Vector2.zero;
        var musicText = CreateText(musicPanel.transform, "MusicReviewText", "당신이 만든 음악을 감상하세요!",
            Vector2.zero, new Vector2(700, 80), 28);
        musicPanel.SetActive(false);

        // [FIX] StageResultUI를 결과 패널에 올바르게 생성 + 필드 연결
        var resultPanelObj = SetupResultPanel(root.transform);

        // UIManager 자동 연결
        var uiManagerGO = GameObject.Find("UIManager");
        if (uiManagerGO != null)
        {
            var uiManager = uiManagerGO.GetComponent<UIManager>();
            if (uiManager != null)
            {
                var so = new SerializedObject(uiManager);
                so.FindProperty("buildPhasePanel").objectReferenceValue = buildPanel;
                so.FindProperty("playPhasePanel").objectReferenceValue = playPanel;
                so.FindProperty("musicReviewPanel").objectReferenceValue = musicPanel;
                so.FindProperty("musicReviewText").objectReferenceValue = musicText;
                so.FindProperty("instrumentSidebarParent").objectReferenceValue = sidebar.transform;
                so.FindProperty("efficiencyScoreText").objectReferenceValue = effText;
                so.ApplyModifiedProperties();
            }
        }
    }

    /// <summary>
    /// [FIX] StageResultUI 패널 생성 + 모든 필드(resultPanel, canvasGroup, 텍스트들, 버튼들) 자동 연결
    /// 원본 코드는 resultPanel 필드 연결 없이 SetActive(false)만 했음 → Show()에서 null 오류
    /// </summary>
    private static GameObject SetupResultPanel(Transform parent)
    {
        // 바깥 래퍼 (StageResultUI 컴포넌트를 품는 오브젝트)
        var wrapper = new GameObject("StageResultUIRoot");
        wrapper.transform.SetParent(parent, false);
        var wrapperRt = wrapper.AddComponent<RectTransform>();
        wrapperRt.anchorMin = Vector2.zero;
        wrapperRt.anchorMax = Vector2.one;
        wrapperRt.offsetMin = wrapperRt.offsetMax = Vector2.zero;
        var stageResultUI = wrapper.AddComponent<StageResultUI>();

        // 실제 패널 (어둡고 중앙 정렬)
        var panel = new GameObject("ResultPanel");
        panel.transform.SetParent(wrapper.transform, false);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.05f, 0.1f, 0.88f);
        var panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(520, 420);
        panelRt.anchoredPosition = Vector2.zero;
        var cg = panel.AddComponent<CanvasGroup>();

        // 텍스트들
        var stageNameTxt   = CreateText(panel.transform, "StageNameText",   "Stage Clear!",       new Vector2(0, 165), new Vector2(480, 50), 30);
        var scoreTxt       = CreateText(panel.transform, "ScoreText",        "효율 점수: 0",         new Vector2(0, 110), new Vector2(480, 40), 24);
        var gradeTxt       = CreateText(panel.transform, "GradeText",        "A",                  new Vector2(0, 55),  new Vector2(160, 70), 52);
        var usageTxt       = CreateText(panel.transform, "UsageCountText",   "악기 사용 횟수: 0",    new Vector2(0, 10),  new Vector2(480, 36), 20);
        var musicReviewLbl = CreateText(panel.transform, "MusicReviewLabel", "잠시 후 음악이 재생됩니다...", new Vector2(0, -35), new Vector2(480, 36), 18);

        // 버튼들
        var nextBtn  = CreateButton(panel.transform, "NextStageButton", "다음 스테이지", new Vector2(90,  -145));
        var retryBtn = CreateButton(panel.transform, "RetryButton",     "다시 도전",    new Vector2(-90, -145));

        panel.SetActive(false); // 시작 시 숨김

        // StageResultUI 필드 연결
        var so = new SerializedObject(stageResultUI);
        so.FindProperty("resultPanel").objectReferenceValue     = panel;
        so.FindProperty("canvasGroup").objectReferenceValue     = cg;
        so.FindProperty("stageNameText").objectReferenceValue   = stageNameTxt;
        so.FindProperty("scoreText").objectReferenceValue       = scoreTxt;
        so.FindProperty("gradeText").objectReferenceValue       = gradeTxt;
        so.FindProperty("usageCountText").objectReferenceValue  = usageTxt;
        so.FindProperty("musicReviewLabel").objectReferenceValue= musicReviewLbl;
        so.FindProperty("nextStageButton").objectReferenceValue = nextBtn.GetComponent<Button>();
        so.FindProperty("retryButton").objectReferenceValue     = retryBtn.GetComponent<Button>();
        so.ApplyModifiedProperties();

        return wrapper;
    }

    private static void SetupLevelGeometry()
    {
        new GameObject("--- LEVEL ---");

        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        if (playerPrefab != null)
        {
            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.transform.position = new Vector3(-11f, 0f, 0f);
        }

        var startPos = new GameObject("PlayerStartPosition");
        startPos.transform.position = new Vector3(-11f, 0f, 0f);
    }

    // ── 자산 자동 연결 ──────────────────────────────────────

    private static void AutoWireAssets()
    {
        WireBridgeBuilder();
        WireStageManager();
        WireUIManager();
        WirePlayPhaseManager();
        WireCameraFollow();
        WireStageBuilder();
    }

    private static void WireStageBuilder()
    {
        var sb = Object.FindObjectOfType<StageBuilder>();
        if (sb == null) return;
        var so = new SerializedObject(sb);
        void Set(string name, string path)
        {
            var prop = so.FindProperty(name);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prop != null && prefab != null) prop.objectReferenceValue = prefab;
        }
        Set("movingObstaclePrefab",  "Assets/Prefabs/MovingObstacle.prefab");
        Set("fallingObstaclePrefab", "Assets/Prefabs/FallingObstacle.prefab");
        Set("windZonePrefab",        "Assets/Prefabs/WindZone.prefab");
        Set("spikePrefab",           "Assets/Prefabs/Spike.prefab");
        Set("bouncerPrefab",         "Assets/Prefabs/Bouncer.prefab");
        Set("gravityZonePrefab",     "Assets/Prefabs/GravityZone.prefab");
        Set("timedGatePrefab",       "Assets/Prefabs/TimedGate.prefab");
        Set("laserPrefab",           "Assets/Prefabs/Laser.prefab");
        Set("iceZonePrefab",         "Assets/Prefabs/IceZone.prefab");
        Set("instrumentPickupPrefab","Assets/Prefabs/InstrumentPickup.prefab");
        Set("groundPrefab",          "Assets/Prefabs/Ground.prefab");
        Set("goalPrefab",            "Assets/Prefabs/Goal.prefab");
        so.ApplyModifiedProperties();
    }

    private static void WireBridgeBuilder()
    {
        var bb = Object.FindObjectOfType<BridgeBuilder>();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BridgePiece.prefab");
        if (bb == null || prefab == null) return;
        var so = new SerializedObject(bb);
        so.FindProperty("bridgePiecePrefab").objectReferenceValue = prefab;
        so.ApplyModifiedProperties();
    }

    private static void WireStageManager()
    {
        var sm = Object.FindObjectOfType<StageManager>();
        if (sm == null) return;

        var guids = AssetDatabase.FindAssets("t:StageData", new[] { "Assets/ScriptableObjects" });
        var list = new System.Collections.Generic.List<StageData>();
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var sd = AssetDatabase.LoadAssetAtPath<StageData>(path);
            if (sd != null) list.Add(sd);
        }
        // [FIX] stageIndex(0-based) 기준으로 정렬
        list.Sort((a, b) => a.stageIndex.CompareTo(b.stageIndex));

        var so = new SerializedObject(sm);
        var prop = so.FindProperty("allStages");
        prop.arraySize = list.Count;
        for (int i = 0; i < list.Count; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = list[i];

        var piano = AssetDatabase.LoadAssetAtPath<InstrumentData>("Assets/ScriptableObjects/Piano.asset");
        if (piano != null)
            so.FindProperty("defaultInstrument").objectReferenceValue = piano;

        so.ApplyModifiedProperties();
    }

    private static void WireUIManager()
    {
        var ui = Object.FindObjectOfType<UIManager>();
        var btnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/InstrumentSidebarButton.prefab");
        if (ui == null || btnPrefab == null) return;
        var so = new SerializedObject(ui);
        so.FindProperty("instrumentButtonPrefab").objectReferenceValue = btnPrefab;
        so.ApplyModifiedProperties();
    }

    private static void WirePlayPhaseManager()
    {
        var pm = Object.FindObjectOfType<PlayPhaseManager>();
        if (pm == null) return;

        var player   = GameObject.FindWithTag("Player");
        var startPos = GameObject.Find("PlayerStartPosition");
        if (player == null) return;

        var so = new SerializedObject(pm);
        so.FindProperty("player").objectReferenceValue              = player.GetComponent<PlayerController>();
        if (startPos != null)
            so.FindProperty("playerStartPosition").objectReferenceValue = startPos.transform;
        so.ApplyModifiedProperties();
    }

    private static void WireCameraFollow()
    {
        var cam    = Object.FindObjectOfType<CameraFollow>();
        var player = GameObject.FindWithTag("Player");
        if (cam == null || player == null) return;

        var so = new SerializedObject(cam);
        var targetProp = so.FindProperty("target");
        if (targetProp != null) targetProp.objectReferenceValue = player.transform;
        so.ApplyModifiedProperties();
    }

    private static void MarkSceneDirty()
    {
        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
    }

    // ── 헬퍼 ──────────────────────────────────────────────

    private static T AddTo<T>(GameObject parent) where T : Component
    {
        var child = new GameObject(typeof(T).Name);
        child.transform.SetParent(parent.transform);
        return child.AddComponent<T>();
    }

    private static Canvas CreateCanvas(string name)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text,
        Vector2 anchoredPos, Vector2 size, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;  // [FIX] 어두운 패널 배경 위에서 보이도록 흰색
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        return tmp;
    }

    private static GameObject CreateButton(Transform parent, string name, string label, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);
        var btn = go.AddComponent<Button>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(200, 60);

        var textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;

        return go;
    }
}
