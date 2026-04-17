using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 전체 게임 흐름을 관리하는 싱글톤 매니저
/// 게임 상태(페이즈) 전환 및 씬 로드 담당
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GamePhase CurrentPhase { get; private set; } = GamePhase.MainMenu;

    // 전체 스테이지 클리어 후 효율 점수 합산용
    public int TotalEfficiencyScore { get; private set; } = 0;
    public int CurrentStageIndex { get; private set; } = 0;
    public int TotalStages = 15; // 전체 스테이지 수

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── 페이즈 전환 ──────────────────────────────────────

    public void StartBuildPhase()
    {
        CurrentPhase = GamePhase.Build;
        Debug.Log($"[GameManager] 빌드 페이즈 시작 - Stage {CurrentStageIndex}");
        // BuildPhaseManager가 씬에 있다면 이벤트로 알림
        BuildPhaseManager.Instance?.StartBuild();
    }

    public void StartPlayPhase()
    {
        CurrentPhase = GamePhase.Play;
        Debug.Log("[GameManager] 플레이 페이즈 시작");
        PlayPhaseManager.Instance?.StartPlay();
    }

    public void StartMusicReview()
    {
        CurrentPhase = GamePhase.MusicReview;
        Debug.Log("[GameManager] 음악 감상 시작");
        AudioManager.Instance?.PlayRecordedMusic();
    }

    // ── 스테이지 클리어 / 다음 스테이지 ─────────────────

    public void OnStageClear(int stageEfficiencyScore)
    {
        TotalEfficiencyScore += stageEfficiencyScore;
        Debug.Log($"[GameManager] 스테이지 클리어! 효율점수: {stageEfficiencyScore} / 누적: {TotalEfficiencyScore}");

        CurrentStageIndex++;
        if (CurrentStageIndex >= TotalStages)
        {
            LoadEnding();
        }
        else
        {
            SceneManager.LoadScene("GameScene");
        }
    }

    public void LoadEnding()
    {
        CurrentPhase = GamePhase.Ending;
        SceneManager.LoadScene("EndingScene");
    }

    public void LoadMainMenu()
    {
        CurrentPhase = GamePhase.MainMenu;
        TotalEfficiencyScore = 0;
        CurrentStageIndex = 0;
        SceneManager.LoadScene("MainMenu");
    }

    // ── 엔딩 분기 ─────────────────────────────────────────

    /// <summary>
    /// 누적 효율점수에 따라 엔딩 타입 반환
    /// 점수가 높을수록 좋은 엔딩
    /// </summary>
    public EndingType GetEndingType()
    {
        // 15스테이지 × 100점 = 1500점 만점 기준
        float avgScore = TotalStages > 0 ? (float)TotalEfficiencyScore / TotalStages : 0;
        if (avgScore >= 75) return EndingType.Best;
        if (avgScore >= 45) return EndingType.Normal;
        return EndingType.Bad;
    }
}

public enum GamePhase
{
    MainMenu,
    Build,       // 리듬으로 다리 만드는 페이즈
    Play,        // 캐릭터가 다리를 건너는 페이즈
    MusicReview, // 만든 음악 감상
    Ending
}

public enum EndingType
{
    Best,
    Normal,
    Bad
}
