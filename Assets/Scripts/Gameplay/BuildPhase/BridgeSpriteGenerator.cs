using UnityEngine;

/// <summary>
/// 다리 조각 타입별 프로시저럴 스프라이트 생성기
/// BridgePiece.Initialize() 시 호출되어 악기/형태에 맞는 텍스처를 베이크
///
/// Straight  → 나무판자 결 (수평 선무늬)
/// Curved    → 아치 돌 블록 (곡선 테두리)
/// Zigzag    → 금속 격자 (대각 해칭)
/// Stepped   → 계단 벽돌 (오프셋 직사각형)
/// Bouncy    → 스프링 패턴 (초록 물결)
/// Slippery  → 얼음 결정 (대각 반짝임)
/// Wide      → 넓은 플랫폼 (2색 타일)
/// </summary>
public static class BridgeSpriteGenerator
{
    private const int W = 64, H = 16, PPU = 16;

    public static Sprite Generate(BridgeShapeType shape, Color baseColor)
    {
        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode   = TextureWrapMode.Clamp,
        };

        Color32[] pixels = shape switch
        {
            BridgeShapeType.Straight  => PaintStraight(baseColor),
            BridgeShapeType.Curved    => PaintCurved(baseColor),
            BridgeShapeType.Zigzag    => PaintZigzag(baseColor),
            BridgeShapeType.Stepped   => PaintStepped(baseColor),
            BridgeShapeType.Bouncy    => PaintBouncy(baseColor),
            BridgeShapeType.Slippery  => PaintSlippery(baseColor),
            BridgeShapeType.Wide      => PaintWide(baseColor),
            _                         => PaintStraight(baseColor),
        };

