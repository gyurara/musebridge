using UnityEngine;

/// <summary>
/// 개별 다리 조각 컴포넌트
/// 악기 종류에 따라 모양이 달라짐
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class BridgePiece : MonoBehaviour
{
    private InstrumentData sourceInstrument;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D col;

    // Bouncy 다리에서 사용
    private bool isBouncy;
    private float bounceForce = 12f;

    // Slippery 다리에서 사용
    private bool isSlippery;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
    }

    /// <summary>
    /// 악기 데이터를 기반으로 다리 조각 초기화
    /// </summary>
    public void Initialize(InstrumentData instrument, Vector3 position)
    {
        sourceInstrument = instrument;
        transform.position = position;

        ApplyShape(instrument);
    }

    private void ApplyShape(InstrumentData instrument)
    {
        spriteRenderer.color = instrument.bridgeColor;

        if (instrument.bridgeSprite != null)
            spriteRenderer.sprite = instrument.bridgeSprite;

        switch (instrument.shapeType)
        {
            case BridgeShapeType.Straight:
                ApplyStraight(instrument);
                break;
            case BridgeShapeType.Curved:
                ApplyCurved(instrument);
                break;
            case BridgeShapeType.Zigzag:
                ApplyZigzag(instrument);
                break;
            case BridgeShapeType.Stepped:
                ApplyStepped(instrument);
                break;
            case BridgeShapeType.Bouncy:
                ApplyBouncy(instrument);
                break;
            case BridgeShapeType.Slippery:
                ApplySlippery(instrument);
                break;
            case BridgeShapeType.Wide:
                ApplyWide(instrument);
                break;
        }
    }

    private void ApplyStraight(InstrumentData instrument)
    {
        transform.localScale = new Vector3(instrument.bridgeWidth, instrument.bridgeHeight, 1f);
        col.size = Vector2.one;
    }

    private void ApplyCurved(InstrumentData instrument)
    {
        // 곡선형: 사인 커브로 약간 아치 형태
        transform.localScale = new Vector3(instrument.bridgeWidth, instrument.bridgeHeight * 1.2f, 1f);
        float angle = Mathf.Sin(transform.position.x * 0.5f) * 12f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        col.size = Vector2.one;
    }

    private void ApplyZigzag(InstrumentData instrument)
    {
        // 지그재그: 교대로 위/아래 오프셋 + 살짝 기울기
        float offset = (Mathf.FloorToInt(transform.position.x) % 2 == 0) ? 0.15f : -0.15f;
        transform.position += new Vector3(0, offset, 0);
        transform.localScale = new Vector3(instrument.bridgeWidth * 0.8f, instrument.bridgeHeight * 1.3f, 1f);
        float tilt = offset > 0 ? 8f : -8f;
        transform.rotation = Quaternion.Euler(0, 0, tilt);
        col.size = Vector2.one;
    }

    private void ApplyStepped(InstrumentData instrument)
    {
        // 계단형: 점진적으로 높이가 올라가는 형태
        float stepHeight = Mathf.Abs(transform.position.x) * 0.05f;
        transform.position += new Vector3(0, stepHeight, 0);
        transform.localScale = new Vector3(instrument.bridgeWidth * 0.7f, instrument.bridgeHeight * 1.5f, 1f);
        col.size = Vector2.one;
    }

    private void ApplyBouncy(InstrumentData instrument)
    {
        // 탄성 다리: 넓고 납작 + 초록빛 + 플레이어 바운스
        isBouncy = true;
        transform.localScale = new Vector3(instrument.bridgeWidth * 1.2f, instrument.bridgeHeight * 0.6f, 1f);
        col.size = Vector2.one;

        // 트리거 콜라이더 추가 (바운스 감지용)
        var trigger = gameObject.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(1f, 1.5f);
        trigger.offset = new Vector2(0, 0.75f);
    }

    private void ApplySlippery(InstrumentData instrument)
    {
        // 미끄러운 다리: 넓적하고 투명감
        isSlippery = true;
        transform.localScale = new Vector3(instrument.bridgeWidth * 1.1f, instrument.bridgeHeight * 0.8f, 1f);
        var c = spriteRenderer.color;
        spriteRenderer.color = new Color(c.r, c.g, c.b, 0.7f);

        // 물리 재질을 통해 마찰 제거
        var mat = new PhysicsMaterial2D("SlipperyBridge") { friction = 0.02f, bounciness = 0f };
        col.sharedMaterial = mat;
        col.size = Vector2.one;
    }

    private void ApplyWide(InstrumentData instrument)
    {
        // 넓은 플랫폼: 폭 1.5배
        transform.localScale = new Vector3(instrument.bridgeWidth * 1.5f, instrument.bridgeHeight * 1.2f, 1f);
        col.size = Vector2.one;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isBouncy) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        var rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = new Vector2(rb.velocity.x, bounceForce);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isBouncy) return;
        if (!other.CompareTag("Player")) return;

        var rb = other.GetComponent<Rigidbody2D>();
        if (rb != null && rb.velocity.y <= 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, bounceForce);
        }
    }
}
