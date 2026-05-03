using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 다리 생성 로직
/// [FIX] PlaceBridge — 판정선 멈춘 후 키 연타로 다리 생성되던 문제 (IsMoving 체크 강화)
/// [FIX] BGMManager.PlaySFXBuild() 연결
/// [추가] GetLastPlacedPosition() — RhythmLineVisual 플래시와 연동용
/// [추가] OnBridgePlaced 이벤트 — 외부 구독용
/// </summary>
public class BridgeBuilder : MonoBehaviour
{
    public static BridgeBuilder Instance { get; private set; }

    [SerializeField] private GameObject bridgePiecePrefab;
    [SerializeField] private float      bridgeYPosition = 0f;

    private readonly List<BridgePiece>     builtBridgePieces    = new();
    private readonly Dictionary<string, int> instrumentUsageCount = new();

    public IReadOnlyList<BridgePiece> BuiltBridgePieces => builtBridgePieces;
    public int TotalUsageCount { get; private set; } = 0;

    // [추가] 외부 구독용 이벤트
    public System.Action<InstrumentData, Vector3> OnBridgePlaced;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ClearAll()
    {
        foreach (var p in builtBridgePieces)
            if (p != null) Destroy(p.gameObject);
        builtBridgePieces.Clear();
        instrumentUsageCount.Clear();
        TotalUsageCount = 0;
    }

    public void PlaceBridge(InstrumentData instrument)
    {
        if (instrument == null || bridgePiecePrefab == null) return;

        var rhythmLine = RhythmLineController.Instance;
        // [FIX] IsMoving 체크 — 판정선 끝난 후에는 생성 불가
        if (rhythmLine == null || !rhythmLine.IsMoving) return;

        Vector3 spawnPos = new Vector3(rhythmLine.CurrentX, bridgeYPosition, 0);
        var pieceObj = Instantiate(bridgePiecePrefab, spawnPos, Quaternion.identity);
        var piece    = pieceObj.GetComponent<BridgePiece>();

        if (piece == null) { Destroy(pieceObj); return; }

        piece.Initialize(instrument, spawnPos);
        builtBridgePieces.Add(piece);

        if (!instrumentUsageCount.ContainsKey(instrument.instrumentName))
            instrumentUsageCount[instrument.instrumentName] = 0;
        instrumentUsageCount[instrument.instrumentName]++;
        TotalUsageCount++;

        PlayNoteForInstrument(instrument);

        // [FIX] 다리 생성 SFX
        BGMManager.Instance?.PlaySFXBuild();

        // [추가] 이벤트 발행
        OnBridgePlaced?.Invoke(instrument, spawnPos);

        Debug.Log($"[BridgeBuilder] {instrument.instrumentName} at {spawnPos}");
    }

    private void PlayNoteForInstrument(InstrumentData instrument)
    {
        if (instrument.noteClips == null || instrument.noteClips.Length == 0) return;
        int noteIndex = (TotalUsageCount - 1) % instrument.noteClips.Length;
        AudioManager.Instance?.PlayAndRecord(instrument, noteIndex, instrument.volume);
    }

    // [추가] 마지막 생성 위치 반환
    public Vector3 GetLastPlacedPosition()
        => builtBridgePieces.Count > 0 && builtBridgePieces[^1] != null
            ? builtBridgePieces[^1].transform.position
            : Vector3.zero;

    public Dictionary<string, int> GetUsageCounts() => new(instrumentUsageCount);
}
