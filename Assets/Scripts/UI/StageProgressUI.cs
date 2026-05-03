using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// [신규] 스테이지 진행 HUD
/// - 현재 스테이지 번호
/// - 남은 다리 조각 예산 (maxBridgePieces - 사용수)
/// - 실시간 효율 게이지 (S/A/B/C/D 색상)
/// - 판정선 진행도 바
///
/// GameUICanvas 하위에 배치
/// </summary>
public class StageProgressUI : MonoBehaviour
{
    public static StageProgressUI Instance { get; private set; }

    [Header("스테이지 정보")]
    [SerializeField] private TextMeshProUGUI stageLabel;      // "Stage 3 / 15"
    [SerializeField] private TextMeshProUGUI stageName;       // "Wind of Change"
    [SerializeField] private TextMeshProUGUI budgetText;      // "남은 조각: 18"
    [SerializeField] private TextMeshProUGUI selectedInstName;// 현재 선택 악기

    [Header("게이지들")]
    [SerializeField] private Slider efficiencySlider;  // 0~1 실시간 효율
    [SerializeField] private Image  efficiencyFill;    // 색상 변경용
    [SerializeField] private Slider rhythmLineSlider;  // 판정선 진행도

    [Header("등급 텍스트")]
    [SerializeField] private TextMeshProUGUI gradeText;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (GameManager.Instance?.CurrentPhase != GamePhase.Build
         && GameManager.Instance?.CurrentPhase != GamePhase.Play)
            return;

        RefreshBudget();
        RefreshEfficiency();
        RefreshRhythmProgress();
    }

    public void RefreshStageInfo()
    {
        var stage = StageManager.Instance?.CurrentStage;
        var gm    = GameManager.Instance;
        if (stage == null || gm == null) return;

        if (stageLabel != null)
            stageLabel.text = $"Stage {stage.stageIndex + 1} / {gm.TotalStages}";
        if (stageName != null)
            stageName.text = stage.stageName;
    }

    private void RefreshBudget()
    {
        var stage = StageManager.Instance?.CurrentStage;
        if (stage == null) return;

        int used      = BridgeBuilder.Instance?.TotalUsageCount ?? 0;
        int remaining = Mathf.Max(0, stage.maxBridgePieces - used);

        if (budgetText != null)
        {
            budgetText.text = $"조각 남음: {remaining}";
            budgetText.color = remaining <= 5
                ? new Color(1f, 0.4f, 0.3f)  // 위험: 빨강
                : remaining <= stage.perfectThreshold + 3
                    ? new Color(1f, 0.85f, 0.2f) // 주의: 노랑
                    : Color.white;
        }
    }

    private void RefreshEfficiency()
    {
        int score = EfficiencyScoreManager.Instance?.CalculateScore() ?? 0;
        string grade = EfficiencyScoreManager.Instance?.GetGrade(score) ?? "D";

        if (efficiencySlider != null)
            efficiencySlider.value = score / 100f;

        if (efficiencyFill != null)
        {
            efficiencyFill.color = grade switch
            {
                "S" => new Color(1f, 0.84f, 0f),
                "A" => new Color(0.5f, 0.8f, 1f),
                "B" => new Color(0.5f, 1f, 0.5f),
                "C" => new Color(1f, 0.75f, 0.3f),
                _   => new Color(0.6f, 0.6f, 0.6f),
            };
        }

        if (gradeText != null)
        {
            gradeText.text  = grade;
            gradeText.color = efficiencyFill?.color ?? Color.white;
        }
    }

    private void RefreshRhythmProgress()
    {
        var line = RhythmLineController.Instance;
        var stage = StageManager.Instance?.CurrentStage;
        if (line == null || stage == null || rhythmLineSlider == null) return;

        float startX = -stage.stageWidth / 2f;
        float endX   =  stage.stageWidth / 2f;
        float range  = endX - startX;
        if (range <= 0) return;

        float progress = Mathf.Clamp01((line.CurrentX - startX) / range);
        rhythmLineSlider.value = progress;
    }

    public void SetSelectedInstrument(string instName)
    {
        if (selectedInstName != null)
            selectedInstName.text = $"♪ {instName}";
    }
}
