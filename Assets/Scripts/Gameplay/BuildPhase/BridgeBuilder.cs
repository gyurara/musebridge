using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 다리 생성 로직 담당
/// 판정선 위치에 선택된 악기에 맞는 다리 조각을 생성
/// </summary>
public class BridgeBuilder : MonoBehaviour
{
    public static BridgeBuilder Instance { get; private set; }

    [SerializeField] private GameObject bridgePiecePrefab;
    [SerializeField] private float bridgeYPosition = 0f; // 다리가 생성될 Y 좌표

    // 생성된 다리 조각 목록 (플레이 페이즈에서 사용)
    private readonly List<BridgePiece> builtBridgePieces = new();
    public IReadOnlyList<BridgePiece> BuiltBridgePieces => builtBridgePieces;

    // 악기 사용 횟수 기록 (효율 점수 계산용)
    private readonly Dictionary<string, int> instrumentUsageCount = new();
    public int TotalUsageCount { get; private set; } = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ClearAll()
    {
        foreach (var piece in builtBridgePieces)
        {
            if (piece != null) Destroy(piece.gameObject);
        }
        builtBridgePieces.Clear();
        instrumentUsageCount.Clear();
        TotalUsageCount = 0;
    }

    /// <summary>
    /// 현재 판정선 위치에 다리 조각 생성 + 음표 재생
    /// </summary>
    public void PlaceBridge(InstrumentData instrument)
    {
        if (instrument == null || bridgePiecePrefab == null) return;

        var rhythmLine = RhythmLineController.Instance;
        if (rhythmLine == null || !rhythmLine.IsMoving) return;

        // 판정선 X 위치에 다리 생성
        Vector3 spawnPos = new Vector3(rhythmLine.CurrentX, bridgeYPosition, 0);

        var pieceObj = Instantiate(bridgePiecePrefab, spawnPos, Quaternion.identity);
        var piece = pieceObj.GetComponent<BridgePiece>();

        if (piece == null)
        {
            Debug.LogWarning("[BridgeBuilder] BridgePiece 컴포넌트 없음");
            Destroy(pieceObj);
            return;
        }

        piece.Initialize(instrument, spawnPos);
        builtBridgePieces.Add(piece);

        // 악기별 사용 횟수 기록
        if (!instrumentUsageCount.ContainsKey(instrument.instrumentName))
            instrumentUsageCount[instrument.instrumentName] = 0;
        instrumentUsageCount[instrument.instrumentName]++;
        TotalUsageCount++;

        // 음표 재생 + 녹음
        PlayNoteForInstrument(instrument);

        Debug.Log($"[BridgeBuilder] 다리 생성: {instrument.instrumentName} at {spawnPos}");
    }

    private void PlayNoteForInstrument(InstrumentData instrument)
    {
        if (instrument.noteClips == null || instrument.noteClips.Length == 0) return;

        int noteIndex = (TotalUsageCount - 1) % instrument.noteClips.Length;
        AudioManager.Instance?.PlayAndRecord(instrument, noteIndex, instrument.volume);
    }

    public Dictionary<string, int> GetUsageCounts() => new(instrumentUsageCount);
}
