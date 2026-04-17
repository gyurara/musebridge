using UnityEngine;

/// <summary>
/// 바람 구역: 플레이어에게 지속적인 방향 힘을 가하는 비치명적 장애물
/// 악기 테마에 맞게 바람 → 음표가 흩날리는 느낌
/// ※ Unity 내장 WindZone 컴포넌트와 이름 충돌 방지를 위해 WindZoneObstacle로 명명
/// </summary>
public class WindZoneObstacle : ObstacleBase
{
    [Header("바람 설정")]
    [SerializeField] private Vector2 windForce = new Vector2(3f, 0f);
    [SerializeField] private ParticleSystem windParticles; // 선택 사항

    private Rigidbody2D playerRb;

    public void Configure(Vector2 force) => windForce = force;

    private void Awake()
    {
        isLethal = false; // 치명적이지 않음
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerRb = other.GetComponent<Rigidbody2D>();
        windParticles?.Play();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || playerRb == null) return;
        playerRb.AddForce(windForce * Time.fixedDeltaTime, ForceMode2D.Force);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerRb = null;
        windParticles?.Stop();
    }
}
