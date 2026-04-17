using UnityEngine;

/// <summary>
/// 장애물 기본 클래스
/// 모든 장애물은 이 클래스를 상속
/// </summary>
public abstract class ObstacleBase : MonoBehaviour
{
    [Header("공통")]
    [SerializeField] protected bool isLethal = true; // 닿으면 낙사 처리

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isLethal)
            PlayPhaseManager.Instance?.OnPlayerFell();
        else
            OnPlayerContact(other);
    }

    /// <summary>
    /// 치명적이지 않은 장애물의 플레이어 접촉 처리 (하위 클래스에서 재정의)
    /// </summary>
    protected virtual void OnPlayerContact(Collider2D player) { }
}
