using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 판정선(리듬 라인) 제어
/// 왼쪽에서 오른쪽으로 이동하며, 현재 X 위치를 외부에 제공
/// </summary>
public class RhythmLineController : MonoBehaviour
{
    public static RhythmLineController Instance { get; private set; }

    [Header("이동 설정")]
    [SerializeField] private float startX = -10f;
    [SerializeField] private float endX = 10f;
    private float speed;

    public bool IsMoving { get; private set; } = false;
    public float CurrentX => transform.position.x;

    // 판정선이 끝에 도달했을 때 발생
    public UnityEvent OnLineReachedEnd = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Initialize(float lineSpeed, float stageWidth)
    {
        speed = lineSpeed;
        startX = -stageWidth / 2f;
        endX = stageWidth / 2f;
        transform.position = new Vector3(startX, transform.position.y, 0);
    }

    public void StartMoving()
    {
        IsMoving = true;
    }

    public void StopMoving()
    {
        IsMoving = false;
    }

    private void Update()
    {
        if (!IsMoving) return;

        transform.Translate(Vector3.right * speed * Time.deltaTime);

        if (transform.position.x >= endX)
        {
            transform.position = new Vector3(endX, transform.position.y, 0);
            IsMoving = false;
            OnLineReachedEnd?.Invoke();
        }
    }

    /// <summary>
    /// 현재 판정선 위치를 월드 좌표 Vector3로 반환
    /// </summary>
    public Vector3 GetCurrentWorldPosition() => transform.position;
}
