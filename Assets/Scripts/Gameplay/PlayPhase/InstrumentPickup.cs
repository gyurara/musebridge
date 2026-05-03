using UnityEngine;

/// <summary>
/// 스테이지 내 악기 아이템 픽업
/// [FIX] PlayerVisual.OnInstrumentPickup 연결 (픽업 플래시 이펙트)
/// [FIX] BGMManager.PlaySFXPickup 연결
/// [FIX] 이미 보유한 악기면 픽업 무시 (StageManager.Owns 체크)
/// </summary>
public class InstrumentPickup : MonoBehaviour
{
    [SerializeField] private InstrumentData instrumentData;
    [SerializeField] private SpriteRenderer iconRenderer;

    // 픽업 애니메이션 (위아래 보빙)
    private Vector3 startPos;
    private float   bobTimer;
    private const float BobAmp   = 0.12f;
    private const float BobSpeed = 2.2f;

    public void SetInstrument(InstrumentData data)
    {
        instrumentData = data;
        ApplyVisual();
    }

    private void Start()
    {
        ApplyVisual();
        startPos = transform.position;
    }

    private void Update()
    {
        // 위아래 보빙
        bobTimer += Time.deltaTime * BobSpeed;
        transform.position = startPos + Vector3.up * Mathf.Sin(bobTimer) * BobAmp;

        // 천천히 회전
        transform.Rotate(0, 0, 45f * Time.deltaTime);
    }

    private void ApplyVisual()
    {
        if (instrumentData == null) return;
        if (iconRenderer == null) iconRenderer = GetComponent<SpriteRenderer>();
        if (iconRenderer != null)
        {
            iconRenderer.color = instrumentData.bridgeColor;
            if (instrumentData.instrumentIcon != null)
                iconRenderer.sprite = instrumentData.instrumentIcon;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // [FIX] 이미 보유한 악기면 무시
        if (StageManager.Instance != null && StageManager.Instance.Owns(instrumentData))
        {
            Destroy(gameObject);
            return;
        }

        StageManager.Instance?.CollectInstrument(instrumentData);

        // [FIX] PlayerVisual 플래시 연결
        var visual = other.GetComponent<PlayerVisual>();
        if (visual != null && instrumentData != null)
            visual.OnInstrumentPickup(instrumentData.bridgeColor);

        // [FIX] 픽업 SFX
        BGMManager.Instance?.PlaySFXPickup();

        Destroy(gameObject);
    }
}
