using UnityEngine;

/// <summary>
/// 플레이 페이즈 흐름 관리
/// - 플레이어 조작 활성화
/// - 목적지 도달 시 효율 점수 계산 후 음악 감상 씬으로
/// - 낙사 시 처리
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
            Debug.LogError("[PlayPhaseManager] PlayerController 미연결");
            return;
        }

        // 플레이어를 시작 위치로 이동
        if (playerStartPosition != null)
            player.transform.position = playerStartPosition.position;

        player.EnableControl();
        UIManager.Instance?.ShowPlayPhaseUI();

        Debug.Log("[PlayPhaseManager] 플레이 페이즈 시작");
    }

    public void OnPlayerReachedGoal()
    {
        Debug.Log("[PlayPhaseManager] 목적지 도달!");

        int efficiencyScore = EfficiencyScoreManager.Instance?.CalculateScore() ?? 0;
        int usageCount = BridgeBuilder.Instance?.TotalUsageCount ?? 0;
        UIManager.Instance?.UpdateEfficiencyScore(efficiencyScore);

        // 녹음된 음악을 로컬에 자동 저장 (다른 세션/다른 플레이어와 공유 가능)
        var stage = StageManager.Instance?.CurrentStage;
        if (stage != null && AudioManager.Instance != null &&
            AudioManager.Instance.CurrentRecording.Count > 0)
        {
            var rec = AudioManager.Instance.BuildRecording(
                stage.stageIndex, stage.stageName, efficiencyScore, usageCount);
            MusicSaveSystem.Save(rec);
        }

        UIManager.Instance?.ShowMusicReviewUI();
        AudioManager.Instance?.PlayRecordedMusic();

        // StageResultUI에서 Next/Retry 버튼을 통해 GameManager.OnStageClear 호출
        // 여기서 직접 호출하면 이중 진행이 발생하므로 제거
        var resultUI = Object.FindObjectOfType<StageResultUI>();
        if (resultUI != null)
            resultUI.Show(efficiencyScore, usageCount);
        else
            GameManager.Instance?.OnStageClear(efficiencyScore);
    }

    public void OnPlayerFell()
    {
        Debug.Log("[PlayPhaseManager] 낙사! 빌드 페이즈 재시작");
        // 다리 초기화 후 빌드 페이즈 재도전
        BridgeBuilder.Instance?.ClearAll();
        GameManager.Instance?.StartBuildPhase();
    }
}
