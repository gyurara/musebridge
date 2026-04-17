using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 스테이지 클리어 후 결과 화면 (음악 감상 + 점수 표시)
/// PlayPhaseManager.OnPlayerReachedGoal() 이후 활성화
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
    [SerializeField] private float musicReviewDelay = 1f; // 결과 패널 뜨고 음악 재생 전 대기

    private void Start()
    {
        nextStageButton?.onClick.AddListener(OnNextStageClicked);
        retryButton?.onClick.AddListener(OnRetryClicked);

        if (resultPanel != null) resultPanel.SetActive(false);
    }

    /// <summary>
    /// 결과 화면 표시 (PlayPhaseManager에서 호출)
    /// </summary>
    public void Show(int efficiencyScore, int usageCount)
    {
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
        }

        // 스테이지 정보 표시
        var stage = StageManager.Instance?.CurrentStage;
        if (stageNameText != null)
            stageNameText.text = stage != null ? $"Stage {stage.stageIndex + 1}: {stage.stageName}" : "Stage Clear!";

        if (scoreText != null) scoreText.text = $"효율 점수: {score}";
        if (usageCountText != null) usageCountText.text = $"악기 사용 횟수: {usageCount}";

        string grade = EfficiencyScoreManager.Instance?.GetGrade(score) ?? "-";
        if (gradeText != null)
        {
            gradeText.text = grade;
            gradeText.color = GetGradeColor(grade);
        }

        // 음악 감상 대기 후 재생
        if (musicReviewLabel != null) musicReviewLabel.text = "잠시 후 당신이 만든 음악이 재생됩니다...";
        yield return new WaitForSeconds(musicReviewDelay);

        if (musicReviewLabel != null) musicReviewLabel.text = "♪ 당신이 만든 음악 ♪";
        AudioManager.Instance?.PlayRecordedMusic();
    }

    private Color GetGradeColor(string grade) => grade switch
    {
        "S" => new Color(1f, 0.84f, 0f),   // 금색
        "A" => new Color(0.5f, 0.8f, 1f),  // 하늘색
        "B" => new Color(0.5f, 1f, 0.5f),  // 연두
        "C" => new Color(1f, 0.75f, 0.3f), // 주황
        _ => Color.gray
    };

    private void OnNextStageClicked()
    {
        int score = EfficiencyScoreManager.Instance?.CalculateScore() ?? 0;
        GameManager.Instance?.OnStageClear(score);
    }

    private void OnRetryClicked()
    {
        BridgeBuilder.Instance?.ClearAll();
        GameManager.Instance?.StartBuildPhase();
    }
}
