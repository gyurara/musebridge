using UnityEngine;

/// <summary>
/// 스테이지 내 악기 아이템 픽업 오브젝트
/// 플레이어가 충돌하면 해당 악기를 영구 보유 목록에 추가
/// </summary>
public class InstrumentPickup : MonoBehaviour
{
    [SerializeField] private InstrumentData instrumentData;
    [SerializeField] private SpriteRenderer iconRenderer;

    public void SetInstrument(InstrumentData data)
    {
        instrumentData = data;
        ApplyVisual();
    }

    private void Start() => ApplyVisual();

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
        StageManager.Instance?.CollectInstrument(instrumentData);
        Destroy(gameObject);
    }
}
