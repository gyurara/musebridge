using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 악보 느낌의 배경 생성
/// - 크림색 종이 배경
/// - 오선지 (5줄 x 여러 그룹)
/// - 소절 구분선
/// - 점선 느낌의 여백
/// Canvas 불필요 — 월드 스페이스 LineRenderer + Quad로 렌더링
/// </summary>
public class SheetMusicBackground : MonoBehaviour
{
    [Header("카메라")]
    [SerializeField] private Camera mainCamera;

    [Header("배경색")]
    [SerializeField] private Color backgroundColor = new Color(0.96f, 0.94f, 0.87f, 1f); // 크림 종이

    [Header("오선지")]
    [SerializeField] private int staffGroupCount = 4;        // 오선 그룹 수
    [SerializeField] private int linesPerStaff = 5;          // 한 그룹당 줄 수
    [SerializeField] private float lineSpacing = 0.22f;      // 줄 간격
    [SerializeField] private float staffGroupSpacing = 1.8f; // 그룹 간 간격
    [SerializeField] private Color staffColor = new Color(0.25f, 0.22f, 0.18f, 0.55f);
    [SerializeField] private float staffLineWidth = 0.025f;

    [Header("소절 구분선")]
    [SerializeField] private int barCount = 6;
    [SerializeField] private Color barColor = new Color(0.25f, 0.22f, 0.18f, 0.7f);
    [SerializeField] private float barLineWidth = 0.045f;

    [Header("여백 점선 (장식)")]
    [SerializeField] private bool showDotDecoration = true;
    [SerializeField] private Color dotColor = new Color(0.3f, 0.25f, 0.2f, 0.18f);

    [Header("렌더링 순서")]
    [SerializeField] private int bgSortingOrder = -20;
    [SerializeField] private int lineSortingOrder = -18;

    private GameObject bgQuad;
    private readonly List<GameObject> createdObjects = new();

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        Generate();
    }

    private void Generate()
    {
        CreateBackground();
        CreateStaffLines();
        CreateBarLines();
        if (showDotDecoration) CreateDotDecorations();
    }

    // ── 배경 ─────────────────────────────────────────────

    private void CreateBackground()
    {
        bgQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bgQuad.name = "BG_Paper";
        bgQuad.transform.SetParent(transform);
        Destroy(bgQuad.GetComponent<MeshCollider>());

        float h = mainCamera.orthographicSize * 2f;
        float w = h * mainCamera.aspect;
        bgQuad.transform.localPosition = new Vector3(0, 0, 2f);
        bgQuad.transform.localScale = new Vector3(w, h, 1f);

        var rend = bgQuad.GetComponent<Renderer>();
        rend.material = CreateMaterial(backgroundColor);
        rend.sortingOrder = bgSortingOrder;
    }

    // ── 오선지 ───────────────────────────────────────────

    private void CreateStaffLines()
    {
        float camH = mainCamera.orthographicSize * 2f;
        float camW = mainCamera.orthographicSize * 2f * mainCamera.aspect;

        float staffH = (linesPerStaff - 1) * lineSpacing;
        float totalH = staffGroupCount * staffH + (staffGroupCount - 1) * staffGroupSpacing;
        float startY = totalH / 2f - 0.3f;

        for (int g = 0; g < staffGroupCount; g++)
        {
            float groupTop = startY - g * (staffH + staffGroupSpacing);

            for (int l = 0; l < linesPerStaff; l++)
            {
                float y = groupTop - l * lineSpacing;
                CreateLineH(y, camW * 1.1f, staffColor, staffLineWidth, $"Staff_{g}_{l}");
            }

            // 그룹 왼쪽 세로 마감선
            float groupBot = groupTop - (linesPerStaff - 1) * lineSpacing;
            CreateLineV((groupTop + groupBot) / 2f, staffH + staffLineWidth,
                barColor, barLineWidth * 1.5f, $"StaffEdge_{g}");
        }
    }

    // ── 소절 구분선 ──────────────────────────────────────

    private void CreateBarLines()
    {
        float camH = mainCamera.orthographicSize * 2f;
        float camW = mainCamera.orthographicSize * 2f * mainCamera.aspect;

        float staffH = (linesPerStaff - 1) * lineSpacing;
        float totalH = staffGroupCount * staffH + (staffGroupCount - 1) * staffGroupSpacing;
        float startY = totalH / 2f - 0.3f;

        float xStep = camW / barCount;
        float startX = -camW / 2f + xStep;

        for (int b = 0; b < barCount - 1; b++)
        {
            float x = startX + b * xStep;

            // 각 오선 그룹에 소절선 그리기
            for (int g = 0; g < staffGroupCount; g++)
            {
                float groupTop = startY - g * (staffH + staffGroupSpacing);
                float groupBot = groupTop - (linesPerStaff - 1) * lineSpacing;
                float midY = (groupTop + groupBot) / 2f;
                float h = staffH + staffLineWidth;

                CreateLineV(midY, h, barColor, barLineWidth, $"Bar_{b}_{g}", x);
            }
        }
    }

    // ── 장식 점들 ─────────────────────────────────────────

    private void CreateDotDecorations()
    {
        float camH = mainCamera.orthographicSize * 2f;
        float camW = mainCamera.orthographicSize * 2f * mainCamera.aspect;

        // 배경에 규칙적인 점 패턴
        float dotSpacing = 1.2f;
        int cols = Mathf.CeilToInt(camW / dotSpacing) + 1;
        int rows = Mathf.CeilToInt(camH / dotSpacing) + 1;

        float startX = -camW / 2f;
        float startY = -camH / 2f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float x = startX + c * dotSpacing;
                float y = startY + r * dotSpacing;

                var dot = GameObject.CreatePrimitive(PrimitiveType.Quad);
                dot.name = "Dot";
                dot.transform.SetParent(transform);
                dot.transform.localPosition = new Vector3(x, y, 1.5f);
                dot.transform.localScale = new Vector3(0.04f, 0.04f, 1f);
                Destroy(dot.GetComponent<MeshCollider>());

                var rend = dot.GetComponent<Renderer>();
                rend.material = CreateMaterial(dotColor);
                rend.sortingOrder = lineSortingOrder - 1;

                createdObjects.Add(dot);
            }
        }
    }

    // ── 유틸 ─────────────────────────────────────────────

    private void CreateLineH(float y, float width, Color color, float thickness, string objName)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        obj.name = objName;
        obj.transform.SetParent(transform);
        obj.transform.localPosition = new Vector3(0, y, 1f);
        obj.transform.localScale = new Vector3(width, thickness, 1f);
        Destroy(obj.GetComponent<MeshCollider>());

        var rend = obj.GetComponent<Renderer>();
        rend.material = CreateMaterial(color);
        rend.sortingOrder = lineSortingOrder;
        createdObjects.Add(obj);
    }

    private void CreateLineV(float centerY, float height, Color color, float thickness, string objName, float x = 0f)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        obj.name = objName;
        obj.transform.SetParent(transform);
        obj.transform.localPosition = new Vector3(x, centerY, 1f);
        obj.transform.localScale = new Vector3(thickness, height, 1f);
        Destroy(obj.GetComponent<MeshCollider>());

        var rend = obj.GetComponent<Renderer>();
        rend.material = CreateMaterial(color);
        rend.sortingOrder = lineSortingOrder;
        createdObjects.Add(obj);
    }

    private Material CreateMaterial(Color color)
    {
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        return mat;
    }

    private void OnDestroy()
    {
        foreach (var obj in createdObjects)
            if (obj != null) Destroy(obj);
        if (bgQuad != null) Destroy(bgQuad);
    }
}
