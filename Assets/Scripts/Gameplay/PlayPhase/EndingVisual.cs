using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 엔딩 씬 연출 [TODO 완료]
/// — Best / Normal / Bad 각각 다른 연출
/// — 점수에 따른 악기 시퀀스 재생
/// — 파티클 + 텍스트 애니메이션
/// — EndingController와 협력 (이 컴포넌트는 순수 비주얼 담당)
///
/// EndingScene의 EndingController 오브젝트에 함께 부착
/// </summary>
public class EndingVisual : MonoBehaviour
{
    [Header("배경 이미지들")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color bestBgColor   = new Color(0.05f, 0.08f, 0.18f); // 심야 파랑
    [SerializeField] private Color normalBgColor = new Color(0.12f, 0.10f, 0.08f); // 따뜻한 갈색
    [SerializeField] private Color badBgColor    = new Color(0.06f, 0.04f, 0.04f); // 어두운 적갈색

    [Header("엔딩별 타이틀")]
    [SerializeField] private TextMeshProUGUI endingTitleText;
    [SerializeField] private TextMeshProUGUI endingSubtitleText;

    [Header("파티클 루트")]
    [SerializeField] private Transform particleRoot;

    [Header("악보 노트 오브젝트 (프리팹 없어도 코드로 생성)")]
    [SerializeField] private bool autoCreateNotes = true;

    private EndingType endingType;

    // ── 생명주기 ──────────────────────────────────────────

    private void Start()
    {
        endingType = GameManager.Instance?.GetEndingType() ?? EndingType.Normal;
        StartCoroutine(PlayEnding(endingType));
    }

    // ── 메인 연출 코루틴 ──────────────────────────────────

    private IEnumerator PlayEnding(EndingType type)
    {
        // 1. 페이드 인
        yield return FadeInBackground(type);

        // 2. 타이틀 연출
        yield return PlayTitle(type);

        // 3. 악기 노트 플로팅 애니메이션
        if (autoCreateNotes)
            StartCoroutine(FloatNotes(type));

        // 4. 파티클
        SpawnEndingParticles(type);

        // 5. 점수 카운트 업
        yield return CountUpScore();
    }

    // ── 배경 페이드 인 ────────────────────────────────────

    private IEnumerator FadeInBackground(EndingType type)
    {
        Color targetColor = type switch
        {
            EndingType.Best   => bestBgColor,
            EndingType.Normal => normalBgColor,
            _                 => badBgColor,
        };

        if (backgroundImage == null) yield break;

        backgroundImage.color = Color.black;
        float t = 0f;
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            backgroundImage.color = Color.Lerp(Color.black, targetColor, t / 1.5f);
            yield return null;
        }
    }

    // ── 타이틀 텍스트 연출 ───────────────────────────────

