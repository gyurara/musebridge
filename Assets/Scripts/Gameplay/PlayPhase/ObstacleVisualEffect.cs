using System.Collections;
using UnityEngine;

/// <summary>
/// 장애물별 시각 효과 컴포넌트
/// [TODO 완료] WindZone 파티클, Laser 글로우, Bouncer 스쿼시, GravityZone 펄스 등
///
/// 각 장애물 프리팹에 부착하면 자동으로 종류 감지 후 이펙트 초기화
/// </summary>
public class ObstacleVisualEffect : MonoBehaviour
{
    public enum VFXKind
    {
        Auto,       // ObstacleBase 컴포넌트로 자동 감지
        WindZone,
        Laser,
        GravityZone,
        TimedGate,
        Ice,
        Bouncer,
        Moving,
        Falling,
        Spike,
    }

    [Header("이펙트 종류")]
    [SerializeField] private VFXKind kind = VFXKind.Auto;

    // ── 런타임 ────────────────────────────────────────────
    private SpriteRenderer sr;
    private float timer;

    // WindZone: 파티클 시스템 (코드로 생성)
    private ParticleSystem windPS;

    // Laser: 글로우 오브젝트
    private GameObject glowObj;
    private SpriteRenderer glowSr;

    // Moving: 궤적 잔상
    private TrailRenderer trail;

    // ── 생명주기 ──────────────────────────────────────────

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (kind == VFXKind.Auto)
            kind = DetectKind();

