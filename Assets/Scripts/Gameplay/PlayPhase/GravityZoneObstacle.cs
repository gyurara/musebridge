using UnityEngine;

/// <summary>
/// 중력 반전 영역 — 진입 시 중력 반전, 퇴장 시 복원 (비치명)
/// </summary>
public class GravityZoneObstacle : MonoBehaviour
{
    private SpriteRenderer sr;
    private float pulseTimer;
    private float originalGravityScale;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        var col = GetComponent<BoxCollider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        if (sr != null)
        {
            pulseTimer += Time.deltaTime * 2f;
            float a = 0.2f + Mathf.Sin(pulseTimer) * 0.1f;
            var c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, a);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            originalGravityScale = rb.gravityScale;
            rb.gravityScale = -originalGravityScale;
            var playerSr = other.GetComponent<SpriteRenderer>();
            if (playerSr != null) playerSr.flipY = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = Mathf.Abs(originalGravityScale);
            var playerSr = other.GetComponent<SpriteRenderer>();
            if (playerSr != null) playerSr.flipY = false;
        }
    }
}
