using UnityEngine;

/// <summary>
/// 빌드 페이즈 전체 흐름 관리
/// [FIX] StartBuild 시 CameraFollow.SetOverviewMode() 호출 추가
/// [FIX] OnLineReachedEnd 리스너 중복 등록 방지 (Remove → Add)
/// </summary>
public class BuildPhaseManager : MonoBehaviour
{
    public static BuildPhaseManager Instance { get; private set; }

    private InstrumentData selectedInstrument;
    private InstrumentSidebarButton currentSelectedButton;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartBuild()
    {
        var stage = StageManager.Instance?.CurrentStage;
        if (stage == null) { Debug.LogError("[BuildPhaseManager] 현재 스테이지 없음"); return; }

        // 스테이지 지형/장애물/픽업 재생성
        StageBuilder.Instance?.BuildFromData(stage);

        // [FIX] 리스너 중복 방지: Remove 먼저
        var rhythmLine = RhythmLineController.Instance;
        if (rhythmLine != null)
        {
            rhythmLine.OnLineReachedEnd.RemoveListener(OnLineReachedEnd);
            rhythmLine.Initialize(stage.rhythmLineSpeed, stage.stageWidth);
            rhythmLine.OnLineReachedEnd.AddListener(OnLineReachedEnd);
            rhythmLine.StartMoving();
        }

        // [FIX] 빌드 페이즈 시작 시 카메라를 전체뷰로
        Camera.main?.GetComponent<CameraFollow>()?.SetOverviewMode();

        UIManager.Instance?.ShowBuildPhaseUI();
        UIManager.Instance?.RefreshInstrumentSidebar();

        AudioManager.Instance?.StartRecording();

        // 기본 악기 선택
        var instruments = StageManager.Instance?.OwnedInstruments;
        if (instruments != null && instruments.Count > 0)
            selectedInstrument = instruments[0];

        Debug.Log($"[BuildPhaseManager] 빌드 페이즈 시작 — Stage {stage.stageIndex + 1}");
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
        foreach (var instrument in instruments)
        {
            if (Input.GetKeyDown(instrument.activationKey))
            {
                selectedInstrument = instrument;
                Debug.Log($"[BuildPhaseManager] 악기 선택: {instrument.instrumentName}");
            }
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
        Debug.Log($"[BuildPhaseManager] 악기 선택: {instrument?.instrumentName}");
    }

    private void OnLineReachedEnd()
    {
        RhythmLineController.Instance?.OnLineReachedEnd.RemoveListener(OnLineReachedEnd);
        Debug.Log("[BuildPhaseManager] 판정선 종료 → 플레이 페이즈 전환");
        GameManager.Instance?.StartPlayPhase();
    }
}