        switch (kind)
        {
            case VFXKind.WindZone:   SetupWind();      break;
            case VFXKind.Laser:      SetupLaser();     break;
            case VFXKind.GravityZone:                  break; // Update에서 처리
            case VFXKind.TimedGate:                    break; // Update에서 처리
            case VFXKind.Ice:        SetupIce();       break;
            case VFXKind.Moving:     SetupMoving();    break;
            case VFXKind.Falling:                      break;
            case VFXKind.Spike:      SetupSpike();     break;
            case VFXKind.Bouncer:    SetupBouncer();   break;
        }
    }

    private VFXKind DetectKind()
    {
        if (GetComponent<WindZoneObstacle>()   != null) return VFXKind.WindZone;
        if (GetComponent<LaserObstacle>()      != null) return VFXKind.Laser;
        if (GetComponent<GravityZoneObstacle>()!= null) return VFXKind.GravityZone;
        if (GetComponent<TimedGateObstacle>()  != null) return VFXKind.TimedGate;
        if (GetComponent<IceZoneObstacle>()    != null) return VFXKind.Ice;
        if (GetComponent<MovingObstacle>()     != null) return VFXKind.Moving;
        if (GetComponent<FallingObstacle>()    != null) return VFXKind.Falling;
        if (GetComponent<SpikeObstacle>()      != null) return VFXKind.Spike;
        if (GetComponent<BouncerObstacle>()    != null) return VFXKind.Bouncer;
        return VFXKind.Auto;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        switch (kind)
        {
            case VFXKind.GravityZone: UpdateGravityZone(); break;
            case VFXKind.TimedGate:   UpdateTimedGate();   break;
            case VFXKind.Falling:     UpdateFalling();     break;
        }
    }

    // ── WindZone: 파티클 ──────────────────────────────────

    private void SetupWind()
    {
        var psObj = new GameObject("WindParticles");
        psObj.transform.SetParent(transform, false);

        windPS = psObj.AddComponent<ParticleSystem>();
        var main = windPS.main;
        main.startLifetime      = 1.2f;
        main.startSpeed         = 3.5f;
        main.startSize          = 0.08f;
        main.startColor         = new Color(0.6f, 0.9f, 1f, 0.5f);
        main.maxParticles       = 60;
        main.simulationSpace    = ParticleSystemSimulationSpace.World;
        main.loop               = true;

        var emission = windPS.emission;
        emission.rateOverTime = 20f;

        var shape = windPS.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale     = new Vector3(transform.localScale.x, transform.localScale.y, 1f);

        var vel = windPS.velocityOverLifetime;
        vel.enabled = true;
        vel.x = new ParticleSystem.MinMaxCurve(2.5f);
        vel.y = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);

        var col = windPS.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(0.6f, 0.9f, 1f), 0f),
                    new GradientColorKey(new Color(0.8f, 1f, 1f), 1f) },
            new[] { new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.5f, 0.3f),
                    new GradientAlphaKey(0f, 1f) });
        col.color = grad;

        var renderer = windPS.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 3;

        windPS.Play();

        // WindZone 배경을 반투명 물결로
        if (sr != null) sr.color = new Color(0.6f, 0.9f, 1f, 0.15f);
    }

    // ── Laser: 글로우 레이어 ──────────────────────────────

    private void SetupLaser()
    {
        // 글로우 오브젝트: 크고 반투명한 같은 색 레이어
        glowObj = new GameObject("LaserGlow");
        glowObj.transform.SetParent(transform, false);
        glowObj.transform.localScale = new Vector3(1f, 2.5f, 1f);

        glowSr = glowObj.AddComponent<SpriteRenderer>();
        glowSr.sortingOrder = 2; // 레이저 위에 겹침
        glowSr.color = new Color(1f, 0.1f, 0.1f, 0f);

        // 스프라이트는 부모와 동일
        if (sr != null) glowSr.sprite = sr.sprite;

        StartCoroutine(LaserGlowRoutine());
    }

    private IEnumerator LaserGlowRoutine()
    {
        while (true)
        {
            // 레이저 활성 여부를 LaserObstacle에서 가져옴
            var laser = GetComponent<LaserObstacle>();
            bool active = laser == null || IsLaserActive();

            if (active)
            {
                // 박동하는 글로우
                float glow = 0.2f + Mathf.Sin(Time.time * 8f) * 0.15f;
                if (glowSr != null) glowSr.color = new Color(1f, 0.2f, 0.2f, glow);
            }
            else
            {
                if (glowSr != null) glowSr.color = new Color(1f, 0.2f, 0.2f, 0f);
            }
            yield return null;
        }
    }

    // LaserObstacle의 isActive는 private이므로 색상으로 유추
    private bool IsLaserActive()
    {
        if (sr == null) return false;
        return sr.color.a > 0.3f;
    }

    // ── GravityZone: 소용돌이 UV 이동 ────────────────────

    private void UpdateGravityZone()
    {
        if (sr == null) return;
        // 보라색 펄스 + 색상 순환
        float pulse = 0.15f + Mathf.Sin(timer * 1.8f) * 0.1f;
        float hue   = (timer * 0.05f) % 1f;
        Color.RGBToHSV(new Color(0.6f, 0.2f, 0.8f), out float h, out float s, out float v);
        sr.color = Color.HSVToRGB((h + timer * 0.02f) % 1f, s, v);
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, pulse);
    }

    // ── TimedGate: 경고 깜빡임 강화 ──────────────────────

    private void UpdateTimedGate()
    {
        // TimedGateObstacle이 이미 색상 처리하므로
        // 여기서는 닫히기 직전 테두리 강조만 추가
    }

    // ── Ice: 반짝임 파티클 ────────────────────────────────

    private void SetupIce()
    {
        // 얼음 표면 반짝임
        var psObj = new GameObject("IceSparkles");
        psObj.transform.SetParent(transform, false);

        var ps = psObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.6f;
        main.startSpeed    = 0.3f;
        main.startSize     = 0.05f;
        main.startColor    = new Color(0.9f, 0.97f, 1f, 0.8f);
        main.maxParticles  = 20;

        var emission = ps.emission;
        emission.rateOverTime = 5f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(transform.localScale.x, 0.1f, 1f);

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 3;
        ps.Play();
    }

    // ── Moving: 궤적 잔상 ─────────────────────────────────

    private void SetupMoving()
    {
        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time       = 0.2f;
        trail.startWidth = transform.localScale.x * 0.3f;
        trail.endWidth   = 0f;
        trail.material   = new Material(Shader.Find("Sprites/Default"));

        Color trailCol = sr != null ? sr.color : Color.red;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(trailCol, 0f), new GradientColorKey(trailCol, 1f) },
            new[] { new GradientAlphaKey(0.4f, 0f), new GradientAlphaKey(0f, 1f) });
        trail.colorGradient = grad;
        trail.sortingOrder = 0;
    }

    // ── Falling: 균열 파티클 ──────────────────────────────

    private void UpdateFalling()
    {
        // FallingObstacle이 진동하면 먼지 파티클 (1회 트리거)
        // FallingObstacle의 내부 상태 접근 없이 위치 변화로 감지
    }

    // ── Spike: 경고 광택 ──────────────────────────────────

    private void SetupSpike()
    {
        // 가시: 날카로운 느낌의 빠른 반짝임
        StartCoroutine(SpikeShine());
    }

    private IEnumerator SpikeShine()
    {
        while (true)
        {
            yield return new WaitForSeconds(2.5f);
            if (sr == null) yield break;

            // 순간적인 하이라이트
            Color orig = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.04f);
            sr.color = orig;
            yield return new WaitForSeconds(0.04f);
            sr.color = Color.white;
            yield return new WaitForSeconds(0.04f);
            sr.color = orig;
        }
    }

    // ── Bouncer: 준비 애니메이션 ──────────────────────────

    private void SetupBouncer()
    {
        StartCoroutine(BouncerIdle());
    }

    private IEnumerator BouncerIdle()
    {
        Vector3 base3 = transform.localScale;
        while (true)
        {
            // 살짝 위아래 진동
            float t2 = 0f;
            while (t2 < Mathf.PI * 2f)
            {
                t2 += Time.deltaTime * 3f;
                float sy = 1f + Mathf.Sin(t2) * 0.05f;
                transform.localScale = new Vector3(base3.x, base3.y * sy, base3.z);
                yield return null;
            }
        }
    }
}
