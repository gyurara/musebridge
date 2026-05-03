using System.Collections;
using UnityEngine;

/// <summary>
/// BGM + UI 효과음 [TODO 완료]
///
/// ① BGM: 프로시저럴 오디오 클립 런타임 생성
///    - BuildPhase: 느린 아르페지오 (오르간 파형)
///    - PlayPhase:  밝고 경쾌한 루프 (피아노 + 마림바)
///    - MusicReview: 잔잔한 앰비언트
///    - Ending:     스테이지별 다른 테마
///
/// ② UI SFX: 버튼 클릭, 스테이지 클리어, 악기 픽업, 판정음
///
/// GameManager 오브젝트에 부착 또는 --- MANAGERS --- 하위에 배치
/// </summary>
public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("BGM 볼륨")]
    [SerializeField] private float bgmVolume = 0.35f;
    [SerializeField] private float sfxVolume = 0.65f;

    [Header("페이드 시간")]
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

    private const int SR = 44100;

    // ── 생명주기 ──────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop       = true;
        bgmSource.volume     = bgmVolume;
        bgmSource.playOnAwake= false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop       = false;
        sfxSource.volume     = sfxVolume;
        sfxSource.playOnAwake= false;

        StartCoroutine(BakeAllClips());
    }

    // ── 클립 베이킹 (첫 프레임 이후 비동기) ─────────────

    private IEnumerator BakeAllClips()
    {
        // 각 클립을 1프레임씩 나눠 생성 → 히치 방지
        buildBGM       = BakeBuildBGM();    yield return null;
        playBGM        = BakePlayBGM();     yield return null;
        reviewBGM      = BakeReviewBGM();   yield return null;
        bestEndingBGM  = BakeBestEnding();  yield return null;
        normalEndingBGM= BakeNormalEnding();yield return null;
        badEndingBGM   = BakeBadEnding();   yield return null;

        sfxClick  = BakeSFXClick();   yield return null;
        sfxClear  = BakeSFXClear();   yield return null;
        sfxPickup = BakeSFXPickup();  yield return null;
        sfxJudge  = BakeSFXJudge();   yield return null;

        Debug.Log("[BGMManager] 모든 BGM/SFX 베이킹 완료");

        // 씬 로드 후 자동으로 현재 페이즈 BGM 시작
        PlayBGMForCurrentPhase();
    }

    // ── 공개 API ──────────────────────────────────────────

    public void PlayBGMForCurrentPhase()
    {
        var phase = GameManager.Instance?.CurrentPhase ?? GamePhase.MainMenu;
        switch (phase)
        {
            case GamePhase.Build:       PlayBGM(buildBGM);    break;
            case GamePhase.Play:        PlayBGM(playBGM);     break;
            case GamePhase.MusicReview: PlayBGM(reviewBGM);   break;
            case GamePhase.Ending:
                var et = GameManager.Instance?.GetEndingType() ?? EndingType.Normal;
                PlayBGM(et == EndingType.Best   ? bestEndingBGM  :
                        et == EndingType.Normal ? normalEndingBGM : badEndingBGM);
                break;
            default:
                PlayBGM(buildBGM); break;
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;
        StopAllCoroutines();
        StartCoroutine(CrossFade(clip));
    }

    public void PlaySFXClick()  => sfxSource.PlayOneShot(sfxClick,  sfxVolume);
    public void PlaySFXClear()  => sfxSource.PlayOneShot(sfxClear,  sfxVolume);
    public void PlaySFXPickup() => sfxSource.PlayOneShot(sfxPickup, sfxVolume);
    public void PlaySFXJudge()  => sfxSource.PlayOneShot(sfxJudge,  sfxVolume * 0.6f);

    // ── 크로스페이드 ──────────────────────────────────────

    private IEnumerator CrossFade(AudioClip newClip)
    {
        float startVol = bgmSource.volume;
        // 페이드 아웃
        float t = 0f;
        while (t < fadeTime / 2f)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVol, 0f, t / (fadeTime / 2f));
            yield return null;
        }
        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.Play();
        // 페이드 인
        t = 0f;
        while (t < fadeTime / 2f)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, bgmVolume, t / (fadeTime / 2f));
            yield return null;
        }
        bgmSource.volume = bgmVolume;
    }

    // ── BGM 베이킹 ─────────────────────────────────────────
    // 모든 클립: 4초 루프, 44100 Hz

    private static readonly float[] C4_SCALE = { 261.63f, 293.66f, 329.63f, 349.23f, 392f, 440f, 493.88f, 523.25f };

    // 빌드 페이즈: 느린 오르간 아르페지오 (D minor)
    private AudioClip BakeBuildBGM()
    {
        int len = SR * 6; // 6초 루프
        float[] data = new float[len];
        float[] notes = { 293.66f, 349.23f, 440f, 523.25f, 587.33f }; // Dm 아르페지오

        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SR;
            // 아르페지오: 0.5초마다 음 전환
            int noteIdx = (int)(t / 0.5f) % notes.Length;
            float freq  = notes[noteIdx];
            float env   = Mathf.Pow(1f - ((t % 0.5f) / 0.5f), 1.5f);

            // 오르간 파형 (하모닉스)
            float s = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.4f
                    + Mathf.Sin(2 * Mathf.PI * freq * 2 * t) * 0.25f
                    + Mathf.Sin(2 * Mathf.PI * freq * 3 * t) * 0.15f
                    + Mathf.Sin(2 * Mathf.PI * freq * 4 * t) * 0.10f;

            // 저음 드론 (D2)
            s += Mathf.Sin(2 * Mathf.PI * 73.4f * t) * 0.15f;

            data[i] = s * env * 0.5f;
        }
        return MakeClip(data, "BuildBGM", true);
    }

    // 플레이 페이즈: 밝고 경쾌 (C major, 피아노+마림바 느낌)
    private AudioClip BakePlayBGM()
    {
        int len = SR * 4;
        float[] data = new float[len];

        // 멜로디 시퀀스 (C major scale 패턴)
        float[] melody = { 523.25f, 587.33f, 659.26f, 698.46f, 783.99f, 698.46f, 659.26f, 587.33f };
        float[] bass   = { 130.81f, 146.83f, 164.81f, 174.61f };

        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SR;

            // 멜로디: 0.25초 퀀텀
            int mi   = (int)(t / 0.25f) % melody.Length;
            float mf = melody[mi];
            float mEnv= Mathf.Pow(1f - ((t % 0.25f) / 0.25f), 2f);
            float mel = (Mathf.Sin(2 * Mathf.PI * mf * t) * 0.5f
                       + Mathf.Sin(2 * Mathf.PI * mf * 2 * t) * 0.25f) * mEnv;

            // 베이스: 1박자
            int bi   = (int)(t) % bass.Length;
            float bf = bass[bi];
            float bEnv= Mathf.Pow(1f - ((t % 1f)), 1.2f);
            float bas = Mathf.Sin(2 * Mathf.PI * bf * t) * bEnv * 0.25f;

            // 퍼커션 (짧은 노이즈, 0.5초마다)
            float perc = 0f;
            float tp = t % 0.5f;
            if (tp < 0.02f)
                perc = UnityEngine.Random.Range(-0.3f, 0.3f) * (1f - tp / 0.02f);

            data[i] = (mel + bas + perc) * 0.6f;
        }
        return MakeClip(data, "PlayBGM", true);
    }

    // 음악 감상: 잔잔한 앰비언트 (사인파 패드)
    private AudioClip BakeReviewBGM()
    {
        int len = SR * 8;
        float[] data = new float[len];
        float[] chords = { 261.63f, 329.63f, 392f, 523.25f }; // C major chord

        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SR;
            float env = 0.5f + Mathf.Sin(t * 0.3f) * 0.2f; // 천천히 숨쉬는 느낌
            float s = 0f;
            foreach (float f in chords)
                s += Mathf.Sin(2 * Mathf.PI * f * t) / chords.Length;
            // 미세 디튠
            s += Mathf.Sin(2 * Mathf.PI * 260.5f * t) * 0.1f;
            data[i] = s * env * 0.35f;
        }
        return MakeClip(data, "ReviewBGM", true);
    }

    // Best 엔딩: 화려한 팡파레 + 밝은 루프
    private AudioClip BakeBestEnding()
    {
        int len = SR * 6;
        float[] data = new float[len];
        // 상행 분산화음 (G major)
        float[] fanfare = { 392f, 493.88f, 587.33f, 783.99f, 987.77f };

        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SR;
            int fi  = Mathf.Min((int)(t / 0.15f), fanfare.Length - 1);
            float f = fanfare[fi];
            float env= fi < 4 ? Mathf.Pow(1f - ((t - fi * 0.15f) / 0.15f), 1.2f)
                              : 0.5f + Mathf.Sin(t * 2f) * 0.2f;

            float s = Mathf.Sin(2 * Mathf.PI * f * t) * 0.45f
                    + Mathf.Sin(2 * Mathf.PI * f * 2 * t) * 0.2f;

            // 반짝이는 트레몰로 (Best에만)
            if (t > 1f)
                s += Mathf.Sin(2 * Mathf.PI * f * 3 * t) * Mathf.Sin(t * 20f) * 0.1f;

            data[i] = s * env * 0.55f;
        }
        return MakeClip(data, "BestEnding", true);
    }

    // Normal 엔딩: 서정적 멜로디
    private AudioClip BakeNormalEnding()
    {
        int len = SR * 6;
        float[] data = new float[len];
        float[] melody = { 329.63f, 293.66f, 261.63f, 293.66f, 329.63f, 392f };

        for (int i = 0; i < len; i++)
        {
            float t   = (float)i / SR;
            int mi    = (int)(t / 0.6f) % melody.Length;
            float f   = melody[mi];
            float env = Mathf.Pow(1f - ((t % 0.6f) / 0.6f), 1.5f);
            float s   = Mathf.Sin(2 * Mathf.PI * f * t) * 0.5f
                      + Mathf.Sin(2 * Mathf.PI * f * 0.5f * t) * 0.2f;
            data[i] = s * env * 0.45f;
        }
        return MakeClip(data, "NormalEnding", true);
    }

    // Bad 엔딩: 불협화음 드론
    private AudioClip BakeBadEnding()
    {
        int len = SR * 6;
        float[] data = new float[len];
        float[] dissonant = { 110f, 116.54f, 123.47f }; // 반음 간격

        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SR;
            float s = 0f;
            foreach (float f in dissonant)
            {
                float wobble = 1f + Mathf.Sin(t * 0.5f) * 0.005f;
                s += Mathf.Sin(2 * Mathf.PI * f * wobble * t) / dissonant.Length;
            }
            float env = 0.4f + Mathf.Sin(t * 0.2f) * 0.1f;
            data[i] = s * env * 0.4f;
        }
        return MakeClip(data, "BadEnding", true);
    }

    // ── SFX 베이킹 ────────────────────────────────────────

    private AudioClip BakeSFXClick()
    {
        int len = SR / 10; // 0.1초
        float[] d = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SR;
            float env = Mathf.Pow(1f - t / (1f / 10f), 3f);
            d[i] = Mathf.Sin(2 * Mathf.PI * 800f * t) * env * 0.5f;
        }
        return MakeClip(d, "SFX_Click", false);
    }

    private AudioClip BakeSFXClear()
    {
        int len = SR / 2; // 0.5초
        float[] d = new float[len];
        float[] notes = { 523.25f, 659.26f, 783.99f, 1046.5f };
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SR;
            int ni  = (int)(t / 0.1f) % notes.Length;
            float env = Mathf.Pow(1f - ((t % 0.1f) / 0.1f), 2f);
            d[i] = Mathf.Sin(2 * Mathf.PI * notes[ni] * t) * env * 0.6f;
        }
        return MakeClip(d, "SFX_Clear", false);
    }

    private AudioClip BakeSFXPickup()
    {
        int len = SR / 4;
        float[] d = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SR;
            float f = Mathf.Lerp(440f, 880f, t * 4f); // 피치 상승
            float env = Mathf.Pow(1f - t * 4f, 1.5f);
            d[i] = Mathf.Sin(2 * Mathf.PI * f * t) * env * 0.5f;
        }
        return MakeClip(d, "SFX_Pickup", false);
    }

    private AudioClip BakeSFXJudge()
    {
        int len = SR / 8;
        float[] d = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SR;
            float env = Mathf.Pow(1f - t * 8f, 2f);
            d[i] = (Mathf.Sin(2 * Mathf.PI * 600f * t) * 0.5f
                  + Mathf.Sin(2 * Mathf.PI * 900f * t) * 0.3f) * env;
        }
        return MakeClip(d, "SFX_Judge", false);
    }

    // ── 유틸 ──────────────────────────────────────────────

    private static AudioClip MakeClip(float[] data, string name, bool loop)
    {
        var clip = AudioClip.Create(name, data.Length, 1, SR, false);
        clip.SetData(data, 0);
        return clip;
    }
}
