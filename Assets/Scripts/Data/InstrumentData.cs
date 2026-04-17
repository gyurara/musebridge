using UnityEngine;

/// <summary>
/// 악기 데이터 ScriptableObject
/// 각 악기의 특성(다리 형태, 사운드, 단축키 등)을 정의
/// </summary>
[CreateAssetMenu(fileName = "NewInstrument", menuName = "BridgeRhythm/Instrument Data")]
public class InstrumentData : ScriptableObject
{
    [Header("기본 정보")]
    public string instrumentName;
    public Sprite instrumentIcon;
    public KeyCode activationKey;

    [Header("다리 형태")]
    public BridgeShapeType shapeType;
    public float bridgeWidth = 1f;
    public float bridgeHeight = 0.2f;
    // 곡선형(기타 등)의 경우 커브 제어에 사용
    public AnimationCurve bridgeCurve = AnimationCurve.Linear(0, 0, 1, 0);

    [Header("시각")]
    public Color bridgeColor = Color.white;
    public Sprite bridgeSprite;

    [Header("사운드")]
    // 판정 시 재생할 음표 클립 (도,레,미... 각 건반/현별로 여러 개)
    public AudioClip[] noteClips;
    public float volume = 1f;
}

public enum BridgeShapeType
{
    Straight,   // 피아노, 플루트, 클라리넷 - 일자
    Curved,     // 기타, 바이올린, 첼로, 밴조 - 곡선
    Zigzag,     // 드럼, 실로폰, 마림바 - 지그재그
    Stepped,    // 베이스, 튜바 - 계단형
    Bouncy,     // 신스, 트럼펫 - 탄성 (플레이어 바운스)
    Slippery,   // 하프, 하모니카 - 미끄러운 표면
    Wide,       // 오르간, 아코디언 - 넓은 플랫폼
}
