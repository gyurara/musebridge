using System.Collections;
using UnityEngine;

/// <summary>
/// 장애물별 시각 효과
/// [FIX] BouncerIdle 내부 while(true) 안의 inner while이 yield return null을 가져야
///        외부 while(true)가 진행됨 → 구조 단순화
/// [FIX] UpdateTimedGate 빈 함수 → TimedGateObstacle의 col.enabled 상태로 경고 강도 표시
/// [FIX] UpdateFalling 빈 함수 → FallingObstacle에 이벤트 콜백 연결
/// [FIX] WindZone shape.scale이 transform.localScale 기준 → StageBuilder 스폰 후 스케일 적용 전
///        Awake에서 Start로 이동
/// </summary>
public class ObstacleVisualEffect : MonoBehaviour
{
    public enum VFXKind
    {
        Auto, WindZone, Laser, GravityZone, TimedGate, Ice, Bouncer, Moving, Falling, Spike
    }

    [SerializeField] private VFXKind kind = VFXKind.Auto;

    private SpriteRenderer sr;
    private float timer;

    private ParticleSystem windPS;
    private SpriteRenderer glowSr;
    private TrailRenderer  trail;

    // Falling 먼지 파티클
    private ParticleSystem dustPS;
    private bool           fallingTriggered;

