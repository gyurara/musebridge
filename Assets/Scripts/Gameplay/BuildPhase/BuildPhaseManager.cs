using UnityEngine;

/// <summary>
/// 빌드 페이즈 전체 흐름 관리
/// - 판정선 시작
/// - 키 입력 감지 → 악기 선택 + 다리 생성
/// - 판정선 끝 도달 시 플레이 페이즈로 전환
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
        if (stage == null) return;

        // 스테이지 데이터 기반 레벨(지형/장애물/픽업) 재생성
        StageBuilder.Instance?.BuildFromData(stage);

        // 판정선 초기화 및 시작
        RhythmLineController.Instance?.Initialize(stage.rhythmLineSpeed, stage.stageWidth);
        RhythmLineController.Instance?.OnLineReachedEnd.AddListener(OnLineReachedEnd);
        RhythmLineController.Instance?.StartMoving();

        // 사이드바 UI 갱신
        UIManager.Instance?.ShowBuildPhaseUI();
        UIManager.Instance?.RefreshInstrumentSidebar();

        // 음악 녹음 시작
        AudioManager.Instance?.StartRecording();

        // 기본 악기 선택 (첫 번째 악기)
        var instruments = StageManager.Instance?.OwnedInstruments;
        if (instruments != null && instruments.Count > 0)
            selectedInstrument = instruments[0];

        Debug.Log("[BuildPhaseManager] 빌드 페이즈 시작");
    }

    private void Update()
    {
        if (GameManager.Instance?.CurrentPhase != GamePhase.Build) return;

        HandleInstrumentSelection();
        HandleBridgePlacement();
    }

    /// <summary>
    /// 악기 선택 키 감지 (각 악기에 지정된 activationKey)
    /// </summary>
    private void HandleInstrumentSelection()
    {
        var instruments = StageManager.Instance?.OwnedInstruments;
        if (instruments == null) return;

        foreach (var instrument in instruments)
        {
            // activationKey를 누르면 해당 악기 선택
            if (Input.GetKeyDown(instrument.activationKey))
            {
                selectedInstrument = instrument;
                Debug.Log($"[BuildPhaseManager] 악기 선택: {instrument.instrumentName}");
            }
        }
    }

    /// <summary>
    /// 스페이스바 = 현재 선택된 악기로 다리 생성
    /// (사이드바 버튼 클릭으로도 호출 가능 - UI 버튼에서 PlaceBridgeWithSelected 연결)
    /// </summary>
    private void HandleBridgePlacement()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlaceBridgeWithSelected();
        }
    }

    /// <summary>
    /// UI 버튼 또는 키 입력으로 다리 생성 호출
    /// </summary>
    public void PlaceBridgeWithSelected()
    {
        if (selectedInstrument == null)
        {
            Debug.LogWarning("[BuildPhaseManager] 선택된 악기 없음");
            return;
        }
        BridgeBuilder.Instance?.PlaceBridge(selectedInstrument);
        // 판정선 플래시 효과
        RhythmLineController.Instance?.GetComponent<RhythmLineVisual>()?.PlayJudgementFlash();
    }

    /// <summary>
    /// 사이드바 버튼에서 호출 — 악기 선택 + 버튼 하이라이트
    /// </summary>
    public void SelectInstrument(InstrumentData instrument, InstrumentSidebarButton button = null)
    {
        selectedInstrument = instrument;
        currentSelectedButton?.SetSelected(false);
        currentSelectedButton = button;
        currentSelectedButton?.SetSelected(true);
        Debug.Log($"[BuildPhaseManager] 악기 선택: {instrument.instrumentName}");
    }

    private void OnLineReachedEnd()
    {
        RhythmLineController.Instance?.OnLineReachedEnd.RemoveListener(OnLineReachedEnd);
        Debug.Log("[BuildPhaseManager] 판정선 종료 → 플레이 페이즈 전환");
        GameManager.Instance?.StartPlayPhase();
    }
}
