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
        CreateButton(root.transform, "QuitButton", "QUIT", new Vector2(0, -160));

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

        CreateButton(root.transform, "ReturnButton", "TITLE", new Vector2(0, -150));

        var ctrl = new GameObject("EndingController");
        ctrl.AddComponent<EndingController>();

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

        // AudioSource 전용 자식 오브젝트 생성
        var sfxObj = new GameObject("SfxSource");
        sfxObj.transform.SetParent(managers.transform);
        var sfxSrc = sfxObj.AddComponent<AudioSource>();
        sfxSrc.playOnAwake = false;

        var musicObj = new GameObject("MusicSource");
        musicObj.transform.SetParent(managers.transform);
        var musicSrc = musicObj.AddComponent<AudioSource>();
        musicSrc.playOnAwake = false;

        // AudioManager에 자동 연결
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
        CreateText(root.transform, "EfficiencyScoreText", "Score: 0",
            new Vector2(-10, -10), new Vector2(200, 40), 20);

        // 결과 패널
        var resultPanel = new GameObject("ResultPanel");
        resultPanel.transform.SetParent(root.transform, false);
        var resultImg = resultPanel.AddComponent<Image>();
        resultImg.color = new Color(0, 0, 0, 0.7f);
        var resultRt = resultPanel.GetComponent<RectTransform>();
        resultRt.anchorMin = new Vector2(0.5f, 0.5f);
        resultRt.anchorMax = new Vector2(0.5f, 0.5f);
        resultRt.pivot = new Vector2(0.5f, 0.5f);
        resultRt.sizeDelta = new Vector2(500, 400);
        resultRt.anchoredPosition = Vector2.zero;
        resultPanel.AddComponent<StageResultUI>();
        resultPanel.SetActive(false);

        // UIManager 자동 연결
        var uiManagerObj = GameObject.Find("UIManager");
        if (uiManagerObj != null)
        {
            var uiManager = uiManagerObj.GetComponent<UIManager>();
            if (uiManager != null)
            {
                var so = new SerializedObject(uiManager);
                so.FindProperty("buildPhasePanel").objectReferenceValue = buildPanel;
                so.FindProperty("playPhasePanel").objectReferenceValue = playPanel;
                so.FindProperty("instrumentSidebarParent").objectReferenceValue = sidebar.transform;
                so.FindProperty("efficiencyScoreText").objectReferenceValue =
                    GameObject.Find("EfficiencyScoreText")?.GetComponent<TextMeshProUGUI>();
                so.ApplyModifiedProperties();
            }
        }
    }

    // ── 레벨 지오메트리 (지형/목적지/플레이어) ─────────────

    private static void SetupLevelGeometry()
    {
        // StageBuilder가 런타임에 스테이지별 지형/장애물을 스폰하므로
        // 이 메서드는 플레이어와 시작 위치만 고정 배치
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

    // ── 생성된 자산 자동 연결 ──────────────────────────────

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
        Set("movingObstaclePrefab", "Assets/Prefabs/MovingObstacle.prefab");
        Set("fallingObstaclePrefab", "Assets/Prefabs/FallingObstacle.prefab");
        Set("windZonePrefab", "Assets/Prefabs/WindZone.prefab");
        Set("spikePrefab", "Assets/Prefabs/Spike.prefab");
        Set("bouncerPrefab", "Assets/Prefabs/Bouncer.prefab");
        Set("gravityZonePrefab", "Assets/Prefabs/GravityZone.prefab");
        Set("timedGatePrefab", "Assets/Prefabs/TimedGate.prefab");
        Set("laserPrefab", "Assets/Prefabs/Laser.prefab");
        Set("iceZonePrefab", "Assets/Prefabs/IceZone.prefab");
        Set("instrumentPickupPrefab", "Assets/Prefabs/InstrumentPickup.prefab");
        Set("groundPrefab", "Assets/Prefabs/Ground.prefab");
        Set("goalPrefab", "Assets/Prefabs/Goal.prefab");
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
        list.Sort((a, b) => a.stageIndex.CompareTo(b.stageIndex));

        var so = new SerializedObject(sm);
        var prop = so.FindProperty("allStages");
        prop.arraySize = list.Count;
        for (int i = 0; i < list.Count; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = list[i];

        // 기본 악기 = 피아노 (있으면)
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

        var player = GameObject.FindWithTag("Player");
        var startPos = GameObject.Find("PlayerStartPosition");
        if (player == null) return;

        var so = new SerializedObject(pm);
        so.FindProperty("player").objectReferenceValue = player.GetComponent<PlayerController>();
        if (startPos != null)
            so.FindProperty("playerStartPosition").objectReferenceValue = startPos.transform;
        so.ApplyModifiedProperties();
    }

    private static void WireCameraFollow()
    {
        var cam = Object.FindObjectOfType<CameraFollow>();
        var player = GameObject.FindWithTag("Player");
        if (cam == null || player == null) return;

        var so = new SerializedObject(cam);
        var targetProp = so.FindProperty("target");
        if (targetProp != null) targetProp.objectReferenceValue = player.transform;
        so.ApplyModifiedProperties();
    }

    // ── 씬 저장 표시 ──────────────────────────────────────

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
        tmp.color = Color.black;
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        return tmp;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 pos)
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
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;

        return btn;
    }
}