    // TimedGate
    private BoxCollider2D gateCol;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (kind == VFXKind.Auto) kind = DetectKind();
    }

    // [FIX] Start로 이동 — StageBuilder가 localScale 적용한 후
    private void Start()
    {
        switch (kind)
        {
            case VFXKind.WindZone:   SetupWind();     break;
            case VFXKind.Laser:      SetupLaser();    break;
            case VFXKind.Ice:        SetupIce();      break;
            case VFXKind.Moving:     SetupMoving();   break;
            case VFXKind.Falling:    SetupFalling();  break;
            case VFXKind.Spike:      SetupSpike();    break;
            case VFXKind.Bouncer:    SetupBouncer();  break;
            case VFXKind.TimedGate:
                gateCol = GetComponent<BoxCollider2D>();
                break;
        }
    }

    private VFXKind DetectKind()
    {
        if (GetComponent<WindZoneObstacle>()    != null) return VFXKind.WindZone;
        if (GetComponent<LaserObstacle>()       != null) return VFXKind.Laser;
        if (GetComponent<GravityZoneObstacle>() != null) return VFXKind.GravityZone;
        if (GetComponent<TimedGateObstacle>()   != null) return VFXKind.TimedGate;
        if (GetComponent<IceZoneObstacle>()     != null) return VFXKind.Ice;
        if (GetComponent<MovingObstacle>()      != null) return VFXKind.Moving;
        if (GetComponent<FallingObstacle>()     != null) return VFXKind.Falling;
        if (GetComponent<SpikeObstacle>()       != null) return VFXKind.Spike;
        if (GetComponent<BouncerObstacle>()     != null) return VFXKind.Bouncer;
        return VFXKind.Auto;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        switch (kind)
        {
            case VFXKind.GravityZone: UpdateGravityZone(); break;
            case VFXKind.TimedGate:   UpdateTimedGate();   break;
            case VFXKind.Falling:     UpdateFalling();      break;
        }
    }

    // ── WindZone ──────────────────────────────────────────

    private void SetupWind()
    {
        var psObj = new GameObject("WindParticles");
        psObj.transform.SetParent(transform, false);
        windPS = psObj.AddComponent<ParticleSystem>();

        var main = windPS.main;
        main.startLifetime   = 1.2f;
        main.startSpeed      = 3.5f;
        main.startSize       = 0.08f;
        main.startColor      = new Color(0.6f, 0.9f, 1f, 0.5f);
        main.maxParticles    = 60;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop            = true;

        windPS.emission.rateOverTime = 20f;

        var shape = windPS.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        // [FIX] Start에서 호출하므로 localScale이 이미 적용된 상태
        shape.scale = new Vector3(
            Mathf.Abs(transform.localScale.x),
            Mathf.Abs(transform.localScale.y), 1f);

        var vel = windPS.velocityOverLifetime;
        vel.enabled = true;
        vel.x = new ParticleSystem.MinMaxCurve(2.5f);
        vel.y = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);

        var col = windPS.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(0.6f, 0.9f, 1f), 0f),
                    new GradientColorKey(new Color(0.8f, 1f,  1f), 1f) },
            new[] { new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.5f, 0.3f),
                    new GradientAlphaKey(0f, 1f) });
        col.color = grad;

        windPS.GetComponent<ParticleSystemRenderer>().material =
            new Material(Shader.Find("Sprites/Default")) { renderQueue = 3001 };
        windPS.Play();

        if (sr != null) sr.color = new Color(0.6f, 0.9f, 1f, 0.15f);
    }

    // ── Laser ─────────────────────────────────────────────

    private void SetupLaser()
    {
        var glowObj = new GameObject("LaserGlow");
        glowObj.transform.SetParent(transform, false);
        glowObj.transform.localScale = new Vector3(1f, 2.5f, 1f);
        glowSr = glowObj.AddComponent<SpriteRenderer>();
        glowSr.sortingOrder = 2;
        glowSr.color = new Color(1f, 0.1f, 0.1f, 0f);
        if (sr != null) glowSr.sprite = sr.sprite;
        StartCoroutine(LaserGlowRoutine());
    }

    private IEnumerator LaserGlowRoutine()
    {
        while (true)
        {
            bool active = sr == null || sr.color.a > 0.3f;
            if (glowSr != null)
            {
                float glow = active ? 0.2f + Mathf.Sin(Time.time * 8f) * 0.15f : 0f;
                glowSr.color = new Color(1f, 0.2f, 0.2f, glow);
            }
            yield return null;
        }
    }

    // ── GravityZone ───────────────────────────────────────

    private void UpdateGravityZone()
    {
        if (sr == null) return;
        Color.RGBToHSV(new Color(0.6f, 0.2f, 0.8f), out float h, out float s, out float v);
        Color c = Color.HSVToRGB((h + timer * 0.02f) % 1f, s, v);
        sr.color = new Color(c.r, c.g, c.b, 0.15f + Mathf.Sin(timer * 1.8f) * 0.1f);
    }

    // ── TimedGate — [FIX] 실제로 경고 효과 구현 ──────────

    private void UpdateTimedGate()
    {
        if (sr == null) return;
        // 닫혀 있을 때 (col.enabled=true) → 강하게 표시
        // 열리기/닫히기 직전 깜빡임은 TimedGateObstacle이 처리
        // 여기서는 닫혀 있을 때 외곽 강조만 추가
        bool closed = gateCol == null || gateCol.enabled;
        float pulse = closed
            ? 1f
            : 0.3f + Mathf.Sin(timer * 12f) * 0.2f;  // 열려있으면 빠른 깜빡임
        var c = sr.color;
        sr.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(pulse));
    }

    // ── Ice ───────────────────────────────────────────────

    private void SetupIce()
    {
        var psObj = new GameObject("IceSparkles");
        psObj.transform.SetParent(transform, false);
        var ps   = psObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.6f;
        main.startSpeed    = 0.3f;
        main.startSize     = 0.05f;
        main.startColor    = new Color(0.9f, 0.97f, 1f, 0.8f);
        main.maxParticles  = 20;
        ps.emission.rateOverTime = 5f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(Mathf.Abs(transform.localScale.x), 0.1f, 1f);
        ps.GetComponent<ParticleSystemRenderer>().material =
            new Material(Shader.Find("Sprites/Default"));
        ps.Play();
    }

    // ── Moving ────────────────────────────────────────────

    private void SetupMoving()
    {
        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time       = 0.2f;
        trail.startWidth = Mathf.Abs(transform.localScale.x) * 0.3f;
        trail.endWidth   = 0f;
        trail.material   = new Material(Shader.Find("Sprites/Default"));
        var grad = new Gradient();
        Color tc = sr != null ? sr.color : Color.red;
        grad.SetKeys(
            new[] { new GradientColorKey(tc, 0f), new GradientColorKey(tc, 1f) },
            new[] { new GradientAlphaKey(0.4f, 0f), new GradientAlphaKey(0f, 1f) });
        trail.colorGradient = grad;
        trail.sortingOrder  = 0;
    }

    // ── Falling — [FIX] 먼지 파티클 실제 구현 ────────────

    private void SetupFalling()
    {
        var psObj = new GameObject("FallingDust");
        psObj.transform.SetParent(transform, false);
        dustPS = psObj.AddComponent<ParticleSystem>();

        var main = dustPS.main;
        main.startLifetime = 0.8f;
        main.startSpeed    = new ParticleSystem.MinMaxCurve(0.5f, 2f);
        main.startSize     = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);
        main.startColor    = new Color(0.7f, 0.6f, 0.4f, 0.7f);
        main.maxParticles  = 30;
        main.loop          = false;
        main.playOnAwake   = false;

        dustPS.emission.rateOverTime = 0f;
        dustPS.emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 15) });

        var shape = dustPS.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(Mathf.Abs(transform.localScale.x), 0.1f, 1f);

        dustPS.GetComponent<ParticleSystemRenderer>().material =
            new Material(Shader.Find("Sprites/Default"));
    }

    private void UpdateFalling()
    {
        if (fallingTriggered || dustPS == null) return;
        // 위치 변화로 낙하 시작 감지
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic && rb.velocity.magnitude > 0.5f)
        {
            fallingTriggered = true;
            dustPS.Play();
        }
    }

    // ── Spike ─────────────────────────────────────────────

    private void SetupSpike()
    {
        StartCoroutine(SpikeShine());
    }

    private IEnumerator SpikeShine()
    {
        while (true)
        {
            yield return new WaitForSeconds(2.5f);
            if (sr == null) yield break;
            Color orig = sr.color;
            for (int i = 0; i < 2; i++)
            {
                sr.color = Color.white;
                yield return new WaitForSeconds(0.04f);
                sr.color = orig;
                yield return new WaitForSeconds(0.04f);
            }
        }
    }

    // ── Bouncer — [FIX] 단순화된 아이들 진동 ─────────────

    private void SetupBouncer()
    {
        StartCoroutine(BouncerIdle());
    }

    private IEnumerator BouncerIdle()
    {
        Vector3 base3 = transform.localScale;
        float   phase = 0f;
        while (true)
        {
            // [FIX] 단일 while 루프 + yield return null 매 프레임
            phase += Time.deltaTime * 3f;
            float sy = 1f + Mathf.Sin(phase) * 0.05f;
            transform.localScale = new Vector3(base3.x, base3.y * sy, base3.z);
            yield return null;
        }
    }
}
