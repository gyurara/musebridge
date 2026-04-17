using UnityEngine;

/// <summary>
/// 주기적으로 열리고 닫히는 문 장애물 (치명)
/// 닫혀 있을 때 충돌체 활성화, 열려 있을 때 통과 가능
/// </summary>
public class TimedGateObstacle : MonoBehaviour
{
    [SerializeField] private float openTime = 2f;
    [SerializeField] private float closeTime = 2f;

    private BoxCollider2D col;
    private SpriteRenderer sr;
    private float timer;
    private bool isOpen;

    private Color closedColor = new Color(0.4f, 0.4f, 0.5f, 1f);
    private Color openColor = new Color(0.4f, 0.4f, 0.5f, 0.15f);

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        if (col != null) col.isTrigger = false;
    }

    public void Configure(float open, float close)
    {
        openTime = open;
        closeTime = close;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float cycleTime = isOpen ? openTime : closeTime;

        if (timer >= cycleTime)
        {
            timer -= cycleTime;
            isOpen = !isOpen;

            if (col != null) col.enabled = !isOpen;
            if (sr != null) sr.color = isOpen ? openColor : closedColor;
        }

        // 닫히기 직전 경고 깜빡임
        if (isOpen && timer > openTime - 0.5f)
        {
            float blink = Mathf.Sin(timer * 20f) > 0 ? 0.4f : 0.15f;
            if (sr != null) sr.color = new Color(0.4f, 0.4f, 0.5f, blink);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        // 닫힌 문에 부딪히면 넉백
        var rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 dir = (collision.transform.position - transform.position).normalized;
            rb.velocity = dir * 5f;
        }
    }
}
