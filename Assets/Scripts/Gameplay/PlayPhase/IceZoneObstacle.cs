using UnityEngine;

/// <summary>
/// 미끄러운 얼음 바닥 영역 (비치명)
/// 영역 내 플레이어의 마찰을 줄이고 관성을 늘림
/// </summary>
public class IceZoneObstacle : MonoBehaviour
{
    [SerializeField] private float frictionMultiplier = 0.3f;

    private SpriteRenderer sr;
    private float shimmerTimer;

    // 플레이어 원래 속도 저장
    private PlayerController affectedPlayer;
    private float originalMoveSpeed;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // 반짝임 효과
        if (sr != null)
        {
            shimmerTimer += Time.deltaTime * 3f;
            float a = 0.4f + Mathf.Sin(shimmerTimer) * 0.1f;
            var c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, a);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        affectedPlayer = other.GetComponent<PlayerController>();
        if (affectedPlayer == null) return;

        // Rigidbody2D의 선형 드래그를 줄여 미끄러짐 효과
        var rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.drag = 0f;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.drag = 0f;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        // 드래그 복원
        var rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.drag = 0.5f;
        }
        affectedPlayer = null;
    }
}
