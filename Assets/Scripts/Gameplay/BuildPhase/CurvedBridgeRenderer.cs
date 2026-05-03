using UnityEngine;

/// <summary>
/// 곡선형 다리 렌더러 — BridgePiece와 완전 연동 버전
///
/// [TODO 완료] BridgePiece.ApplyCurved()에서 직접 호출
///   - LineRenderer로 스플라인 곡선 렌더링
///   - PolygonCollider2D로 물리 근사 (상·하 윤곽선)
///   - AnimationCurve 대신 Catmull-Rom 스플라인 사용 (더 부드러운 아치)
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class CurvedBridgeRenderer : MonoBehaviour
{
    [SerializeField] private int   resolution  = 24;   // 곡선 분할 수
    [SerializeField] private float curveHeight = 0.55f; // 아치 높이
    [SerializeField] private float bridgeWidth = 2f;
    [SerializeField] private float thickness   = 0.18f; // 다리 두께

    private LineRenderer lr;
    private PolygonCollider2D poly;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        lr.useWorldSpace = false;
        lr.positionCount = resolution + 1;
        lr.startWidth    = thickness;
        lr.endWidth      = thickness;
        lr.alignment     = LineAlignment.TransformZ;
        lr.textureMode   = LineTextureMode.Tile;

        var mat = new Material(Shader.Find("Sprites/Default"));
        lr.material     = mat;
        lr.sortingOrder = 1;
    }

    // ── 외부 설정 메서드 (BridgePiece에서 호출) ──────────

    public void SetWidth(float w)  => bridgeWidth  = w;
    public void SetHeight(float h) => curveHeight  = h;

    public void SetColor(Color col)
    {
        if (lr == null) lr = GetComponent<LineRenderer>();
        lr.startColor = col;
        lr.endColor   = col;
        if (lr.material != null) lr.material.color = col;
    }

    /// <summary>
    /// 곡선 빌드 — BridgePiece.ApplyCurved()에서 호출
    /// </summary>
    public void BuildCurve(InstrumentData instrument)
    {
        if (lr == null) lr = GetComponent<LineRenderer>();
        SetColor(instrument.bridgeColor);
        curveHeight = instrument.bridgeHeight * 2.5f;
        BuildSpline();
    }

    // ── 스플라인 계산 ─────────────────────────────────────

    private void BuildSpline()
    {
        float half = bridgeWidth / 2f;

        // 제어점 (Catmull-Rom: 양 끝 + 아치 정점)
        var pts = new Vector3[]
        {
            new Vector3(-half - 0.5f, 0,           0),  // 왼쪽 앵커
            new Vector3(-half,        0,           0),  // 왼쪽 끝
            new Vector3(0,            curveHeight, 0),  // 아치 정점
            new Vector3(half,         0,           0),  // 오른쪽 끝
            new Vector3(half + 0.5f,  0,           0),  // 오른쪽 앵커
        };

        var upper = new Vector3[resolution + 1];
        for (int i = 0; i <= resolution; i++)
        {
            float t = (float)i / resolution;
            upper[i] = CatmullRom(pts, 1, 3, t); // 점 1~3 사이 보간
        }

        lr.positionCount = resolution + 1;
        lr.SetPositions(upper);

        ApplyPolygonCollider(upper);
    }

    // Catmull-Rom 보간 (p0~p3 제어점, t: 0~1)
    private Vector3 CatmullRom(Vector3[] pts, int startIdx, int endIdx, float t)
    {
        // 전체 구간을 segment 수로 나눔
        int segments = endIdx - startIdx;
        float scaledT = t * segments;
        int seg = Mathf.Clamp((int)scaledT, 0, segments - 1);
        float localT = scaledT - seg;

        int p0 = Mathf.Max(startIdx + seg - 1, 0);
        int p1 = startIdx + seg;
        int p2 = Mathf.Min(startIdx + seg + 1, pts.Length - 1);
        int p3 = Mathf.Min(startIdx + seg + 2, pts.Length - 1);

        float t2 = localT * localT;
        float t3 = t2 * localT;

        return 0.5f * (
            2f * pts[p1]
            + (-pts[p0] + pts[p2]) * localT
            + (2f * pts[p0] - 5f * pts[p1] + 4f * pts[p2] - pts[p3]) * t2
            + (-pts[p0] + 3f * pts[p1] - 3f * pts[p2] + pts[p3]) * t3
        );
    }

    // ── 폴리곤 콜라이더 ───────────────────────────────────

    private void ApplyPolygonCollider(Vector3[] upper)
    {
        if (poly == null) poly = GetComponent<PolygonCollider2D>()
                              ?? gameObject.AddComponent<PolygonCollider2D>();

        // 상단 경로 + 하단 경로(두께 적용) 로 닫힌 폴리곤 생성
        int n = upper.Length;
        var path = new Vector2[n * 2];

        for (int i = 0; i < n; i++)
            path[i] = new Vector2(upper[i].x, upper[i].y);

        for (int i = 0; i < n; i++)
        {
            var up = upper[n - 1 - i];
            path[n + i] = new Vector2(up.x, up.y - thickness);
        }

        poly.SetPath(0, path);
    }

    // ── 기즈모 ────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (lr == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < lr.positionCount - 1; i++)
        {
            var a = transform.TransformPoint(lr.GetPosition(i));
            var b = transform.TransformPoint(lr.GetPosition(i + 1));
            Gizmos.DrawLine(a, b);
        }
    }
}
