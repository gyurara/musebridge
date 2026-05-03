using UnityEngine;

/// <summary>
/// 플레이 페이즈 흐름 관리
/// [FIX] OnPlayerFell → BGMManager.PlaySFXFall() 연결
/// [FIX] OnPlayerReachedGoal → BGMManager.PlaySFXClear() 연결
/// [FIX] StartPlay → BGMManager 페이즈 전환 알림
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

        var cam = Camera.main?.GetComponent<CameraFollow>();
        if (cam != null) cam.SetFollowMode(player.transform);

        // [FIX] 플레이 BGM 전환
        BGMManager.Instance?.PlayBGMForCurrentPhase();

        Debug.Log("[PlayPhaseManager] 플레이 페이즈 시작");
    }

    public void OnPlayerReachedGoal()
    {
        Debug.Log("[PlayPhaseManager] 목적지 도달!");
        player?.DisableControl();

        // [FIX] 클리어 SFX
        BGMManager.Instance?.PlaySFXClear();

        int efficiencyScore = EfficiencyScoreManager.Instance?.CalculateScore() ?? 0;
        int usageCount      = BridgeBuilder.Instance?.TotalUsageCount ?? 0;
        UIManager.Instance?.UpdateEfficiencyScore(efficiencyScore);

        // 음악 저장
        var stage = StageManager.Instance?.CurrentStage;
        if (stage != null && AudioManager.Instance != null
            && AudioManager.Instance.CurrentRecording.Count > 0)
        {
            var rec = AudioManager.Instance.BuildRecording(
                stage.stageIndex, stage.stageName, efficiencyScore, usageCount);
            MusicSaveSystem.Save(rec);
        }

        // 결과 UI
        var resultUI = Object.FindObjectOfType<StageResultUI>();
        if (resultUI != null)
            resultUI.Show(efficiencyScore, usageCount);
        else
        {
            UIManager.Instance?.ShowMusicReviewUI();
            AudioManager.Instance?.PlayRecordedMusic();
            GameManager.Instance?.OnStageClear(efficiencyScore);
        }
    }

    public void OnPlayerFell()
    {
        Debug.Log("[PlayPhaseManager] 낙사!");
        player?.DisableControl();

        // [FIX] 낙사 SFX
        BGMManager.Instance?.PlaySFXFall();

        BridgeBuilder.Instance?.ClearAll();
        GameManager.Instance?.StartBuildPhase();
    }
}