        tex.SetPixels32(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, W, H),
            new Vector2(0.5f, 0.5f), PPU);
    }

    // ── 헬퍼 ──────────────────────────────────────────────

    private static Color32[] NewCanvas(Color fill)
    {
        var buf = new Color32[W * H];
        Color32 c = fill;
        for (int i = 0; i < buf.Length; i++) buf[i] = c;
        return buf;
    }

    private static void Set(Color32[] buf, int x, int y, Color32 col)
    {
        if (x < 0 || x >= W || y < 0 || y >= H) return;
        buf[y * W + x] = col;
    }

    private static Color32 Darken(Color c, float f) =>
        new Color32((byte)(c.r * f * 255), (byte)(c.g * f * 255), (byte)(c.b * f * 255), 255);

    private static Color32 Lighten(Color c, float f) =>
        new Color32(
            (byte)Mathf.Min(c.r * f * 255, 255),
            (byte)Mathf.Min(c.g * f * 255, 255),
            (byte)Mathf.Min(c.b * f * 255, 255), 255);

    private static Color32 ToC32(Color c) =>
        new Color32((byte)(c.r * 255), (byte)(c.g * 255), (byte)(c.b * 255), 255);

    // ── 나무판자 (Straight) ────────────────────────────────
    // 수평 결 + 상단 하이라이트 + 하단 그림자

    private static Color32[] PaintStraight(Color col)
    {
        var buf = NewCanvas(col);
        var dark   = Darken(col, 0.6f);
        var light  = Lighten(col, 1.35f);
        var shadow = Darken(col, 0.35f);

        // 나무 결 (3픽셀 간격 어두운 줄)
        for (int x = 0; x < W; x++)
            for (int y = 1; y < H - 1; y++)
                if (y % 3 == 0)
                    Set(buf, x, y, dark);

        // 상단 하이라이트
        for (int x = 0; x < W; x++) Set(buf, x, H - 1, light);
        // 하단 그림자
        for (int x = 0; x < W; x++) Set(buf, x, 0, shadow);

        // 나무 마디 (5개)
        for (int k = 0; k < 5; k++)
        {
            int kx = 6 + k * 11;
            for (int dy = 2; dy < H - 2; dy++)
                Set(buf, kx, dy, dark);
        }
        return buf;
    }

    // ── 아치 돌 (Curved) ──────────────────────────────────
    // 돌 블록 패턴 + 줄눈(grout)

    private static Color32[] PaintCurved(Color col)
    {
        var buf    = NewCanvas(col);
        var grout  = Darken(col, 0.45f);
        var hiLight= Lighten(col, 1.3f);
        var shadow = Darken(col, 0.55f);

        // 돌 블록 그리드 (8픽셀 폭, 4픽셀 높이)
        for (int x = 0; x < W; x++)
        {
            if (x % 8 == 0) // 수직 줄눈
                for (int y = 0; y < H; y++) Set(buf, x, y, grout);
        }
        for (int y = 0; y < H; y += 4) // 수평 줄눈
            for (int x = 0; x < W; x++) Set(buf, x, y, grout);

        // 블록 내부 베벨 (오른쪽·아래에 그림자)
        for (int bx = 1; bx < W; bx += 8)
            for (int by = 1; by < H; by += 4)
            {
                int bw = Mathf.Min(6, W - bx);
                int bh = Mathf.Min(2, H - by);
                // 오른쪽 그림자
                if (bx + bw < W) Set(buf, bx + bw, by, shadow);
                // 상단 하이라이트
                Set(buf, bx, by + bh, hiLight);
            }
        return buf;
    }

    // ── 금속 격자 (Zigzag) ────────────────────────────────
    // 대각선 해칭

    private static Color32[] PaintZigzag(Color col)
    {
        var buf    = NewCanvas(col);
        var dark   = Darken(col, 0.5f);
        var bright = Lighten(col, 1.4f);

        // 대각 격자선
        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
            {
                if ((x + y) % 6 == 0) Set(buf, x, y, dark);
                if ((x - y + 64) % 6 == 0) Set(buf, x, y, dark);
            }

        // 리벳 (볼트)
        for (int rx = 4; rx < W; rx += 12)
        {
            int ry = H / 2;
            Set(buf, rx,   ry,   bright);
            Set(buf, rx+1, ry,   bright);
            Set(buf, rx,   ry+1, dark);
            Set(buf, rx+1, ry+1, dark);
        }
        // 테두리
        for (int x = 0; x < W; x++) { Set(buf, x, 0, dark); Set(buf, x, H-1, bright); }
        return buf;
    }

    // ── 계단 벽돌 (Stepped) ───────────────────────────────

    private static Color32[] PaintStepped(Color col)
    {
        var buf    = NewCanvas(col);
        var mortar = Darken(col, 0.4f);
        var top    = Lighten(col, 1.4f);
        var side   = Darken(col, 0.7f);

        // 벽돌 패턴 (offset row)
        for (int y = 0; y < H; y += 5)
        {
            for (int x = 0; x < W; x++) Set(buf, x, y, mortar);
            int offset = (y / 5 % 2) * 8;
            for (int x = offset; x < W; x += 16)
                Set(buf, x, y + 1, mortar);
        }

        // 상단 하이라이트 → 계단 느낌
        for (int x = 0; x < W; x++) Set(buf, x, H - 1, top);
        for (int x = 0; x < W; x++) Set(buf, x, H - 2, top);
        for (int x = 0; x < W; x++) Set(buf, x, 0, side);
        return buf;
    }

    // ── 스프링 / 트램폴린 (Bouncy) ───────────────────────

    private static Color32[] PaintBouncy(Color col)
    {
        var buf    = NewCanvas(col);
        var dark   = Darken(col, 0.55f);
        var bright = Lighten(col, 1.5f);
        var spring = new Color32(80, 200, 90, 255); // 스프링 녹색

        // 물결 줄무늬
        for (int x = 0; x < W; x++)
        {
            int wave = (int)(Mathf.Sin(x * 0.4f) * 2f);
            Set(buf, x, H / 2 + wave,     spring);
            Set(buf, x, H / 2 + wave + 1, spring);
        }

        // 스프링 코일 기호 (5개)
        for (int k = 0; k < 5; k++)
        {
            int sx = 5 + k * 12;
            for (int sy = 1; sy < H - 1; sy++)
            {
                int xOff = (int)(Mathf.Sin(sy * 1.2f) * 2f);
                Set(buf, sx + xOff, sy, bright);
            }
        }

        // 테두리
        for (int x = 0; x < W; x++) { Set(buf, x, 0, dark); Set(buf, x, H-1, bright); }
        return buf;
    }

    // ── 얼음 (Slippery) ───────────────────────────────────

    private static Color32[] PaintSlippery(Color col)
    {
        // 얼음: 기본 반투명 하늘색에 반짝임 크리스탈 패턴
        var iceBase = new Color(0.7f, 0.88f, 1.0f, 0.85f);
        var buf     = NewCanvas(iceBase);
        var crack   = new Color32(180, 220, 255, 200);
        var sparkle = new Color32(255, 255, 255, 255);
        var shadow  = new Color32(140, 180, 220, 255);

        // 얼음 금 (불규칙 선)
        System.Random rng = new System.Random(42);
        for (int i = 0; i < 8; i++)
        {
            int sx = rng.Next(W);
            int sy = rng.Next(H);
            int len = rng.Next(4, 12);
            int dx = rng.Next(-1, 2);
            int dy = rng.Next(-1, 2);
            for (int t = 0; t < len; t++)
                Set(buf, sx + dx * t, sy + dy * t, crack);
        }

        // 반짝임 (별 모양)
        int[] sxs = {8, 22, 38, 52};
        foreach (int spx in sxs)
        {
            int spy = H / 2;
            Set(buf, spx,   spy,   sparkle);
            Set(buf, spx-1, spy,   sparkle);
            Set(buf, spx+1, spy,   sparkle);
            Set(buf, spx,   spy-1, sparkle);
            Set(buf, spx,   spy+1, sparkle);
        }

        // 테두리
        for (int x = 0; x < W; x++) { Set(buf, x, 0, shadow); Set(buf, x, H-1, sparkle); }
        return buf;
    }

    // ── 넓은 플랫폼 타일 (Wide) ───────────────────────────

    private static Color32[] PaintWide(Color col)
    {
        var buf   = NewCanvas(col);
        var dark  = Darken(col, 0.65f);
        var light = Lighten(col, 1.3f);
        var tile2 = Darken(col, 0.85f); // 체커보드 2번째 색

        // 체커보드 타일 (8×8)
        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
            {
                bool checker = ((x / 8) + (y / 8)) % 2 == 1;
                if (checker) Set(buf, x, y, tile2);
            }

        // 타일 경계선
        for (int x = 0; x < W; x += 8)
            for (int y = 0; y < H; y++)
                Set(buf, x, y, dark);
        for (int y = 0; y < H; y += 8)
            for (int x = 0; x < W; x++)
                Set(buf, x, y, dark);

        // 상단 하이라이트
        for (int x = 0; x < W; x++) Set(buf, x, H - 1, light);
        return buf;
    }
}
