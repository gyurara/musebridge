using UnityEngine;

/// <summary>
/// 중력 반전 영역 — 진입 시 중력 반전, 퇴장 시 복원 (비치명)
/// [FIX] ObstacleBase 상속으로 통일 (원본은 MonoBehaviour 직접 상속)
/// [FIX] originalGravityScale을 인스턴스 변수로 저장해 퇴장 시 정확히 복원
/// </summary>
public class GravityZoneObstacle : ObstacleBase
{
    private SpriteRenderer sr;
    private float pulseTimer;

    // 진입한 플레이어의 원래 gravityScale 저장 (퇴장 시 복원)
    private float originalGravityScale = 3f; // Player 프리팹 기본값
    private bool playerInside = false;

    private void Awake()
    {
        isLethal = false; // 비치명
        sr = GetComponent<SpriteRenderer>();
        var col = GetComponent<BoxCollider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        if (sr == null) return;
        pulseTimer += Time.deltaTime * 2f;
        float a = 0.2f + Mathf.Sin(pulseTimer) * 0.1f;
        var c = sr.color;
        sr.color = new Color(c.r, c.g, c.b, Mathf.Clamp(a, 0.1f, 0.4f));
    }

    // ObstacleBase의 OnTriggerEnter2D를 override (isLethal=false → OnPlayerContact 호출)
    protected override void OnPlayerContact(Collider2D player)
    {
        if (playerInside) return; // 이중 진입 방지
        playerInside = true;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 현재 gravityScale이 이미 반전된 상태일 수 있으므로 절댓값 저장
            originalGravityScale = Mathf.Abs(rb.gravityScale);
            rb.gravityScale = -originalGravityScale;
        }
        var sr2 = player.GetComponent<SpriteRenderer>();
        if (sr2 != null) sr2.flipY = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;

        var rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // [FIX] Mathf.Abs로 복원 — 퇴장 시 항상 양수(정방향 중력)로 돌아옴
            rb.gravityScale = Mathf.Abs(originalGravityScale);
        }
        var playerSr = other.GetComponent<SpriteRenderer>();
        if (playerSr != null) playerSr.flipY = false;
    }
}
