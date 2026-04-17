using UnityEngine;

/// <summary>
/// 플레이어 캐릭터 조작 (플레이 페이즈)
/// 좌우 이동 + 점프
/// 목적지 도달 시 스테이지 클리어
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;

    [Header("접지 판정")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.1f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool canControl = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void EnableControl() => canControl = true;
    public void DisableControl()
    {
        canControl = false;
        rb.velocity = Vector2.zero;
    }

    private void Update()
    {
        if (!canControl) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        float h = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(h * moveSpeed, rb.velocity.y);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    // ── 낙사 / 목적지 충돌 처리 ──────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Goal"))
        {
            DisableControl();
            PlayPhaseManager.Instance?.OnPlayerReachedGoal();
        }
        else if (other.CompareTag("DeathZone"))
        {
            DisableControl();
            PlayPhaseManager.Instance?.OnPlayerFell();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
