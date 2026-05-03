using UnityEngine;

/// <summary>
/// 빌드 페이즈 흐름 관리
/// [FIX] 리스너 중복 등록 방지 (RemoveListener → AddListener)
/// [FIX] CameraFollow.SetOverviewMode() 호출
/// [추가] StageProgressUI 연동 — 악기 선택 시 HUD 업데이트
/// [추가] BGMManager.PlayBGMForCurrentPhase() — 빌드 시작 시 호출
/// </summary>
public class BuildPhaseManager : MonoBehaviour
{
    public static BuildPhaseManager Instance { get; private set; }

    private InstrumentData         selectedInstrument;
    private InstrumentSidebarButton currentSelectedButton;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartBuild()
    {
        var stage = StageManager.Instance?.CurrentStage;
        if (stage == null) { Debug.LogError("[BuildPhaseManager] CurrentStage == null"); return; }

        StageBuilder.Instance?.BuildFromData(stage);

        var line = RhythmLineController.Instance;
        if (line != null)
        {
            line.OnLineReachedEnd.RemoveListener(OnLineReachedEnd);
            line.Initialize(stage.rhythmLineSpeed, stage.stageWidth);
            line.OnLineReachedEnd.AddListener(OnLineReachedEnd);
            line.StartMoving();
        }

        Camera.main?.GetComponent<CameraFollow>()?.SetOverviewMode();

        UIManager.Instance?.ShowBuildPhaseUI();
        UIManager.Instance?.RefreshInstrumentSidebar();
        AudioManager.Instance?.StartRecording();

        // 기본 악기 선택
        var instruments = StageManager.Instance?.OwnedInstruments;
        if (instruments != null && instruments.Count > 0)
        {
            selectedInstrument = instruments[0];
            StageProgressUI.Instance?.SetSelectedInstrument(selectedInstrument.instrumentName);
        }

        // [추가] 스테이지 정보 HUD 갱신
        StageProgressUI.Instance?.RefreshStageInfo();

        // [추가] Build BGM
        BGMManager.Instance?.PlayBGMForCurrentPhase();

        Debug.Log($"[BuildPhaseManager] Build Phase — Stage {stage.stageIndex + 1}");
    }

    private void Update()
    {
        if (GameManager.Instance?.CurrentPhase != GamePhase.Build) return;
        HandleInstrumentSelection();
        HandleBridgePlacement();
    }

    private void HandleInstrumentSelection()
    {
        var instruments = StageManager.Instance?.OwnedInstruments;
        if (instruments == null) return;
        foreach (var inst in instruments)
        {
            if (Input.GetKeyDown(inst.activationKey))
                SelectInstrument(inst);
        }
    }

    private void HandleBridgePlacement()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            PlaceBridgeWithSelected();
    }

    public void PlaceBridgeWithSelected()
    {
        if (selectedInstrument == null)
        {
            Debug.LogWarning("[BuildPhaseManager] 선택된 악기 없음");
            return;
        }
        BridgeBuilder.Instance?.PlaceBridge(selectedInstrument);
        RhythmLineController.Instance?.GetComponent<RhythmLineVisual>()?.PlayJudgementFlash();
    }

    public void SelectInstrument(InstrumentData instrument, InstrumentSidebarButton button = null)
    {
        selectedInstrument = instrument;
        currentSelectedButton?.SetSelected(false);
        currentSelectedButton = button;
        currentSelectedButton?.SetSelected(true);

        // [추가] HUD 업데이트
        if (instrument != null)
            StageProgressUI.Instance?.SetSelectedInstrument(instrument.instrumentName);

        Debug.Log($"[BuildPhaseManager] 악기 선택: {instrument?.instrumentName}");
    }

    private void OnLineReachedEnd()
    {
        RhythmLineController.Instance?.OnLineReachedEnd.RemoveListener(OnLineReachedEnd);
        Debug.Log("[BuildPhaseManager] 판정선 종료 → Play Phase");
        GameManager.Instance?.StartPlayPhase();
    }
}
