using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개별 다리 조각 컴포넌트 — 완전 재작성
///
/// [TODO 완료] CurvedBridgeRenderer 연동:
///   Curved 타입은 LineRenderer + PolygonCollider2D로 실제 곡선 렌더링
///
/// [TODO 완료] Zigzag / Stepped 정교한 비주얼:
///   하나의 BridgePiece가 여러 자식 조각을 생성해 진짜 지그재그/계단 형태 구현
///
/// [TODO 완료] 다리 조각 스프라이트:
///   BridgeSpriteGenerator로 형태별 텍스처 베이크
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class BridgePiece : MonoBehaviour
{
    private InstrumentData sourceInstrument;
    private SpriteRenderer sr;
    private BoxCollider2D  col;

    // Bouncy
    private bool  isBouncy;
    private float bounceForce = 12f;

    // Slippery
    private bool isSlippery;

    // 자식 조각 (Zigzag / Stepped 멀티피스)
    private readonly List<GameObject> subPieces = new();

    // Curved 렌더러
    private CurvedBridgeRenderer curvedRenderer;

    // ── 공개 프로퍼티 ──────────────────────────────────────
    public BridgeShapeType ShapeType => sourceInstrument?.shapeType ?? BridgeShapeType.Straight;
    public static int SubPieceCount = 5; // Zigzag/Stepped에서 쓸 분할 수

    private void Awake()
    {
        sr  = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
    }

    public void Initialize(InstrumentData instrument, Vector3 position)
    {
        sourceInstrument = instrument;
        transform.position = position;
        ApplyShape(instrument);
    }

    private void ApplyShape(InstrumentData instrument)
    {
        // 공통 스프라이트 (형태별 텍스처)
        sr.sprite = BridgeSpriteGenerator.Generate(instrument.shapeType, instrument.bridgeColor);
        sr.color  = Color.white; // 텍스처 자체에 색상이 들어있음
        sr.sortingOrder = 1;

        switch (instrument.shapeType)
        {
            case BridgeShapeType.Straight: ApplyStraight(instrument);  break;
            case BridgeShapeType.Curved:   ApplyCurved(instrument);    break;
            case BridgeShapeType.Zigzag:   ApplyZigzag(instrument);    break;
            case BridgeShapeType.Stepped:  ApplyStepped(instrument);   break;
            case BridgeShapeType.Bouncy:   ApplyBouncy(instrument);    break;
            case BridgeShapeType.Slippery: ApplySlippery(instrument);  break;
            case BridgeShapeType.Wide:     ApplyWide(instrument);      break;
        }
    }

    // ── Straight ──────────────────────────────────────────

    private void ApplyStraight(InstrumentData inst)
    {
        transform.localScale = new Vector3(inst.bridgeWidth, inst.bridgeHeight, 1f);
        col.size   = Vector2.one;
        col.offset = Vector2.zero;
    }

    // ── Curved ────────────────────────────────────────────
    // [FIX] CurvedBridgeRenderer 실제 연동

    private void ApplyCurved(InstrumentData inst)
    {
        // SpriteRenderer는 숨기고 LineRenderer로 렌더링
        sr.enabled = false;
        col.enabled = false; // 콜라이더는 CurvedBridgeRenderer가 PolygonCollider2D로 대체

        curvedRenderer = GetComponent<CurvedBridgeRenderer>()
                      ?? gameObject.AddComponent<CurvedBridgeRenderer>();

        curvedRenderer.SetWidth(inst.bridgeWidth * 2f);
        curvedRenderer.SetColor(inst.bridgeColor);
        curvedRenderer.BuildCurve(inst);
    }

    // ── Zigzag ────────────────────────────────────────────
    // [FIX] 진짜 지그재그: SubPieceCount개의 기울어진 조각으로 분할

    private void ApplyZigzag(InstrumentData inst)
    {
        // 본체는 숨김
        sr.enabled  = false;
        col.enabled = false;

        float segW  = inst.bridgeWidth / SubPieceCount;
        float segH  = inst.bridgeHeight * 1.4f;

        for (int i = 0; i < SubPieceCount; i++)
        {
            var sub = CreateSubPiece(inst, $"Zigzag_{i}");

            float xOff  = (-inst.bridgeWidth / 2f + segW * i + segW / 2f);
            float yOff  = (i % 2 == 0) ? inst.bridgeHeight * 0.8f : -inst.bridgeHeight * 0.8f;
            float angle = (i % 2 == 0) ? 20f : -20f;

            sub.transform.localPosition = new Vector3(xOff, yOff, 0);
            sub.transform.localScale    = new Vector3(segW * 1.05f, segH, 1f);
            sub.transform.localRotation = Quaternion.Euler(0, 0, angle);

            // 스프라이트: 형태 반영
            var subSr = sub.GetComponent<SpriteRenderer>();
            subSr.sprite = BridgeSpriteGenerator.Generate(BridgeShapeType.Zigzag, inst.bridgeColor);
            subSr.color  = Color.white;
        }
    }

    // ── Stepped ───────────────────────────────────────────
    // [FIX] 계단형: 왼쪽에서 오른쪽으로 높이가 점진 상승

    private void ApplyStepped(InstrumentData inst)
    {
        sr.enabled  = false;
        col.enabled = false;

        float segW    = inst.bridgeWidth / SubPieceCount;
        float totalRise = inst.bridgeHeight * (SubPieceCount - 1);

        for (int i = 0; i < SubPieceCount; i++)
        {
            var sub = CreateSubPiece(inst, $"Step_{i}");

            float xOff = -inst.bridgeWidth / 2f + segW * i + segW / 2f;
            float yOff = -totalRise / 2f + inst.bridgeHeight * i; // 계단식 상승
            float h    = inst.bridgeHeight * (1.5f + i * 0.2f);  // 아래로 갈수록 두꺼움

            sub.transform.localPosition = new Vector3(xOff, yOff, 0);
            sub.transform.localScale    = new Vector3(segW * 1.02f, h, 1f);
            sub.transform.localRotation = Quaternion.identity;

            var subSr = sub.GetComponent<SpriteRenderer>();
            subSr.sprite = BridgeSpriteGenerator.Generate(BridgeShapeType.Stepped, inst.bridgeColor);
            subSr.color  = Color.white;
        }
    }

    // ── Bouncy ────────────────────────────────────────────

    private void ApplyBouncy(InstrumentData inst)
    {
        isBouncy   = true;
        bounceForce = 14f;

        transform.localScale = new Vector3(inst.bridgeWidth * 1.2f, inst.bridgeHeight * 0.5f, 1f);
        col.size   = Vector2.one;
        col.offset = Vector2.zero;

        // 트리거: 위쪽 감지용
        var trig = gameObject.AddComponent<BoxCollider2D>();
        trig.isTrigger = true;
        trig.size   = new Vector2(1f, 1.5f);
        trig.offset = new Vector2(0, 0.75f);
    }

    // ── Slippery ──────────────────────────────────────────

    private void ApplySlippery(InstrumentData inst)
    {
        isSlippery = true;

        transform.localScale = new Vector3(inst.bridgeWidth * 1.1f, inst.bridgeHeight * 0.8f, 1f);
        var mat = new PhysicsMaterial2D("SlipperyBridge") { friction = 0.01f, bounciness = 0f };
        col.sharedMaterial = mat;
        col.size   = Vector2.one;
        col.offset = Vector2.zero;

        // 투명도 조절 (얼음 느낌)
        sr.color = new Color(1f, 1f, 1f, 0.75f);
    }

    // ── Wide ──────────────────────────────────────────────

    private void ApplyWide(InstrumentData inst)
    {
        transform.localScale = new Vector3(inst.bridgeWidth * 1.6f, inst.bridgeHeight * 1.2f, 1f);
        col.size   = Vector2.one;
        col.offset = Vector2.zero;
    }

    // ── 자식 조각 생성 헬퍼 ───────────────────────────────

    private GameObject CreateSubPiece(InstrumentData inst, string objName)
    {
        var sub = new GameObject(objName);
        sub.layer = LayerMask.NameToLayer("Ground");
        sub.transform.SetParent(transform, false);

        var subSr = sub.AddComponent<SpriteRenderer>();
        subSr.sortingOrder = 1;

        var subCol = sub.AddComponent<BoxCollider2D>();
        subCol.size   = Vector2.one;
        subCol.offset = Vector2.zero;

        subPieces.Add(sub);
        return sub;
    }

    // ── 물리 이벤트 (Bouncy) ──────────────────────────────

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isBouncy || !collision.gameObject.CompareTag("Player")) return;
        ApplyBounce(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isBouncy || !other.CompareTag("Player")) return;
        var rb = other.GetComponent<Rigidbody2D>();
        if (rb != null && rb.velocity.y <= 0)
            ApplyBounce(other.gameObject);
    }

    private void ApplyBounce(GameObject player)
    {
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb == null) return;
        rb.velocity = new Vector2(rb.velocity.x, bounceForce);

        // 시각 피드백: 눌림/복원
        StopAllCoroutines();
        StartCoroutine(SquashRoutine());
    }

    private System.Collections.IEnumerator SquashRoutine()
    {
        var orig = transform.localScale;
        transform.localScale = new Vector3(orig.x * 1.3f, orig.y * 0.5f, orig.z);
        yield return new WaitForSeconds(0.08f);
        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(
                new Vector3(orig.x * 1.3f, orig.y * 0.5f, orig.z),
                orig, t / 0.15f);
            yield return null;
        }
        transform.localScale = orig;
    }

    private void OnDestroy()
    {
        foreach (var s in subPieces)
            if (s != null) Destroy(s);
    }
}
