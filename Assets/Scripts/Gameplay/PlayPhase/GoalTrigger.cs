using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 스테이지 목적지 트리거
/// 플레이어가 도달하면 클리어 처리
/// Tag: "Goal" 필요
/// </summary>
public class GoalTrigger : MonoBehaviour
{
    [SerializeField] private ParticleSystem celebrationParticle;
    public UnityEvent OnGoalReached = new();

    private bool reached = false;

    private void Awake()
    {
        gameObject.tag = "Goal";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (reached || !other.CompareTag("Player")) return;
        reached = true;

        celebrationParticle?.Play();
        OnGoalReached?.Invoke();
        PlayPhaseManager.Instance?.OnPlayerReachedGoal();
    }

    /// <summary>
    /// 스테이지 재시작 시 리셋
    /// </summary>
    public void Reset()
    {
        reached = false;
    }
}
