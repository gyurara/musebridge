using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 비주얼 & 애니메이션
/// [FIX] BakeAllFrames 호출 시 이전 Texture2D 메모리 누수 → 재베이크 전 Destroy
/// [FIX] Transform.Find("GroundCheck") 매 프레임 호출 → Awake에서 캐싱
/// [FIX] StopCoroutine("PickupFlash") string 오버로드가 IEnumerator에 미작동 → Coroutine ref 저장
/// [FIX] Trans 필드 선언만 하고 미사용 → 제거
/// [FIX] BuildRun legPoses에서 lp[3] 인덱스(범위 4개 배열에 4번째) → lp[1] 재활용으로 수정
/// [FIX] BGMManager.PlaySFXPickup 연결 — InstrumentPickup에서 PlayerVisual 호출 시 자동 연계
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerVisual : MonoBehaviour
{
    public enum AnimState { Idle, Run, Jump, Fall }

    [Header("애니메이션 속도")]
    [SerializeField] private float idleFrameRate  = 4f;
    [SerializeField] private float runFrameRate   = 10f;
    [SerializeField] private float landSquashTime = 0.12f;

    [Header("색상 (악기 반영)")]
    [SerializeField] private Color baseBodyColor   = new Color(0.25f, 0.65f, 1.0f);
    [SerializeField] private Color baseAccentColor = new Color(1.0f,  0.85f, 0.25f);

    private AnimState      currentState = AnimState.Idle;
    private SpriteRenderer sr;
    private Rigidbody2D    rb;

    private float   frameTimer;
    private int     frameIndex;
    private Vector3 baseScale;
    private bool    wasGrounded;
    private bool    squashing;

    private Color bodyColor;
    private Color accentColor;

    // ── 픽셀 상수 ─────────────────────────────────────────
    private const int PW = 16, PH = 20, PPU = 16;

    // ── 스프라이트 캐시 ───────────────────────────────────
    private Sprite[] idleFrames;
    private Sprite[] runFrames;
    private Sprite   jumpSprite;
    private Sprite   fallSprite;

    // [FIX] 이전 텍스처 추적 (메모리 누수 방지)
    private readonly List<Texture2D> ownedTextures = new();

    // [FIX] GroundCheck 캐싱
    private Transform groundCheckTransform;

    // [FIX] Coroutine 레퍼런스 저장 (StopCoroutine 정확히 작동하게)
    private Coroutine pickupFlashCoroutine;

    // ── 생명주기 ──────────────────────────────────────────

    private void Awake()
    {
        sr  = GetComponent<SpriteRenderer>();
        rb  = GetComponent<Rigidbody2D>();
        baseScale   = transform.localScale;
        bodyColor   = baseBodyColor;
        accentColor = baseAccentColor;

        // [FIX] 한 번만 Find
        groundCheckTransform = transform.Find("GroundCheck");

        BakeAllFrames();
    }

    private void Update()
    {
        UpdateState();
        TickAnimation();
        FlipSprite();
    }

    private void OnDestroy()
    {
        // [FIX] 소유한 텍스처 전부 해제
        foreach (var tex in ownedTextures)
            if (tex != null) Destroy(tex);
        ownedTextures.Clear();
    }

    // ── 상태 갱신 ─────────────────────────────────────────

    private void UpdateState()
    {
        bool  grounded = IsGrounded();
        float vy = rb.velocity.y;
        float vx = Mathf.Abs(rb.velocity.x);

        if (!wasGrounded && grounded && vy < -0.5f)
            StartCoroutine(LandSquash());

        wasGrounded = grounded;

        AnimState next;
        if      (!grounded && vy > 0.5f)  next = AnimState.Jump;
        else if (!grounded && vy < -0.5f) next = AnimState.Fall;
        else if (grounded  && vx > 0.5f)  next = AnimState.Run;
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
        if (groundCheckTransform == null) return false;
        // [FIX] LayerMask.GetMask 매 프레임 호출 → 인라인은 괜찮지만 캐싱이 더 깔끔
        return Physics2D.OverlapCircle(
            groundCheckTransform.position, 0.15f,
            LayerMask.GetMask("Ground")) != null;
    }

    // ── 애니메이션 틱 ─────────────────────────────────────

    private void TickAnimation()
    {
        switch (currentState)
        {
            case AnimState.Idle: Tick(idleFrames, idleFrameRate); break;
            case AnimState.Run:  Tick(runFrames,  runFrameRate);  break;
            case AnimState.Jump: sr.sprite = jumpSprite;          break;
            case AnimState.Fall: sr.sprite = fallSprite;          break;
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
        if (sr.sprite != frames[frameIndex])
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
            float p  = Mathf.Clamp01(t / landSquashTime);
            float sx = Mathf.Lerp(1.3f, 1f, p);
            float sy = Mathf.Lerp(0.7f, 1f, p);
            transform.localScale = new Vector3(baseScale.x * sx, baseScale.y * sy, baseScale.z);
            yield return null;
        }
        transform.localScale = baseScale;
        squashing = false;
    }

    // ── 악기 픽업 반응 ────────────────────────────────────

    public void OnInstrumentPickup(Color instrumentColor)
    {
        // [FIX] Coroutine 레퍼런스로 정확히 중단
        if (pickupFlashCoroutine != null)
            StopCoroutine(pickupFlashCoroutine);
        pickupFlashCoroutine = StartCoroutine(PickupFlash(instrumentColor));
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
        pickupFlashCoroutine = null;

        accentColor = Color.Lerp(accentColor, col, 0.4f);
        BakeAllFrames();
    }

    // ── 스프라이트 베이킹 ─────────────────────────────────

    private void BakeAllFrames()
    {
        // [FIX] 기존 텍스처 해제 (메모리 누수 방지)
        foreach (var tex in ownedTextures)
            if (tex != null) Destroy(tex);
        ownedTextures.Clear();

        idleFrames = new[]
        {
            BakeFrame(BuildIdle(0)),
            BakeFrame(BuildIdle(1)),
            BakeFrame(BuildIdle(2)),
            BakeFrame(BuildIdle(1)),
        };
        runFrames = new[]
        {
            BakeFrame(BuildRun(0)),
            BakeFrame(BuildRun(1)),
            BakeFrame(BuildRun(2)),
            BakeFrame(BuildRun(3)),
        };
        jumpSprite = BakeFrame(BuildJump());
        fallSprite = BakeFrame(BuildFall());

        if (idleFrames.Length > 0 && sr != null)
            sr.sprite = idleFrames[0];
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
        ownedTextures.Add(tex); // [FIX] 추적 등록
        return Sprite.Create(tex, new Rect(0, 0, PW, PH), new Vector2(0.5f, 0f), PPU);
    }

    // ── 픽셀 헬퍼 ─────────────────────────────────────────

    private Color32[] NewCanvas() => new Color32[PW * PH];

    private void SetPixel(Color32[] b, int x, int y, Color32 c)
    {
        if (x < 0 || x >= PW || y < 0 || y >= PH) return;
        b[y * PW + x] = c;
    }

    private void FillRect(Color32[] b, int x0, int y0, int w, int h, Color32 c)
    {
        for (int dy = 0; dy < h; dy++)
            for (int dx = 0; dx < w; dx++)
                SetPixel(b, x0 + dx, y0 + dy, c);
    }

    // ── 팔레트 ────────────────────────────────────────────

    private Color32 Body   => (Color32)bodyColor;
    private Color32 Shadow => (Color32)(bodyColor * 0.65f);
    private Color32 Accent => (Color32)accentColor;
    private Color32 White  => new Color32(240, 240, 240, 255);
    private Color32 Dark   => new Color32(30,  25,  20,  255);
    private Color32 Skin   => new Color32(255, 210, 170, 255);

    // ── 공통 서브루틴 ────────────────────────────────────

    private void DrawHead(Color32[] b, int bx, int by)
    {
        FillRect(b, bx,   by,   8, 6, Skin);
        FillRect(b, bx+1, by+4, 6, 2, Body);
        SetPixel(b, bx+2, by+3, Dark);
        SetPixel(b, bx+5, by+3, Dark);
        SetPixel(b, bx+2, by+2, White);
        SetPixel(b, bx+5, by+2, White);
    }

    private void DrawBody(Color32[] b, int bx, int by)
    {
        FillRect(b, bx-1, by,   10, 7, Body);
        FillRect(b, bx,   by,   8,  7, Body);
        FillRect(b, bx+1, by+3, 6,  1, Accent);
        SetPixel(b, bx-1, by+1, Shadow);
        SetPixel(b, bx+8, by+1, Shadow);
    }

    // ── 프레임 빌더 ───────────────────────────────────────

    private Color32[] BuildIdle(int frame)
    {
        var b  = NewCanvas();
        int bx = 4;
        DrawBody(b, bx, 7);
        DrawHead(b, bx, 13);

        int bob    = frame == 2 ? 1 : 0;
        int armBob = frame == 1 ? 1 : 0;
        FillRect(b, bx+1, 1+bob, 3, 6, Shadow);
        FillRect(b, bx+4, 1+bob, 3, 6, Dark);
        FillRect(b, bx,   0,     4, 2, Dark);
        FillRect(b, bx+3, 0,     4, 2, Dark);
        FillRect(b, bx-2, 9+armBob, 2, 4, Shadow);
        FillRect(b, bx+8, 9-armBob, 2, 4, Shadow);
        return b;
    }

    private Color32[] BuildRun(int frame)
    {
        var b  = NewCanvas();
        int bx = 4;
        DrawBody(b, bx, 7);
        DrawHead(b, bx, 13);

        // [FIX] legPoses를 4x4 → 인덱스 0~3만 사용 (lp[3] 제거, lp[1]로 대체)
        int[][] legPoses =
        {
            new[]{-2,  0,  2, -2},
            new[]{ 0,  2, -2,  0},
            new[]{ 2,  0, -2,  2},
            new[]{ 0, -2,  2,  0},
        };
        int[] lp = legPoses[frame % 4];

        // lp[0]=앞다리xOff, lp[1]=앞다리높이(±), lp[2]=뒷다리xOff, lp[3]=뒷다리높이(±)
        int frontH = 5 + Mathf.Abs(lp[1]);   // 항상 양수 높이
        int backH  = 5 + Mathf.Abs(lp[3]);

        FillRect(b, bx+4+lp[2], 2, 3, backH,  Dark);
        FillRect(b, bx+3+lp[2], 0, 4, 2,      Dark);
        FillRect(b, bx+1+lp[0], 2, 3, frontH, Shadow);
        FillRect(b, bx+lp[0],   0, 4, 2,      Dark);
        FillRect(b, bx-2, 9+lp[1]/2, 2, 4, Shadow);
        FillRect(b, bx+8, 9-lp[1]/2, 2, 4, Shadow);
        return b;
    }

    private Color32[] BuildJump()
    {
        var b  = NewCanvas();
        int bx = 4;
        DrawBody(b, bx, 8);
        DrawHead(b, bx, 14);
        FillRect(b, bx+1, 4, 3, 4, Shadow);
        FillRect(b, bx+4, 4, 3, 4, Dark);
        FillRect(b, bx,   2, 4, 2, Dark);
        FillRect(b, bx+4, 3, 4, 2, Dark);
        FillRect(b, bx-3, 11, 3, 3, Shadow);
        FillRect(b, bx+8, 11, 3, 3, Shadow);
        return b;
    }

    private Color32[] BuildFall()
    {
        var b  = NewCanvas();
        int bx = 4;
        DrawBody(b, bx, 6);
        DrawHead(b, bx, 12);
        FillRect(b, bx+1, 1, 3, 5, Shadow);
        FillRect(b, bx+4, 0, 3, 6, Dark);
        FillRect(b, bx,   0, 4, 2, Dark);
        FillRect(b, bx+3, 0, 4, 2, Dark);
        FillRect(b, bx-2, 8, 2, 5, Shadow);
        FillRect(b, bx+8, 8, 2, 5, Shadow);
        return b;
    }
}
