using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 전체 게임 흐름 싱글톤
/// [FIX] OnStageClear → BGMManager 페이즈 전환 알림
/// [FIX] LoadMainMenu → StageManager.ResetPersistedInstruments() 호출
///        (DontDestroyOnLoad인 StageManager가 악기 목록을 유지하던 문제)
/// [FIX] StartBuildPhase/StartPlayPhase → BGMManager 알림
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GamePhase CurrentPhase      { get; private set; } = GamePhase.MainMenu;
    public int TotalEfficiencyScore    { get; private set; } = 0;
    public int CurrentStageIndex       { get; private set; } = 0;
    public int TotalStages = 15;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── 페이즈 전환 ──────────────────────────────────────

    public void StartBuildPhase()
    {
        CurrentPhase = GamePhase.Build;
        Debug.Log($"[GameManager] Build Phase — Stage {CurrentStageIndex}");
        BuildPhaseManager.Instance?.StartBuild();
        // [FIX] BGM 전환
        BGMManager.Instance?.PlayBGMForCurrentPhase();
    }

    public void StartPlayPhase()
    {
        CurrentPhase = GamePhase.Play;
        Debug.Log("[GameManager] Play Phase");
        PlayPhaseManager.Instance?.StartPlay();
        // BGM은 PlayPhaseManager.StartPlay()에서 처리
    }

    public void StartMusicReview()
    {
        CurrentPhase = GamePhase.MusicReview;
        Debug.Log("[GameManager] Music Review");
        AudioManager.Instance?.PlayRecordedMusic();
        BGMManager.Instance?.PlayBGMForCurrentPhase();
    }

    // ── 스테이지 클리어 ───────────────────────────────────

    public void OnStageClear(int stageEfficiencyScore)
    {
        TotalEfficiencyScore += stageEfficiencyScore;
        Debug.Log($"[GameManager] Stage Clear! eff={stageEfficiencyScore} total={TotalEfficiencyScore}");

        CurrentStageIndex++;
        if (CurrentStageIndex >= TotalStages)
            LoadEnding();
        else
            SceneManager.LoadScene("GameScene");
    }

    public void LoadEnding()
    {
        CurrentPhase = GamePhase.Ending;
        SceneManager.LoadScene("EndingScene");
        // BGM은 BGMManager.OnSceneLoaded에서 처리
    }

    public void LoadMainMenu()
    {
        CurrentPhase         = GamePhase.MainMenu;
        TotalEfficiencyScore = 0;
        CurrentStageIndex    = 0;
        // [FIX] DontDestroyOnLoad된 StageManager의 악기 목록 초기화
        StageManager.Instance?.ResetPersistedInstruments();
        SceneManager.LoadScene("MainMenu");
    }

    // ── 엔딩 분기 ─────────────────────────────────────────

    public EndingType GetEndingType()
    {
        float avg = TotalStages > 0 ? (float)TotalEfficiencyScore / TotalStages : 0f;
        if (avg >= 75) return EndingType.Best;
        if (avg >= 45) return EndingType.Normal;
        return EndingType.Bad;
    }
}

public enum GamePhase { MainMenu, Build, Play, MusicReview, Ending }
public enum EndingType { Best, Normal, Bad }
