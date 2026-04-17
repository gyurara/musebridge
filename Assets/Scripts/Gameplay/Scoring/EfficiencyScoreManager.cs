using UnityEngine;

/// <summary>
/// 효율 점수 계산
/// 악기 사용 횟수가 적을수록 높은 점수
/// StageData에 정의된 기준값(threshold)에 따라 등급 결정
/// </summary>
public class EfficiencyScoreManager : MonoBehaviour
{
    public static EfficiencyScoreManager Instance { get; private set; }

    // 점수 범위: 0 ~ 100
    private const int MaxScore = 100;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// 현재 스테이지의 다리 사용 횟수를 기반으로 효율 점수 계산
    /// </summary>
    public int CalculateScore()
    {
        var stage = StageManager.Instance?.CurrentStage;
        if (stage == null) return 0;

        int usageCount = BridgeBuilder.Instance?.TotalUsageCount ?? 0;

        // perfectThreshold 이하 → 100점, maxBridgePieces 이상 → 0점
        int minUsage = stage.perfectThreshold;
        int maxUsage = stage.maxBridgePieces;

        if (usageCount <= minUsage) return MaxScore;
        if (usageCount >= maxUsage) return 0;

        // 선형 보간으로 점수 계산
        float ratio = 1f - (float)(usageCount - minUsage) / (maxUsage - minUsage);
        int score = Mathf.RoundToInt(ratio * MaxScore);

        Debug.Log($"[EfficiencyScore] 사용 횟수: {usageCount} / 점수: {score}");
        return score;
    }

    /// <summary>
    /// 점수 기반 등급 반환
    /// </summary>
    public string GetGrade(int score)
    {
        if (score >= 90) return "S";
        if (score >= 70) return "A";
        if (score >= 50) return "B";
        if (score >= 30) return "C";
        return "D";
    }
}
