using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어 비주얼 & 애니메이션
/// — 프로시저럴 픽셀아트 스프라이트를 코드로 생성해 SpriteRenderer에 적용
/// — 상태: Idle / Run / Jump / Fall / Land
/// — 악기 픽업 시 색상 변화 + 반짝임 이펙트
///
/// 사용법: Player 프리팹에 PlayerController와 함께 부착
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerVisual : MonoBehaviour
{
    public enum AnimState { Idle, Run, Jump, Fall, Land }

    [Header("애니메이션 속도")]
    [SerializeField] private float idleFrameRate  = 4f;
    [SerializeField] private float runFrameRate   = 10f;
    [SerializeField] private float landSquashTime = 0.12f;

    [Header("색상 (악기 반영)")]
    [SerializeField] private Color baseBodyColor  = new Color(0.25f, 0.65f, 1.0f);
    [SerializeField] private Color baseAccentColor= new Color(1.0f,  0.85f, 0.25f);

    // ── 상태 ──────────────────────────────────────────────
    private AnimState currentState = AnimState.Idle;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private PlayerController ctrl;

    private float frameTimer;
    private int   frameIndex;

    private Vector3 baseScale;
    private bool    wasGrounded;
    private bool    squashing;

    private Color bodyColor;
    private Color accentColor;

    // ── 픽셀 상수 ─────────────────────────────────────────
    private const int PW = 16, PH = 20; // 픽셀 크기
    private const int PPU = 16;          // Pixels Per Unit

    // ── 프레임 캐시 ───────────────────────────────────────
    private Sprite[] idleFrames;
    private Sprite[] runFrames;
    private Sprite   jumpSprite;
    private Sprite   fallSprite;

    // ── 생명주기 ──────────────────────────────────────────

    private void Awake()
    {
        sr   = GetComponent<SpriteRenderer>();
        rb   = GetComponent<Rigidbody2D>();
        ctrl = GetComponent<PlayerController>();
        baseScale = transform.localScale;

        bodyColor   = baseBodyColor;
        accentColor = baseAccentColor;

        BakeAllFrames();
    }

    private void Update()
    {
        UpdateState();
        TickAnimation();
        FlipSprite();
    }

    // ── 상태 갱신 ─────────────────────────────────────────

    private void UpdateState()
    {
        bool grounded = IsGrounded();
        float vy = rb.velocity.y;
        float vx = Mathf.Abs(rb.velocity.x);

        // 착지 감지
        if (!wasGrounded && grounded && vy < -0.5f)
            StartCoroutine(LandSquash());

        wasGrounded = grounded;

        AnimState next;
        if (!grounded && vy > 0.5f)        next = AnimState.Jump;
        else if (!grounded && vy < -0.5f)  next = AnimState.Fall;
        else if (grounded && vx > 0.5f)    next = AnimState.Run;
        else                               next = AnimState.Idle;

        if (next != currentState)
        {
            currentState = next;
            frameIndex   = 0;
            frameTimer   = 0f;
        }
    }

    private bool IsGrounded()
    {
        // PlayerController의 groundCheck를 재활용
        var gc = transform.Find("GroundCheck");
        if (gc == null) return false;
        return Physics2D.OverlapCircle(gc.position, 0.15f,
            LayerMask.GetMask("Ground")) != null;
    }

    // ── 애니메이션 틱 ─────────────────────────────────────

    private void TickAnimation()
    {
        switch (currentState)
        {
            case AnimState.Idle:
                Tick(idleFrames, idleFrameRate);
                break;
            case AnimState.Run:
                Tick(runFrames, runFrameRate);
                break;
            case AnimState.Jump:
                sr.sprite = jumpSprite;
                break;
            case AnimState.Fall:
                sr.sprite = fallSprite;
                break;
        }
    }

    private void Tick(Sprite[] frames, float fps)
    {
        if (frames == null || frames.Length == 0) return;
        frameTimer += Time.deltaTime;
        if (frameTimer >= 1f / fps)
        {
            frameTimer = 0f;
            frameIndex = (frameIndex + 1) % frames.Length;
        }
        sr.sprite = frames[frameIndex];
    }

    private void FlipSprite()
    {
        float vx = rb.velocity.x;
        if (Mathf.Abs(vx) > 0.2f)
            sr.flipX = vx < 0;
    }

    // ── 착지 스쿼시 ───────────────────────────────────────

    private IEnumerator LandSquash()
    {
        if (squashing) yield break;
        squashing = true;

        float t = 0f;
        while (t < landSquashTime)
        {
            t += Time.deltaTime;
            float p = t / landSquashTime;
            // 가로로 퍼지고 세로로 눌림
            float sx = Mathf.Lerp(1.3f, 1f, p);
            float sy = Mathf.Lerp(0.7f, 1f, p);
            transform.localScale = new Vector3(
                baseScale.x * sx,
                baseScale.y * sy,
                baseScale.z);
            yield return null;
        }
        transform.localScale = baseScale;
        squashing = false;
    }

    // ── 악기 픽업 반응 ────────────────────────────────────

    /// <summary>
    /// InstrumentPickup에서 호출 — 악기 색상으로 플래시
    /// </summary>
    public void OnInstrumentPickup(Color instrumentColor)
    {
        StopCoroutine("PickupFlash");
        StartCoroutine(PickupFlash(instrumentColor));
    }

    private IEnumerator PickupFlash(Color col)
    {
        for (int i = 0; i < 5; i++)
        {
            sr.color = col;
            yield return new WaitForSeconds(0.06f);
            sr.color = Color.white;
            yield return new WaitForSeconds(0.06f);
        }
        sr.color = Color.white;

        // 악센트 색상을 악기 색으로 물들임 (점진적)
        accentColor = Color.Lerp(accentColor, col, 0.4f);
        BakeAllFrames(); // 새 색상으로 재베이크
    }

    // ── 스프라이트 베이킹 ─────────────────────────────────

    private void BakeAllFrames()
    {
        idleFrames = new Sprite[]
        {
            BakeFrame(BuildIdle(0)),
            BakeFrame(BuildIdle(1)),
            BakeFrame(BuildIdle(2)),
            BakeFrame(BuildIdle(1)),
        };

        runFrames = new Sprite[]
        {
            BakeFrame(BuildRun(0)),
            BakeFrame(BuildRun(1)),
            BakeFrame(BuildRun(2)),
            BakeFrame(BuildRun(3)),
        };

        jumpSprite = BakeFrame(BuildJump());
        fallSprite = BakeFrame(BuildFall());

        if (idleFrames.Length > 0) sr.sprite = idleFrames[0];
    }

    private Sprite BakeFrame(Color32[] pixels)
    {
        var tex = new Texture2D(PW, PH, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode   = TextureWrapMode.Clamp,
        };
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex,
            new Rect(0, 0, PW, PH),
            new Vector2(0.5f, 0f),   // pivot 바닥 중앙
            PPU);
    }

    // ── 픽셀 페인팅 헬퍼 ──────────────────────────────────

    private Color32[] NewCanvas() => new Color32[PW * PH]; // 투명

    private void SetPixel(Color32[] buf, int x, int y, Color32 col)
    {
        if (x < 0 || x >= PW || y < 0 || y >= PH) return;
        buf[y * PW + x] = col;
    }

    private void FillRect(Color32[] buf, int x0, int y0, int w, int h, Color32 col)
    {
        for (int dy = 0; dy < h; dy++)
            for (int dx = 0; dx < w; dx++)
                SetPixel(buf, x0 + dx, y0 + dy, col);
    }

    // ── 색상 팔레트 ───────────────────────────────────────

    private Color32 Body   => (Color32)bodyColor;
    private Color32 Shadow => (Color32)(bodyColor * 0.65f);
    private Color32 Accent => (Color32)accentColor;
    private Color32 White  => new Color32(240, 240, 240, 255);
    private Color32 Dark   => new Color32(30,  25,  20,  255);
    private Color32 Skin   => new Color32(255, 210, 170, 255);
    private Color32 Trans  => new Color32(0,   0,   0,   0);

    // ── 프레임 빌더 ───────────────────────────────────────

    // 공통: 머리(4×4), 몸통(6×6), 팔, 눈
    private void DrawHead(Color32[] buf, int bx, int by)
    {
        FillRect(buf, bx,   by,   8, 6, Skin);           // 얼굴
        FillRect(buf, bx+1, by+4, 6, 2, Body);           // 머리카락(위)
        SetPixel(buf, bx+2, by+3, Dark);                  // 눈L
        SetPixel(buf, bx+5, by+3, Dark);                  // 눈R
        SetPixel(buf, bx+2, by+2, White);                 // 눈하이라이트L
        SetPixel(buf, bx+5, by+2, White);                 // 눈하이라이트R
    }

    private void DrawBody(Color32[] buf, int bx, int by)
    {
        FillRect(buf, bx-1, by,   10, 7, Body);
        FillRect(buf, bx,   by,   8,  7, Body);
        // 가슴 악센트 라인
        FillRect(buf, bx+1, by+3, 6,  1, Accent);
        // 어깨 그림자
        SetPixel(buf, bx-1, by+1, Shadow);
        SetPixel(buf, bx+8, by+1, Shadow);
    }

    private void DrawLegs(Color32[] buf, int bx, int by, bool left_forward)
    {
        // 다리 2개 교차
        int lOff = left_forward ?  2 : -2;
        int rOff = left_forward ? -2 :  2;
        FillRect(buf, bx+1+lOff, by,   3, 5, Shadow);
        FillRect(buf, bx+4+rOff, by,   3, 5, Dark);
        // 발
        FillRect(buf, bx+lOff,   by-1, 4, 2, Dark);
        FillRect(buf, bx+3+rOff, by-1, 4, 2, Dark);
    }

    private Color32[] BuildIdle(int frame)
    {
        var buf = NewCanvas();
        int bx = 4;   // 수평 오프셋

        // 몸통 y=7~13, 머리 y=14~19, 다리 y=1~6
        DrawBody(buf, bx, 7);
        DrawHead(buf, bx, 13);

        // idle: 약간 위아래 bob
        int bob = frame == 2 ? 1 : 0;
        FillRect(buf, bx+1, 1+bob, 3, 6, Shadow);  // 왼다리
        FillRect(buf, bx+4, 1+bob, 3, 6, Dark);    // 오른다리
        FillRect(buf, bx,   0,     4, 2, Dark);    // 왼발
        FillRect(buf, bx+3, 0,     4, 2, Dark);    // 오른발

        // 팔 (살짝 흔들)
        int armBob = frame == 1 ? 1 : 0;
        FillRect(buf, bx-2, 9+armBob, 2, 4, Shadow);  // 왼팔
        FillRect(buf, bx+8, 9-armBob, 2, 4, Shadow);  // 오른팔
        return buf;
    }

    private Color32[] BuildRun(int frame)
    {
        var buf = NewCanvas();
        int bx = 4;

        DrawBody(buf, bx, 7);
        DrawHead(buf, bx, 13);

        // 다리 교차 4프레임
        int[][] legPoses = new int[][]
        {
            new[]{-2, 0,  2, -2},
            new[]{ 0, 2, -2,  0},
            new[]{ 2, 0, -2,  2},
            new[]{ 0,-2,  2,  0},
        };
        int[] lp = legPoses[frame % 4];

        // 뒷다리
        FillRect(buf, bx+4+lp[2], 2, 3, 5+lp[3], Dark);
        FillRect(buf, bx+3+lp[2], 0, 4, 2, Dark);
        // 앞다리
        FillRect(buf, bx+1+lp[0], 2, 3, 5+lp[1], Shadow);
        FillRect(buf, bx+lp[0],   0, 4, 2, Dark);

        // 팔 교차
        FillRect(buf, bx-2, 9+lp[1]/2, 2, 4, Shadow);
        FillRect(buf, bx+8, 9-lp[1]/2, 2, 4, Shadow);
        return buf;
    }

    private Color32[] BuildJump()
    {
        var buf = NewCanvas();
        int bx = 4;

        DrawBody(buf, bx, 8);
        DrawHead(buf, bx, 14);

        // 무릎 굽힌 다리
        FillRect(buf, bx+1, 4, 3, 4, Shadow);
        FillRect(buf, bx+4, 4, 3, 4, Dark);
        FillRect(buf, bx,   2, 4, 2, Dark);   // 발L
        FillRect(buf, bx+4, 3, 4, 2, Dark);   // 발R

        // 팔 벌림
        FillRect(buf, bx-3, 11, 3, 3, Shadow);
        FillRect(buf, bx+8, 11, 3, 3, Shadow);
        return buf;
    }

    private Color32[] BuildFall()
    {
        var buf = NewCanvas();
        int bx = 4;

        DrawBody(buf, bx, 6);
        DrawHead(buf, bx, 12);

        // 다리 뒤로 쭉
        FillRect(buf, bx+1, 1, 3, 5, Shadow);
        FillRect(buf, bx+4, 0, 3, 6, Dark);
        FillRect(buf, bx,   0, 4, 2, Dark);
        FillRect(buf, bx+3, 0, 4, 2, Dark);

        // 팔 아래로
        FillRect(buf, bx-2, 8, 2, 5, Shadow);
        FillRect(buf, bx+8, 8, 2, 5, Shadow);
        return buf;
    }
}