    private IEnumerator PlayTitle(EndingType type)
    {
        if (endingTitleText == null) yield break;

        (string title, string subtitle, Color col) = type switch
        {
            EndingType.Best   => ("♪ True Musician ♪",
                                  "당신의 다리는 하나의 교향곡이었습니다.",
                                  new Color(1f, 0.84f, 0.1f)),
            EndingType.Normal => ("Bridge Complete",
                                  "음악과 함께 다리를 건넜습니다.",
                                  new Color(0.7f, 0.9f, 1f)),
            _                 => ("Noise...",
                                  "세상은 불협화음으로 가득합니다.",
                                  new Color(0.6f, 0.3f, 0.3f)),
        };

        endingTitleText.text  = "";
        endingTitleText.color = col;

        // 타이핑 효과
        foreach (char c in title)
        {
            endingTitleText.text += c;
            yield return new WaitForSeconds(0.06f);
        }

        yield return new WaitForSeconds(0.5f);

        if (endingSubtitleText != null)
        {
            endingSubtitleText.text  = subtitle;
            endingSubtitleText.color = new Color(col.r, col.g, col.b, 0f);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 1.5f;
                endingSubtitleText.color = new Color(col.r, col.g, col.b, t);
                yield return null;
            }
        }
    }

    // ── 악보 노트 플로팅 ─────────────────────────────────

    private static readonly string[] NoteGlyphs = { "♩", "♪", "♫", "♬", "𝄞", "𝄢" };

    private IEnumerator FloatNotes(EndingType type)
    {
        float duration = type == EndingType.Best ? 12f : 6f;
        float elapsed  = 0f;
        int   rate     = type == EndingType.Best ? 8 : 3;
        var   rng      = new System.Random();

        while (elapsed < duration)
        {
            for (int i = 0; i < rate; i++)
                SpawnFloatingNote(rng, type);

            yield return new WaitForSeconds(0.4f);
            elapsed += 0.4f;
        }
    }

    private void SpawnFloatingNote(System.Random rng, EndingType type)
    {
        // Canvas 없어도 World Space에 텍스트 생성
        var go  = new GameObject("FloatingNote");
        if (particleRoot != null) go.transform.SetParent(particleRoot, false);

        float startX = (float)(rng.NextDouble() * 16 - 8);
        float startY = (float)(rng.NextDouble() * 4  - 6);
        go.transform.localPosition = new Vector3(startX, startY, 0);

        var tm = go.AddComponent<TextMesh>();
        tm.text      = NoteGlyphs[rng.Next(NoteGlyphs.Length)];
        tm.fontSize  = 40 + rng.Next(30);
        tm.color     = NoteColor(type, rng);
        tm.alignment = TextAlignment.Center;
        tm.anchor    = TextAnchor.MiddleCenter;

        StartCoroutine(FloatAndFade(go, rng));
    }

    private Color NoteColor(EndingType type, System.Random rng)
    {
        float r = (float)rng.NextDouble();
        return type switch
        {
            EndingType.Best   => Color.Lerp(new Color(1f, 0.9f, 0.3f), new Color(0.5f, 0.8f, 1f), r),
            EndingType.Normal => Color.Lerp(new Color(0.6f, 0.8f, 1f), new Color(0.9f, 0.9f, 0.9f), r),
            _                 => Color.Lerp(new Color(0.5f, 0.3f, 0.3f), new Color(0.3f, 0.3f, 0.3f), r),
        };
    }

    private IEnumerator FloatAndFade(GameObject go, System.Random rng)
    {
        float dur   = 3f + (float)rng.NextDouble() * 2f;
        float elapsed = 0f;
        var tm    = go.GetComponent<TextMesh>();
        Vector3 vel = new Vector3(
            (float)(rng.NextDouble() - 0.5f) * 0.8f,
            1.2f + (float)rng.NextDouble(),
            0);

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            go.transform.localPosition += vel * Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / dur);
            if (tm != null) tm.color = new Color(tm.color.r, tm.color.g, tm.color.b, alpha);
            yield return null;
        }
        Destroy(go);
    }

    // ── 파티클 ────────────────────────────────────────────

    private void SpawnEndingParticles(EndingType type)
    {
        if (type == EndingType.Bad) return; // Bad 엔딩은 파티클 없음

        var psObj = new GameObject("EndingParticles");
        if (particleRoot != null) psObj.transform.SetParent(particleRoot, false);

        var ps   = psObj.AddComponent<ParticleSystem>();
        var main = ps.main;

        main.startLifetime = type == EndingType.Best ? 4f : 2.5f;
        main.startSpeed    = type == EndingType.Best ? 3f : 1.5f;
        main.startSize     = type == EndingType.Best ? new ParticleSystem.MinMaxCurve(0.05f, 0.2f)
                                                      : new ParticleSystem.MinMaxCurve(0.03f, 0.12f);
        main.maxParticles  = type == EndingType.Best ? 200 : 80;
        main.loop          = false;

        main.startColor = type == EndingType.Best
            ? new ParticleSystem.MinMaxGradient(new Color(1f, 0.85f, 0.2f), new Color(0.4f, 0.8f, 1f))
            : new ParticleSystem.MinMaxGradient(new Color(0.6f, 0.8f, 1f), new Color(0.9f, 0.9f, 1f));

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, main.maxParticles),
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius    = 3f;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.y       = new ParticleSystem.MinMaxCurve(0.5f, 2f);

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 5;

        ps.Play();
        Destroy(psObj, 8f);
    }

    // ── 점수 카운트 업 ────────────────────────────────────

    private IEnumerator CountUpScore()
    {
        int totalScore = GameManager.Instance?.TotalEfficiencyScore ?? 0;
        var scoreText  = GameObject.Find("TotalScoreText")?.GetComponent<TextMeshProUGUI>();
        if (scoreText == null) yield break;

        yield return new WaitForSeconds(1.5f);

        int displayed = 0;
        while (displayed < totalScore)
        {
            displayed = Mathf.Min(displayed + Mathf.Max(1, totalScore / 60), totalScore);
            scoreText.text = $"최종 효율 점수: {displayed}";
            yield return new WaitForSeconds(0.016f);
        }
        scoreText.text = $"최종 효율 점수: {totalScore}";
    }
}
