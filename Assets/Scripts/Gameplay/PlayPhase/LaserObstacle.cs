using UnityEngine;

/// <summary>
/// 주기적으로 활성/비활성되는 레이저 빔 (치명)
/// 활성 시 빨간 빔 + 콜라이더, 비활성 시 희미한 라인
/// </summary>
public class LaserObstacle : MonoBehaviour
{
    [SerializeField] private float onTime = 1.5f;
    [SerializeField] private float offTime = 2f;

    private BoxCollider2D col;
    private SpriteRenderer sr;
    private float timer;
    private bool isActive = true;

    private Color activeColor = new Color(1f, 0.1f, 0.1f, 0.8f);
    private Color inactiveColor = new Color(1f, 0.3f, 0.3f, 0.1f);

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void Configure(float on, float off)
    {
        onTime = on;
        offTime = off;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float cycleTime = isActive ? onTime : offTime;

        if (timer >= cycleTime)
        {
            timer -= cycleTime;
            isActive = !isActive;

            if (col != null) col.enabled = isActive;
            if (sr != null) sr.color = isActive ? activeColor : inactiveColor;
        }

        // 활성화 직전 경고: 빠른 깜빡임
        if (!isActive && timer > offTime - 0.6f)
        {
            float blink = Mathf.Sin(timer * 25f) > 0 ? 0.5f : 0.1f;
            if (sr != null) sr.color = new Color(1f, 0.2f, 0.2f, blink);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        if (!other.CompareTag("Player")) return;

        // 플레이어가 조작 가능한 상태일 때�� 사망 처리
        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        player.DisableControl();
        PlayPhaseManager.Instance?.OnPlayerFell();
    }
}
