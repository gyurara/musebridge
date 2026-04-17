using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI 전반 관리 (사이드바 악기 목록, 페이즈 UI, 점수 표시 등)
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("빌드 페이즈 UI")]
    [SerializeField] private GameObject buildPhasePanel;
    [SerializeField] private Transform instrumentSidebarParent; // 악기 버튼 나열될 부모
    [SerializeField] private GameObject instrumentButtonPrefab;  // 악기 버튼 프리팹

    [Header("플레이 페이즈 UI")]
    [SerializeField] private GameObject playPhasePanel;

    [Header("음악 감상 UI")]
    [SerializeField] private GameObject musicReviewPanel;
    [SerializeField] private TextMeshProUGUI musicReviewText;

    [Header("효율 점수 UI")]
    [SerializeField] private TextMeshProUGUI efficiencyScoreText;

    private readonly List<GameObject> instrumentButtons = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── 페이즈별 패널 전환 ──────────────────────────────

    public void ShowBuildPhaseUI()
    {
        buildPhasePanel?.SetActive(true);
        playPhasePanel?.SetActive(false);
        musicReviewPanel?.SetActive(false);
    }

    public void ShowPlayPhaseUI()
    {
        buildPhasePanel?.SetActive(false);
        playPhasePanel?.SetActive(true);
        musicReviewPanel?.SetActive(false);
    }

    public void ShowMusicReviewUI(string message = "당신이 만든 음악을 감상하세요!")
    {
        buildPhasePanel?.SetActive(false);
        playPhasePanel?.SetActive(false);
        musicReviewPanel?.SetActive(true);
        if (musicReviewText != null) musicReviewText.text = message;
    }

    // ── 사이드바 악기 버튼 갱신 ─────────────────────────

    /// <summary>
    /// 보유 악기 목록을 읽어 사이드바 버튼 재생성
    /// </summary>
    public void RefreshInstrumentSidebar()
    {
        // 기존 버튼 제거
        foreach (var btn in instrumentButtons) Destroy(btn);
        instrumentButtons.Clear();

        var instruments = StageManager.Instance?.OwnedInstruments;
        if (instruments == null || instrumentSidebarParent == null) return;

        foreach (var instrument in instruments)
        {
            var btnObj = Instantiate(instrumentButtonPrefab, instrumentSidebarParent);
            instrumentButtons.Add(btnObj);

            // InstrumentSidebarButton 컴포넌트로 초기화
            var sidebarBtn = btnObj.GetComponent<InstrumentSidebarButton>();
            if (sidebarBtn != null)
            {
                sidebarBtn.Setup(instrument);
            }
            else
            {
                // 폴백: 텍스트 직접 설정
                var label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.text = $"{instrument.instrumentName}\n[{instrument.activationKey}]";
            }
        }
    }

    // ── 점수 표시 ─────────────────────────────────────────

    public void UpdateEfficiencyScore(int score)
    {
        if (efficiencyScoreText != null)
            efficiencyScoreText.text = $"효율 점수: {score}";
    }
}
