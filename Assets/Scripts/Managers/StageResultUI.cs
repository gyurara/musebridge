using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 스테이지 클리어 후 결과 화면
/// [FIX] resultPanel이 자기 자신의 자식이면 SetActive(false) → Show()에서 null 참조 발생하던 구조 수정
/// [FIX] EfficiencyScoreManager.CalculateScore()를 Show() 호출 시점 1회만 실행 (중복 계산 방지)
/// [FIX] OnNextStageClicked에서 이미 받은 score 값 사용 (재계산 안 함)
/// </summary>
public class StageResultUI : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI stageNameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI gradeText;
    [SerializeField] private TextMeshProUGUI usageCountText;
    [SerializeField] private TextMeshProUGUI musicReviewLabel;

    [Header("버튼")]
    [SerializeField] private Button nextStageButton;
    [SerializeField] private Button retryButton;

    [Header("연출")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float musicReviewDelay = 1f;

    // Show()에서 받은 값 저장 — 버튼 콜백에서 재사용
    private int cachedEfficiencyScore;
    private bool isShowing = false; // 이중 호출 방지

    private void Start()
    {
        nextStageButton?.onClick.AddListener(OnNextStageClicked);
        retryButton?.onClick.AddListener(OnRetryClicked);

        // resultPanel이 null이면 자기 자신을 패널로 사용 (폴백)
        if (resultPanel == null) resultPanel = gameObject;
        resultPanel.SetActive(false);
    }

    /// <summary>
    /// 결과 화면 표시 — PlayPhaseManager.OnPlayerReachedGoal()에서 호출
    /// </summary>
    public void Show(int efficiencyScore, int usageCount)
    {
        if (isShowing) return; // 이중 호출 방지
        isShowing = true;
        cachedEfficiencyScore = efficiencyScore;

        if (resultPanel == null) return;
        resultPanel.SetActive(true);
        StartCoroutine(ShowSequence(efficiencyScore, usageCount));
    }

    private IEnumerator ShowSequence(int score, int usageCount)
    {
        // 페이드 인
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            float t = 0f;
            while (t < fadeInDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / fadeInDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        // 스테이지 정보
        var stage = StageManager.Instance?.CurrentStage;
        if (stageNameText != null)
        {
            // [FIX] stageIndex가 0-based이므로 +1해서 표시
            int displayNum = stage != null ? stage.stageIndex + 1 : 0;
            stageNameText.text = stage != null
                ? $"Stage {displayNum}: {stage.stageName}"
                : "Stage Clear!";
        }

        if (scoreText    != null) scoreText.text    = $"효율 점수: {score}";
        if (usageCountText != null) usageCountText.text = $"악기 사용 횟수: {usageCount}";

        string grade = EfficiencyScoreManager.Instance?.GetGrade(score) ?? "-";
        if (gradeText != null)
        {
            gradeText.text  = grade;
            gradeText.color = GetGradeColor(grade);
        }

        if (musicReviewLabel != null)
            musicReviewLabel.text = "잠시 후 당신이 만든 음악이 재생됩니다...";

        yield return new WaitForSeconds(musicReviewDelay);

        if (musicReviewLabel != null)
            musicReviewLabel.text = "♪ 당신이 만든 음악 ♪";

        AudioManager.Instance?.PlayRecordedMusic();
    }

    private Color GetGradeColor(string grade) => grade switch
    {
        "S" => new Color(1f, 0.84f, 0f),
        "A" => new Color(0.5f, 0.8f, 1f),
        "B" => new Color(0.5f, 1f, 0.5f),
        "C" => new Color(1f, 0.75f, 0.3f),
        _   => Color.gray,
    };

    private void OnNextStageClicked()
    {
        isShowing = false;
        // [FIX] cachedEfficiencyScore 사용 — CalculateScore() 재호출 안 함
        GameManager.Instance?.OnStageClear(cachedEfficiencyScore);
    }

    private void OnRetryClicked()
    {
        isShowing = false;
        resultPanel?.SetActive(false);
        BridgeBuilder.Instance?.ClearAll();
        GameManager.Instance?.StartBuildPhase();
    }
}
