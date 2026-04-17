using UnityEngine;

/// <summary>
/// 트램폴린 장애물 — 플레이어를 위로 튕겨 올린다 (비치명)
/// </summary>
public class BouncerObstacle : MonoBehaviour
{
    [SerializeField] private float bounceForce = 15f;

    private SpriteRenderer sr;
    private Vector3 baseScale;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    public void Configure(float force)
    {
        bounceForce = force;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        Bounce(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Bounce(other.gameObject);
    }

    private void Bounce(GameObject player)
    {
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        rb.velocity = new Vector2(rb.velocity.x, bounceForce);

        // 시각 피드백: 눌렸다가 복원
        StopAllCoroutines();
        StartCoroutine(SquashAndStretch());
    }

    private System.Collections.IEnumerator SquashAndStretch()
    {
        // 눌림
        transform.localScale = new Vector3(baseScale.x * 1.3f, baseScale.y * 0.5f, baseScale.z);
        yield return new WaitForSeconds(0.1f);
        // 복원
        float t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(
                new Vector3(baseScale.x * 1.3f, baseScale.y * 0.5f, baseScale.z),
                baseScale, t / 0.2f);
            yield return null;
        }
        transform.localScale = baseScale;
    }
}
