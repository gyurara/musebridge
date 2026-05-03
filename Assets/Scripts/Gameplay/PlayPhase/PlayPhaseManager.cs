using UnityEngine;

/// <summary>
/// 플레이 페이즈 흐름 관리
/// [FIX] OnPlayerReachedGoal에서 AudioManager.PlayRecordedMusic() 직접 호출 제거
///        → StageResultUI.ShowSequence()에서 delay 후 재생하므로 여기서 호출하면 두 번 재생됨
/// [FIX] UIManager.ShowMusicReviewUI() 호출도 StageResultUI 없는 폴백 시에만 수행
/// </summary>
public class PlayPhaseManager : MonoBehaviour
{
    public static PlayPhaseManager Instance { get; private set; }

    [SerializeField] private PlayerController player;
    [SerializeField] private Transform playerStartPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartPlay()
    {
        if (player == null)
        {
            // 런타임에 Player 태그로 재탐색 (SceneSetup 후 변경됐을 경우 대비)
            var found = GameObject.FindWithTag("Player");
            if (found != null) player = found.GetComponent<PlayerController>();
        }

        if (player == null)
        {
            Debug.LogError("[PlayPhaseManager] PlayerController 미연결");
            return;
        }

        if (playerStartPosition != null)
            player.transform.position = playerStartPosition.position;

        player.EnableControl();
        UIManager.Instance?.ShowPlayPhaseUI();

        // [NOTE] 카메라 팔로우 활성화
        var cam = Camera.main?.GetComponent<CameraFollow>();
        if (cam != null) cam.SetFollowMode(player.transform);

        Debug.Log("[PlayPhaseManager] 플레이 페이즈 시작");
    }

    public void OnPlayerReachedGoal()
    {
        Debug.Log("[PlayPhaseManager] 목적지 도달!");

        // [FIX] player가 null일 경우 대비
        player?.DisableControl();

        int efficiencyScore = EfficiencyScoreManager.Instance?.CalculateScore() ?? 0;
        int usageCount = BridgeBuilder.Instance?.TotalUsageCount ?? 0;
        UIManager.Instance?.UpdateEfficiencyScore(efficiencyScore);

        // 녹음된 음악 저장
        var stage = StageManager.Instance?.CurrentStage;
        if (stage != null && AudioManager.Instance != null
            && AudioManager.Instance.CurrentRecording.Count > 0)
        {
            var rec = AudioManager.Instance.BuildRecording(
                stage.stageIndex, stage.stageName, efficiencyScore, usageCount);
            MusicSaveSystem.Save(rec);
        }

        // StageResultUI를 통해 결과 표시 + 음악 재생 (UI 내부에서 처리)
        var resultUI = Object.FindObjectOfType<StageResultUI>();
        if (resultUI != null)
        {
            // [FIX] PlayRecordedMusic은 StageResultUI.ShowSequence에서 delay 후 재생
            // 여기서 직접 AudioManager.PlayRecordedMusic() 호출하지 않음
            resultUI.Show(efficiencyScore, usageCount);
        }
        else
        {
            // 폴백: StageResultUI 없으면 바로 음악 감상 + 스테이지 진행
            UIManager.Instance?.ShowMusicReviewUI();
            AudioManager.Instance?.PlayRecordedMusic();
            GameManager.Instance?.OnStageClear(efficiencyScore);
        }
    }

    public void OnPlayerFell()
    {
        Debug.Log("[PlayPhaseManager] 낙사! 빌드 페이즈 재시작");
        player?.DisableControl();
        BridgeBuilder.Instance?.ClearAll();
        GameManager.Instance?.StartBuildPhase();
    }
}
