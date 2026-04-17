using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 스테이지 데이터 및 악기 보유 상태 관리
/// - 획득한 악기는 다음 스테이지에서도 유지 (PlayerPrefs 영구 저장)
/// - 초기 기본 악기는 defaultInstrument 로 자동 부여
/// </summary>
public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("스테이지 설정")]
    [SerializeField] private StageData[] allStages;
    [SerializeField] private InstrumentData defaultInstrument; // 시작부터 보유한 악기 (피아노)

    public StageData[] AllStages => allStages;
    public StageData CurrentStage => (GameManager.Instance != null && allStages.Length > 0)
        ? allStages[Mathf.Clamp(GameManager.Instance.CurrentStageIndex, 0, allStages.Length - 1)]
        : null;

    private readonly List<InstrumentData> ownedInstruments = new();
    public IReadOnlyList<InstrumentData> OwnedInstruments => ownedInstruments;

    private const string PrefKey = "BridgeMelody.OwnedInstruments";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        LoadOwnedInstruments();
    }

    // ── 영구 저장 ────────────────────────────────────────

    private void LoadOwnedInstruments()
    {
        ownedInstruments.Clear();
        if (defaultInstrument != null && !ownedInstruments.Contains(defaultInstrument))
            ownedInstruments.Add(defaultInstrument);

        var saved = PlayerPrefs.GetString(PrefKey, "");
        if (string.IsNullOrEmpty(saved)) { UIManager.Instance?.RefreshInstrumentSidebar(); return; }

        var registry = InstrumentRegistry.Load();
        if (registry == null) { UIManager.Instance?.RefreshInstrumentSidebar(); return; }

        foreach (var name in saved.Split('|'))
        {
            if (string.IsNullOrEmpty(name)) continue;
            var inst = registry.Find(name);
            if (inst != null && !ownedInstruments.Contains(inst))
                ownedInstruments.Add(inst);
        }

        UIManager.Instance?.RefreshInstrumentSidebar();
    }

    private void SaveOwnedInstruments()
    {
        var names = new List<string>();
        foreach (var inst in ownedInstruments)
            if (inst != null && inst != defaultInstrument) names.Add(inst.instrumentName);
        PlayerPrefs.SetString(PrefKey, string.Join("|", names));
        PlayerPrefs.Save();
    }

    /// <summary>현재 세션의 획득 기록 전체 초기화 (디버그/메인메뉴에서)</summary>
    public void ResetPersistedInstruments()
    {
        PlayerPrefs.DeleteKey(PrefKey);
        ownedInstruments.Clear();
        if (defaultInstrument != null) ownedInstruments.Add(defaultInstrument);
        UIManager.Instance?.RefreshInstrumentSidebar();
    }

    // ── 악기 수집 ────────────────────────────────────────

    public void CollectInstrument(InstrumentData instrument)
    {
        if (instrument == null) return;
        if (ownedInstruments.Contains(instrument)) return;

        ownedInstruments.Add(instrument);
        SaveOwnedInstruments();
        Debug.Log($"[StageManager] 악기 획득: {instrument.instrumentName}");
        UIManager.Instance?.RefreshInstrumentSidebar();
    }

    public bool Owns(InstrumentData instrument)
        => instrument != null && ownedInstruments.Contains(instrument);
}
