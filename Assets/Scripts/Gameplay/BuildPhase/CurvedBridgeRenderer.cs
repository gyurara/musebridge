using UnityEngine;

/// <summary>
/// 기타 등 곡선형 악기의 다리를 LineRenderer로 렌더링
/// BridgePiece와 함께 사용 — BridgeShapeType.Curved 일 때 활성화됨
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class CurvedBridgeRenderer : MonoBehaviour
{
    [SerializeField] private int curveResolution = 20;   // 곡선 분할 수 (높을수록 부드러움)
    [SerializeField] private float curveHeight = 0.5f;   // 곡선 최대 높이
    [SerializeField] private float bridgeWidth = 1.5f;

    private LineRenderer lr;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.positionCount = curveResolution + 1;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.sortingOrder = 1;
    }

    /// <summary>
    /// 악기 데이터의 AnimationCurve를 사용해 곡선 형태로 렌더링
    /// </summary>
    public void BuildCurve(InstrumentData instrument)
    {
        if (instrument == null) return;

        lr.startColor = instrument.bridgeColor;
        lr.endColor = instrument.bridgeColor;

        float halfWidth = bridgeWidth / 2f;

        for (int i = 0; i <= curveResolution; i++)
        {
            float t = (float)i / curveResolution;
            float x = Mathf.Lerp(-halfWidth, halfWidth, t);
            // AnimationCurve로 Y 높이 결정 (0~1 범위를 curveHeight 배율로 변환)
            float y = instrument.bridgeCurve.Evaluate(t) * curveHeight;
            lr.SetPosition(i, new Vector3(x, y, 0));
        }

        // 콜라이더를 곡선에 맞게 근사 (폴리곤 콜라이더 사용)
        ApplyCollider();
    }

    private void ApplyCollider()
    {
        var poly = GetComponent<PolygonCollider2D>();
        if (poly == null) poly = gameObject.AddComponent<PolygonCollider2D>();

        var points = new Vector2[lr.positionCount];
        for (int i = 0; i < lr.positionCount; i++)
        {
            var p = lr.GetPosition(i);
            points[i] = new Vector2(p.x, p.y);
        }
        // 단순 상단 경로만으로 얇은 콜라이더 근사
        poly.SetPath(0, points);
    }
}
