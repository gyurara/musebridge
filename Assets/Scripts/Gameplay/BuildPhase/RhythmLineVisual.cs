using UnityEngine;

/// <summary>
/// 판정선 시각화
/// - 세로 라인 (SpriteRenderer or LineRenderer)
/// - 판정 성공 시 플래시 이펙트
/// - 이동 중 트레일 잔상 효과
/// </summary>
[RequireComponent(typeof(RhythmLineController))]
public class RhythmLineVisual : MonoBehaviour
{
    [Header("라인 시각")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float lineHeight = 10f;
    [SerializeField] private Color defaultColor = new Color(0.2f, 0.6f, 1f, 0.8f);
    [SerializeField] private float lineWidth = 0.08f;

    [Header("판정 플래시")]
    [SerializeField] private Color flashColor = new Color(1f, 0.9f, 0.2f, 1f);
    [SerializeField] private float flashDuration = 0.12f;

    [Header("트레일")]
    [SerializeField] private TrailRenderer trailRenderer;

    private float flashTimer = 0f;
    private bool isFlashing = false;

    private void Awake()
    {
        SetupLineRenderer();
        SetupTrail();
    }

    private void SetupLineRenderer()
    {
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = false;
        lineRenderer.SetPosition(0, new Vector3(0, -lineHeight / 2f, 0));
        lineRenderer.SetPosition(1, new Vector3(0, lineHeight / 2f, 0));

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = defaultColor;
        lineRenderer.endColor = defaultColor;
        lineRenderer.sortingOrder = 5;
    }

    private void SetupTrail()
    {
        if (trailRenderer == null)
            trailRenderer = gameObject.AddComponent<TrailRenderer>();

        trailRenderer.time = 0.15f;
        trailRenderer.startWidth = lineWidth * 0.8f;
        trailRenderer.endWidth = 0f;
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));

        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(defaultColor, 0f),
                new GradientColorKey(defaultColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.5f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        trailRenderer.colorGradient = gradient;
        trailRenderer.sortingOrder = 4;
    }

    private void Update()
    {
        if (isFlashing)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
            {
                isFlashing = false;
                SetColor(defaultColor);
            }
        }
    }

    /// <summary>
    /// 다리 생성 판정 시 외부에서 호출하여 플래시 효과
    /// </summary>
    public void PlayJudgementFlash()
    {
        isFlashing = true;
        flashTimer = flashDuration;
        SetColor(flashColor);
    }

    private void SetColor(Color color)
    {
        if (lineRenderer == null) return;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    private void OnEnable()
    {
        if (trailRenderer != null) trailRenderer.Clear();
    }
}
