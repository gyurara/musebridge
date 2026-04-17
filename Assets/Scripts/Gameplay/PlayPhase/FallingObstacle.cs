using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어가 아래 영역에 들어오면 떨어지는 장애물
/// 무너지는 발판, 낙하 돌 등에 사용
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class FallingObstacle : ObstacleBase
{
    [Header("낙하 설정")]
    [SerializeField] private float fallDelay = 0.5f;    // 감지 후 낙하까지 대기
    [SerializeField] private float shakeIntensity = 0.05f;

    private Rigidbody2D rb;
    private bool triggered = false;
    private Vector3 originPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        originPos = transform.position;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        triggered = true;
        StartCoroutine(FallSequence());
    }

    private IEnumerator FallSequence()
    {
        // 흔들림
        float elapsed = 0f;
        while (elapsed < fallDelay)
        {
            float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
            transform.position = originPos + new Vector3(offsetX, 0, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originPos;

        // 낙하
        rb.bodyType = RigidbodyType2D.Dynamic;
    }
}
