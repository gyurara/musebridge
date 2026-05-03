using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// BGM + UI 효과음 관리자
/// [FIX] PlayBGM(null) 호출 시 크래시 방지 (null 가드 강화)
/// [FIX] CrossFade에서 StopAllCoroutines() → BakeAllClips 코루틴도 중단되던 문제
///       → crossfadeCoroutine 레퍼런스로만 멈춤
/// [FIX] 씬 전환 후 자동 BGM 전환: SceneManager.sceneLoaded 이벤트 구독
/// [FIX] 베이킹 중 PlayBGM 요청 오면 큐에 담아 완료 후 재생
/// [추가] PlaySFXFall() — 낙사 효과음
/// [추가] PlaySFXBuild() — 다리 생성 판정음 (RhythmLineVisual 연동)
/// </summary>
public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("볼륨")]
    [SerializeField] private float bgmVolume = 0.35f;
    [SerializeField] private float sfxVolume = 0.65f;

    [Header("페이드")]
    [SerializeField] private float fadeTime = 1.2f;

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    private AudioClip buildBGM;
    private AudioClip playBGM;
    private AudioClip reviewBGM;
    private AudioClip bestEndingBGM;
    private AudioClip normalEndingBGM;
    private AudioClip badEndingBGM;

    private AudioClip sfxClick;
    private AudioClip sfxClear;
    private AudioClip sfxPickup;
    private AudioClip sfxJudge;
    private AudioClip sfxFall;
    private AudioClip sfxBuild;

    private const int SR = 44100;

    // [FIX] CrossFade 코루틴 레퍼런스
    private Coroutine crossfadeCoroutine;
    private bool      bakingComplete = false;

    // [FIX] 베이킹 전에 요청된 클립 큐
    private AudioClip pendingClip = null;

    // ── 생명주기 ──────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop        = true;
        bgmSource.volume      = bgmVolume;
        bgmSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop        = false;
        sfxSource.volume      = sfxVolume;
        sfxSource.playOnAwake = false;

        // [FIX] 씬 전환 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;

        StartCoroutine(BakeAllClips());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!bakingComplete) return;
        // 씬 이름 기반으로 BGM 자동 전환
        switch (scene.name)
        {
            case "MainMenu":   PlayBGM(buildBGM);       break;
            case "GameScene":  PlayBGMForCurrentPhase(); break;
            case "EndingScene":
                var et = GameManager.Instance?.GetEndingType() ?? EndingType.Normal;
                PlayBGM(et == EndingType.Best   ? bestEndingBGM  :
                        et == EndingType.Normal ? normalEndingBGM : badEndingBGM);
                break;
        }
    }

    // ── 베이킹 ────────────────────────────────────────────

    private IEnumerator BakeAllClips()
    {
        bakingComplete = false;

        buildBGM        = BakeBuildBGM();    yield return null;
        playBGM         = BakePlayBGM();     yield return null;
        reviewBGM       = BakeReviewBGM();   yield return null;
        bestEndingBGM   = BakeBestEnding();  yield return null;
        normalEndingBGM = BakeNormalEnding();yield return null;
        badEndingBGM    = BakeBadEnding();   yield return null;
        sfxClick  = BakeSFXClick();  yield return null;
        sfxClear  = BakeSFXClear();  yield return null;
        sfxPickup = BakeSFXPickup(); yield return null;
        sfxJudge  = BakeSFXJudge();  yield return null;
        sfxFall   = BakeSFXFall();   yield return null;
        sfxBuild  = BakeSFXBuild();  yield return null;

        bakingComplete = true;
        Debug.Log("[BGMManager] 베이킹 완료");

        // [FIX] 베이킹 중 요청된 클립이 있으면 재생
        if (pendingClip != null)
        {
            PlayBGM(pendingClip);
            pendingClip = null;
        }
        else
        {
            PlayBGMForCurrentPhase();
        }
    }

    // ── 공개 API ──────────────────────────────────────────

    public void PlayBGMForCurrentPhase()
    {
        if (!bakingComplete) return;
        var phase = GameManager.Instance?.CurrentPhase ?? GamePhase.MainMenu;
        switch (phase)
        {
            case GamePhase.Build:       PlayBGM(buildBGM);  break;
            case GamePhase.Play:        PlayBGM(playBGM);   break;
            case GamePhase.MusicReview: PlayBGM(reviewBGM); break;
            case GamePhase.Ending:
                var et = GameManager.Instance?.GetEndingType() ?? EndingType.Normal;
                PlayBGM(et == EndingType.Best   ? bestEndingBGM  :
                        et == EndingType.Normal ? normalEndingBGM : badEndingBGM);
                break;
            default: PlayBGM(buildBGM); break;
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        // [FIX] null 가드
        if (clip == null) return;

        // [FIX] 아직 베이킹 안 됐으면 큐에 보관
        if (!bakingComplete)
        {
            pendingClip = clip;
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        // [FIX] 이전 크로스페이드만 중단 (BakeAllClips 건드리지 않음)
        if (crossfadeCoroutine != null)
            StopCoroutine(crossfadeCoroutine);
        crossfadeCoroutine = StartCoroutine(CrossFade(clip));
    }

    // SFX
    public void PlaySFXClick()  { if (sfxClick  != null) sfxSource.PlayOneShot(sfxClick,  sfxVolume); }
    public void PlaySFXClear()  { if (sfxClear  != null) sfxSource.PlayOneShot(sfxClear,  sfxVolume); }
    public void PlaySFXPickup() { if (sfxPickup != null) sfxSource.PlayOneShot(sfxPickup, sfxVolume); }
    public void PlaySFXJudge()  { if (sfxJudge  != null) sfxSource.PlayOneShot(sfxJudge,  sfxVolume * 0.6f); }
    public void PlaySFXFall()   { if (sfxFall   != null) sfxSource.PlayOneShot(sfxFall,   sfxVolume * 0.8f); }
    public void PlaySFXBuild()  { if (sfxBuild  != null) sfxSource.PlayOneShot(sfxBuild,  sfxVolume * 0.5f); }

    // ── 크로스페이드 ──────────────────────────────────────

    private IEnumerator CrossFade(AudioClip newClip)
    {
        float startVol = bgmSource.volume;
        float half     = fadeTime / 2f;

        for (float t = 0; t < half; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVol, 0f, t / half);
            yield return null;
        }
        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.Play();

        for (float t = 0; t < half; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0f, bgmVolume, t / half);
            yield return null;
        }
        bgmSource.volume = bgmVolume;
        crossfadeCoroutine = null;
    }

    // ── BGM 합성 ──────────────────────────────────────────

    private AudioClip BakeBuildBGM()
    {
        int len = SR * 6;
        var d   = new float[len];
        float[] notes = { 293.66f, 349.23f, 440f, 523.25f, 587.33f };
        for (int i = 0; i < len; i++)
        {
            float t   = (float)i / SR;
            int   ni  = (int)(t / 0.5f) % notes.Length;
            float f   = notes[ni];
            float env = Mathf.Pow(1f - (t % 0.5f) / 0.5f, 1.5f);
            float s   = Mathf.Sin(2 * Mathf.PI * f * t) * 0.4f
                      + Mathf.Sin(2 * Mathf.PI * f * 2 * t) * 0.25f
                      + Mathf.Sin(2 * Mathf.PI * f * 3 * t) * 0.15f
                      + Mathf.Sin(2 * Mathf.PI * f * 4 * t) * 0.10f;
            s += Mathf.Sin(2 * Mathf.PI * 73.4f * t) * 0.15f;
            d[i] = s * env * 0.5f;
        }
        return MakeClip(d, "BuildBGM");
    }

    private AudioClip BakePlayBGM()
    {
        int len = SR * 4;
        var d   = new float[len];
        float[] melody = { 523.25f, 587.33f, 659.26f, 698.46f, 783.99f, 698.46f, 659.26f, 587.33f };
        float[] bass   = { 130.81f, 146.83f, 164.81f, 174.61f };
        for (int i = 0; i < len; i++)
        {
            float t    = (float)i / SR;
            int   mi   = (int)(t / 0.25f) % melody.Length;
            float mf   = melody[mi];
            float mEnv = Mathf.Pow(1f - (t % 0.25f) / 0.25f, 2f);
            float mel  = (Mathf.Sin(2 * Mathf.PI * mf * t) * 0.5f
                        + Mathf.Sin(2 * Mathf.PI * mf * 2 * t) * 0.25f) * mEnv;
            int   bi   = (int)t % bass.Length;
            float bf   = bass[bi];
            float bEnv = Mathf.Pow(1f - t % 1f, 1.2f);
            float bas  = Mathf.Sin(2 * Mathf.PI * bf * t) * bEnv * 0.25f;
            float tp   = t % 0.5f;
            float perc = tp < 0.02f ? UnityEngine.Random.Range(-0.3f, 0.3f) * (1f - tp / 0.02f) : 0f;
            d[i] = (mel + bas + perc) * 0.6f;
        }
        return MakeClip(d, "PlayBGM");
    }

    private AudioClip BakeReviewBGM()
    {
        int len = SR * 8;
        var d   = new float[len];
        float[] chord = { 261.63f, 329.63f, 392f, 523.25f };
        for (int i = 0; i < len; i++)
        {
            float t   = (float)i / SR;
            float env = 0.5f + Mathf.Sin(t * 0.3f) * 0.2f;
            float s   = 0f;
            foreach (float f in chord)
                s += Mathf.Sin(2 * Mathf.PI * f * t) / chord.Length;
            s    += Mathf.Sin(2 * Mathf.PI * 260.5f * t) * 0.1f;
            d[i]  = s * env * 0.35f;
        }
        return MakeClip(d, "ReviewBGM");
    }

    private AudioClip BakeBestEnding()
    {
        int len = SR * 6;
        var d   = new float[len];
        float[] fan = { 392f, 493.88f, 587.33f, 783.99f, 987.77f };
        for (int i = 0; i < len; i++)
        {
            float t  = (float)i / SR;
            int   fi = Mathf.Min((int)(t / 0.15f), fan.Length - 1);
            float f  = fan[fi];
            float env= fi < 4 ? Mathf.Pow(1f - (t - fi * 0.15f) / 0.15f, 1.2f)
                              : 0.5f + Mathf.Sin(t * 2f) * 0.2f;
            float s  = Mathf.Sin(2 * Mathf.PI * f * t) * 0.45f
                     + Mathf.Sin(2 * Mathf.PI * f * 2 * t) * 0.2f;
            if (t > 1f)
                s += Mathf.Sin(2 * Mathf.PI * f * 3 * t) * Mathf.Sin(t * 20f) * 0.1f;
            d[i] = s * env * 0.55f;
        }
        return MakeClip(d, "BestEnding");
    }

    private AudioClip BakeNormalEnding()
    {
        int len = SR * 6;
        var d   = new float[len];
        float[] mel = { 329.63f, 293.66f, 261.63f, 293.66f, 329.63f, 392f };
        for (int i = 0; i < len; i++)
        {
            float t   = (float)i / SR;
            int   mi  = (int)(t / 0.6f) % mel.Length;
            float f   = mel[mi];
            float env = Mathf.Pow(1f - (t % 0.6f) / 0.6f, 1.5f);
            d[i] = (Mathf.Sin(2 * Mathf.PI * f * t) * 0.5f
                  + Mathf.Sin(2 * Mathf.PI * f * 0.5f * t) * 0.2f) * env * 0.45f;
        }
        return MakeClip(d, "NormalEnding");
    }

    private AudioClip BakeBadEnding()
    {
        int len = SR * 6;
        var d   = new float[len];
        float[] dis = { 110f, 116.54f, 123.47f };
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SR;
            float s = 0f;
            foreach (float f in dis)
            {
                float w = 1f + Mathf.Sin(t * 0.5f) * 0.005f;
                s += Mathf.Sin(2 * Mathf.PI * f * w * t) / dis.Length;
            }
            d[i] = s * (0.4f + Mathf.Sin(t * 0.2f) * 0.1f) * 0.4f;
        }
        return MakeClip(d, "BadEnding");
    }

    // ── SFX 합성 ──────────────────────────────────────────

    private AudioClip BakeSFXClick()
    {
        var d = new float[SR / 10];
        for (int i = 0; i < d.Length; i++)
        {
            float t = (float)i / SR;
            d[i] = Mathf.Sin(2 * Mathf.PI * 800f * t) * Mathf.Pow(1f - t * 10f, 3f) * 0.5f;
        }
        return MakeClip(d, "SFX_Click");
    }

    private AudioClip BakeSFXClear()
    {
        var d = new float[SR / 2];
        float[] notes = { 523.25f, 659.26f, 783.99f, 1046.5f };
        for (int i = 0; i < d.Length; i++)
        {
            float t   = (float)i / SR;
            int   ni  = (int)(t / 0.1f) % notes.Length;
            float env = Mathf.Pow(1f - (t % 0.1f) / 0.1f, 2f);
            d[i] = Mathf.Sin(2 * Mathf.PI * notes[ni] * t) * env * 0.6f;
        }
        return MakeClip(d, "SFX_Clear");
    }

    private AudioClip BakeSFXPickup()
    {
        var d = new float[SR / 4];
        for (int i = 0; i < d.Length; i++)
        {
            float t   = (float)i / SR;
            float f   = Mathf.Lerp(440f, 880f, Mathf.Clamp01(t * 4f));
            float env = Mathf.Pow(Mathf.Clamp01(1f - t * 4f), 1.5f);
            d[i] = Mathf.Sin(2 * Mathf.PI * f * t) * env * 0.5f;
        }
        return MakeClip(d, "SFX_Pickup");
    }

    private AudioClip BakeSFXJudge()
    {
        var d = new float[SR / 8];
        for (int i = 0; i < d.Length; i++)
        {
            float t   = (float)i / SR;
            float env = Mathf.Pow(Mathf.Clamp01(1f - t * 8f), 2f);
            d[i] = (Mathf.Sin(2 * Mathf.PI * 600f * t) * 0.5f
                  + Mathf.Sin(2 * Mathf.PI * 900f * t) * 0.3f) * env;
        }
        return MakeClip(d, "SFX_Judge");
    }

    // [추가] 낙사 효과음 — 하강 글리산도
    private AudioClip BakeSFXFall()
    {
        int len = SR / 3;
        var d   = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t   = (float)i / SR;
            float f   = Mathf.Lerp(400f, 80f, t * 3f); // 피치 하강
            float env = Mathf.Pow(Mathf.Clamp01(1f - t * 3f), 0.8f);
            d[i] = Mathf.Sin(2 * Mathf.PI * f * t) * env * 0.6f;
        }
        return MakeClip(d, "SFX_Fall");
    }

    // [추가] 다리 생성 판정음 — 짧은 스탭
    private AudioClip BakeSFXBuild()
    {
        int len = SR / 12;
        var d   = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t   = (float)i / SR;
            float env = Mathf.Pow(Mathf.Clamp01(1f - t * 12f), 2f);
            d[i] = (Mathf.Sin(2 * Mathf.PI * 350f * t) * 0.6f
                  + Mathf.Sin(2 * Mathf.PI * 700f * t) * 0.3f) * env;
        }
        return MakeClip(d, "SFX_Build");
    }

    private static AudioClip MakeClip(float[] data, string name)
    {
        var clip = AudioClip.Create(name, data.Length, 1, SR, false);
        clip.SetData(data, 0);
        return clip;
    }
}
