using UnityEngine;
using TMPro;

/// <summary>
/// EndingScene에서 엔딩 타입에 맞는 연출 표시
/// </summary>
public class EndingController : MonoBehaviour
{
    [Header("엔딩별 오브젝트/텍스트")]
    [SerializeField] private GameObject bestEndingUI;
    [SerializeField] private GameObject normalEndingUI;
    [SerializeField] private GameObject badEndingUI;

    [Header("점수 표시")]
    [SerializeField] private TextMeshProUGUI totalScoreText;

    private void Start()
    {
        var endingType = GameManager.Instance?.GetEndingType() ?? EndingType.Normal;
        int totalScore = GameManager.Instance?.TotalEfficiencyScore ?? 0;

        if (totalScoreText != null)
            totalScoreText.text = $"최종 효율 점수: {totalScore}";

        bestEndingUI?.SetActive(endingType == EndingType.Best);
        normalEndingUI?.SetActive(endingType == EndingType.Normal);
        badEndingUI?.SetActive(endingType == EndingType.Bad);

        Debug.Log($"[EndingController] 엔딩: {endingType} / 점수: {totalScore}");
    }

    public void OnReturnToMainMenuClicked()
    {
        GameManager.Instance?.LoadMainMenu();
    }
}
