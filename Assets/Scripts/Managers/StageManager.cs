using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 스테이지 데이터 및 악기 보유 상태 관리
/// [FIX] DontDestroyOnLoad 추가 — GameScene 리로드 시 AudioManager(DontDestroyOnLoad)와
///        수명 주기가 달라 LoadOwnedInstruments 타이밍 오류가 나던 문제 해결
/// [FIX] UIManager 호출을 Start → RefreshIfReady()로 분리, null 안전 처리
/// </summary>
public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("스테이지 설정")]
    [SerializeField] private StageData[] allStages;
    [SerializeField] private InstrumentData defaultInstrument; // 시작부터 보유 (피아노)

    public StageData[] AllStages => allStages;

    public StageData CurrentStage =>
        (GameManager.Instance != null && allStages != null && allStages.Length > 0)
            ? allStages[Mathf.Clamp(GameManager.Instance.CurrentStageIndex, 0, allStages.Length - 1)]
            : null;

    private readonly List<InstrumentData> ownedInstruments = new();
    public IReadOnlyList<InstrumentData> OwnedInstruments => ownedInstruments;

    private const string PrefKey = "BridgeMelody.OwnedInstruments";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // [FIX] DontDestroyOnLoad — AudioManager와 동일하게 씬 전환 시 유지
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadOwnedInstruments();
    }

    // UIManager가 아직 생성되지 않았을 수 있으므로 별도 메서드로 분리
    public void RefreshSidebarIfReady()
    {
        UIManager.Instance?.RefreshInstrumentSidebar();
    }

    // ── 영구 저장 ────────────────────────────────────────

    private void LoadOwnedInstruments()
    {
        ownedInstruments.Clear();
        if (defaultInstrument != null && !ownedInstruments.Contains(defaultInstrument))
            ownedInstruments.Add(defaultInstrument);

        var saved = PlayerPrefs.GetString(PrefKey, "");
        if (string.IsNullOrEmpty(saved))
        {
            // UIManager가 준비된 후 갱신 — Start 타이밍엔 null일 수 있음
            UIManager.Instance?.RefreshInstrumentSidebar();
            return;
        }

        var registry = InstrumentRegistry.Load();
        if (registry == null)
        {
            UIManager.Instance?.RefreshInstrumentSidebar();
            return;
        }

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
