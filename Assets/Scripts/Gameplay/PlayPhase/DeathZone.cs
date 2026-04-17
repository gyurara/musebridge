using UnityEngine;

/// <summary>
/// 낙사 구역
/// 화면 아래 또는 특정 위험 영역에 배치
/// Tag: "DeathZone" 필요
/// </summary>
public class DeathZone : MonoBehaviour
{
    private void Awake()
    {
        gameObject.tag = "DeathZone";
        // 콜라이더를 트리거로 설정
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        PlayPhaseManager.Instance?.OnPlayerFell();
    }
}
